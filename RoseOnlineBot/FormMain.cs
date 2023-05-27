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
                EjectIfAlreadyInjected(selectedProcess);
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
            bool found = false;
            do
            {
                found = false;
                Injector injector = new Injector(myProcess);
                myProcess = Process.GetProcessById(myProcess.Id);

                foreach (ProcessModule mod in myProcess.Modules)
                {
                    if (mod.ModuleName == "rBotMagic.dll")
                    {
                        injector.EjectLibrary(@"C:\develop\rBotMagic\x64\Debug\rBotMagic.dll", mod);
                        found = true;
                        break;
                    }
                }
            }
            while (found);
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
            //GameData.Player.SendJoinZone();
            var startingPointX = GameData.Player.PosX;
            var startingPointY = GameData.Player.PosY;
            while (true)
            {
                if (Vector2D.CalculateDistance(startingPointX, startingPointY, GameData.Player.PosX, GameData.Player.PosY) >= 2000 && GameData.Player.Targets.Count == 0)
                {
                    GameData.Player.Move(startingPointX, startingPointY);
                    while (Vector2D.CalculateDistance(startingPointX, startingPointY, GameData.Player.PosX, GameData.Player.PosY) >= 100 && GameData.Player.Targets.Count == 0) // run back to starting point
                    {
                        Thread.Sleep(50);
                        if (GameData.Player.CurrentAnimation != Models.Logic.Animation.Run)
                            GameData.Player.Move(startingPointX, startingPointY);
                    }
                }
                if (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.5f && GameData.Player.Targets.Count == 0)
                {
                    while(GameData.Player.CurrentAnimation != Models.Logic.Animation.Stand)
                    {
                        Thread.Sleep(100);
                    }
                    

                    GameData.Player.ToggleSit();
                    Thread.Sleep(500);
                    while (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 1.0f && GameData.Player.Targets.Count == 0)
                    {
                        if(GameData.Player.CurrentAnimation == Models.Logic.Animation.Stand)
                        {
                            GameData.Player.ToggleSit();
                            Thread.Sleep(500);
                        }
                        Thread.Sleep(100);
                    }
                        
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
                    var targetMob = mobs.FirstOrDefault(x => x.Id == GameData.Player.Targets[i]);
                    if (targetMob == null)
                    {
                        GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                        i--;
                    }
                    else
                    {
                        while (targetMob.HP > 0)
                        {
                            mobs = GameData.Player.GetMobs();
                            targetMob = mobs.FirstOrDefault(x => x.Id == GameData.Player.Targets[i]);
                            if (targetMob == null)
                            {
                                GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                i--;
                                break;
                            }
                            GameData.Player.TargetId = targetMob.Id;

                            if (GameData.Player.CurrentAnimation == 0) // When standing - just attack
                            {
                                GameData.Player.AttackTarget(targetMob.DBId);
                                Thread.Sleep(300);
                                if (GameData.Player.CurrentAnimation == 0)
                                {
                                    // cant reach target
                                    for (int x = 0; x < 10; x++)
                                    {
                                        GameData.Player.AttackTarget(targetMob.DBId);
                                        Thread.Sleep(100);
                                        if (GameData.Player.CurrentAnimation != 0)
                                        {
                                            break;
                                        }
                                    }
                                    if (GameData.Player.CurrentAnimation == 0)
                                    {
                                        // ignore target
                                        GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                        i--;
                                        break;
                                    }
                                }
                            }

                            foreach (var skill in GameData.Player.Skills)
                            {
                                if (skill.Enabled && skill.ManaCost < GameData.Player.MP && !skill.IsOnCooldown)
                                {
                                    // Get Targets in Range for AOE to not kill myself
                                    if (skill.IsAOE)
                                    {
                                        var mobsInRange = mobs.Count(x => x.HP > 0 && Vector2D.CalculateDistance(GameData.Player.PosX, GameData.Player.PosY, x.PosX, x.PosY) < skill.Range);
                                        if (mobsInRange == 0 || mobsInRange > 2)
                                            continue;
                                    }
                                    if (skill.IsAOE)
                                        GameData.Player.CastSpellOnMySelf(skill.Slot);
                                    else
                                        GameData.Player.CastSpellOnTarget(targetMob.DBId, skill.Slot);
                                    break;
                                }
                            }
                            while (
                                (GameData.Player.CurrentAnimation == Models.Logic.Animation.ExecutingSkill || // while executing the skill
                                GameData.Player.CurrentAnimation == Models.Logic.Animation.PrepareExecutingSkill) &&
                                (GameData.Player.CurrentAnimation != Models.Logic.Animation.Stand || // and not standing or basic attacking
                                GameData.Player.CurrentAnimation != Models.Logic.Animation.Attack)
                                )
                            {
                                Thread.Sleep(500);
                            }
                            GameData.Player.WaitingForSkillExecution = false;


                            if (targetMob.HP < 0)
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

        private void button2_Click(object sender, EventArgs e)
        {
            GameData.Player.GetInventory();
        }
    }
}
