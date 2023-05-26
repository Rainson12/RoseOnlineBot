using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using RoseOnlineBot.Win32;
using RoseOnlineBot.Utils;
using ProcessMemoryUtilities.Managed;
using GameOffsets;
using RoseOnlineBot.Business;

namespace RoseOnlineBot
{
    public unsafe partial class FormMain : Form
    {

        private Communication pipeServer;

        /// <summary>
        ///     Gets the game handle.
        /// </summary>
        internal Memory Handle { get; private set; }


        private void btnStart_Click(object sender, EventArgs e)
        {
            // check if process is running
            Process[] p = Process.GetProcessesByName("Trose");
            Process selectedProcess = null;
            if (p.Length > 1)
            {
                SelectProcess selectProcess = new SelectProcess(p);
                if (selectProcess.ShowDialog() == DialogResult.OK)
                    selectedProcess = Process.GetProcessById(int.Parse(selectProcess.comboBox1.SelectedItem.ToString()));
            }
            else if (p.Length > 0)
                selectedProcess = p[0];
            if (selectedProcess != null)
            {
                pipeServer = new Communication(@"myRosePipe" + selectedProcess.Id);
                pipeServer.Start();



                Thread.Sleep(1000);
                GameData.Init(selectedProcess, pipeServer);

                //Main _main = new Main(myProcess, pipeServer, txWaypointPath.Text);
                //Thread main = new Thread(new ThreadStart(_main.start));
                //Thread.Sleep(1000);
                //main.Start();
            }
            else
            {
                MessageBox.Show("Please start Rose Online first");
            }
        }

        private void EjectIfAlreadyInjected(Process myProcess)
        {
            Injector injector = new Injector(myProcess);
            myProcess = Process.GetProcessById(myProcess.Id);
            foreach (ProcessModule mod in myProcess.Modules)
            {
                if (mod.ModuleName == "rBotMagic.dll")
                {
                    injector.EjectLibrary(@"C:\develop\rBotMagic\x64\Debug\rBotMagic.dll", mod);
                    break;
                }
            }
        }

        private void btnBrowseWaypoint_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open Wayoints";
            fdlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            fdlg.FilterIndex = 1;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txWaypointPath.Text = fdlg.FileName;
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            Process[] p = Process.GetProcessesByName("Trose");
            foreach (var process in p)
            {
                EjectIfAlreadyInjected(process);
            }

        }
        public FormMain()
        {
            InitializeComponent();
        }

        RecordWaypoint rw;
        private void btnRecordWay_Click(object sender, EventArgs e)
        {
            Process[] p = Process.GetProcessesByName("Trose");
            Process myProcess = null;
            if (p.Length > 1)
            {
                SelectProcess selectProcess = new SelectProcess(p);
                if (selectProcess.ShowDialog() == DialogResult.OK)
                    myProcess = Process.GetProcessById(int.Parse(selectProcess.comboBox1.SelectedItem.ToString()));
            }
            else if (p.Length > 0)
                myProcess = p[0];
            rw = new RecordWaypoint(this);
            rw.startRecordingWay();
            btnStopRecord.Enabled = true;
            btnRecordWay.Enabled = false;
        }

        private void btnStopRecord_Click(object sender, EventArgs e)
        {
            rw.stopRecordingWay();
            btnSaveWay.Enabled = true;
            btnStopRecord.Enabled = false;
            btnRecordWay.Enabled = false;
        }

        private void btnSaveWay_Click(object sender, EventArgs e)
        {
            string dateiname = "";
            SaveFileDialog diag = new SaveFileDialog();
            diag.DefaultExt = ".xml";
            diag.Title = "Open Wayoints";
            diag.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            diag.FilterIndex = 1;
            diag.RestoreDirectory = true;
            if (DialogResult.OK == diag.ShowDialog())
            {
                dateiname = diag.FileName;
                //memFunc.saveWaypoints(rw.waypoints, dateiname);
                btnSaveWay.Enabled = false;
                btnStopRecord.Enabled = false;
                btnRecordWay.Enabled = true;
            }

        }


        
        
        private void button1_Click(object sender, EventArgs e)
        {
            var startingPointX = GameData.Player.PosX;
            var startingPointY = GameData.Player.PosX;
            while (true)
            {
                if(Vector2D.CalculateDistance(startingPointX, startingPointY, GameData.Player.PosX, GameData.Player.PosX) >= 600)
                {
                    GameData.Player.Move(startingPointX, startingPointY);
                }
                if (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.5f)
                {
                    GameData.Player.ToggleSit();
                    Thread.Sleep(500);
                    while (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 1.0f)
                        Thread.Sleep(100);
                    GameData.Player.ToggleSit();
                    Thread.Sleep(1000);
                }
                

                if (GameData.Player.Targets.Count == 0 && GameData.Player.FindNextTarget() is NpcEntity newTarget)
                {
                    GameData.Player.Targets.Add(newTarget.Id);
                }

                for (int i = 0; i < GameData.Player.Targets.Count; i++)
                {
                    var mobs = GameData.Player.GetMobs();
                    var targetMob = mobs.FirstOrDefault(x => x.Id ==  GameData.Player.Targets[i]);
                    if (targetMob == null)
                    {
                        GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                        i--;
                    }
                    else
                    {
                        bool firstAttack = true;
                        while (targetMob.HP> 0)
                        {
                            mobs = GameData.Player.GetMobs();
                            targetMob = mobs.FirstOrDefault(x => x.Id == GameData.Player.Targets[i]);
                            if (targetMob == null)
                            {
                                GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                i--;
                                break;
                            }
                            if(firstAttack)
                            {
                                firstAttack = false;
                                
                            }
                            GameData.Player.TargetId = targetMob.Id;

                            GameData.Player.AttackTarget(targetMob.DBId);
                            Thread.Sleep(100);
                            
                            //GameData.Player.Move(targetMob.PosX, targetMob.PosY);
                            //GameData.Player.AttackTarget(targetMob.DBId);
                            //Thread.Sleep(100);
                            //GameData.Player.CastSpellOnTarget(targetMob.DBId, 0x211);
                            //Thread.Sleep(100);
                            GameData.Player.CastSpellOnTarget(targetMob.DBId, 0x210);
                            Thread.Sleep(3800);
                            //GameData.Player.AttackTarget(targetMob.DBId);
                            //Thread.Sleep(100);
                            if(targetMob.HP < 0)
                            {
                                GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                i--;
                            }
                        }
                        
                    }
                }
                Thread.Sleep(10);
            }
        }
    }
}
