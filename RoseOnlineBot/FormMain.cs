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
using RoseOnlineBot.Classes;
using RoseOnlineBot.Models.Logic;
using System.Xml.Serialization;
using System.Runtime;
using RoseOnlineBot.Models.Metadata;

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

        public static void saveWaypoints(List<WayPoint> Waypoints, string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<WayPoint>));
            TextWriter textWriter = new StreamWriter(fileName);
            serializer.Serialize(textWriter, Waypoints);
            textWriter.Close();
        }
        public static List<WayPoint> readWaypoints(string fileName)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<WayPoint>));
            TextReader textReader = new StreamReader(fileName);
            List<WayPoint> Waypoints;
            Waypoints = (List<WayPoint>)deserializer.Deserialize(textReader);
            textReader.Close();

            return Waypoints;
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
                saveWaypoints(rw.waypoints, dateiname);
                btnSaveWay.Enabled = false;
                btnStopRecord.Enabled = false;
                btnRecordWay.Enabled = true;
            }

        }






        private void button2_Click(object sender, EventArgs e)
        {
            GameData.Player.TurnInQuest();
            GameData.Player.AcceptQuest();
            //var data = GameData.Player.GetInventory();
            //GameData.Player.UseItem(data.Consumabes[3].DBId);
            var combat = new Combat();
            var waypoints = readWaypoints(txWaypointPath.Text);
            foreach(var waypoint in waypoints)
            {
                combat.MoveToCoordinate(waypoint.CoordX, waypoint.CoordY, false);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }


        private System.Timers.Timer hpCheckTimer;
        private void HpCheckTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            hpCheckTimer.Stop();
            if (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.4f) // low on hp 
            {
                var usableItems = GameData.UsableRecoveryItems;
                var inventory = GameData.Player.GetInventory();


                var items = inventory.Consumables.Where(x => usableItems.Any(y => y.ItemId == x.ItemId && y.ItemType == ItemType.Food)).Select(x => new { inventoryItem = x, Metadata = usableItems.First(y => y.ItemId == x.ItemId) }).ToArray();
                if (items.Length > 0)
                {
                    var bestItem = items.OrderByDescending(x => x.Metadata.RestoreAmount).First();
                    var recoveryAmount = bestItem.Metadata.RestoreAmount * 20;
                    GameData.Player.UseItem(bestItem.inventoryItem.DBId);
                    Thread.Sleep(20000); // status effect duration
                }
            }
            if(hpCheckTimer != null)
                hpCheckTimer.Start();
        }

        
        Thread combatThread = null;
        private void button1_Click(object sender, EventArgs e)
        {
         
            if (hpCheckTimer == null)
            {
                hpCheckTimer = new System.Timers.Timer();
                hpCheckTimer.Interval = 50;
                hpCheckTimer.Enabled = true;
                hpCheckTimer.Elapsed += HpCheckTimer_Elapsed;
            }

            if(combatThread == null)
            {
                if (GameData.Player.PartyMode == true)
                    combatThread = new Thread(new ThreadStart(new Combat().PartyMode));
                else
                    combatThread = new Thread(new ThreadStart(new Combat().SingleTargetMode));
                combatThread.Start();
                hpCheckTimer.Start();

                button1.Text = "Stop";
            }
            else
            {
                combatThread.Interrupt();
                hpCheckTimer.Stop();
                hpCheckTimer = null;
                button1.Text = "Start Bot";
                combatThread = null;
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GameData.Player.GetQuestProgess();
        }
    }
}
