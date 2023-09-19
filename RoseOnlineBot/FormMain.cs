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
using System.Numerics;

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
            rtbWaypoints.Text = "";
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
            //var anim = GameData.Player.CurrentAnimation;
            //Thread.Sleep(100);
            //GameData.Player.UseCart();
            //Thread.Sleep(100);
            var combat = new Combat();

            // return
            var waypoints = readWaypoints(@"C:\Users\Rainson\Documents\RoseOnBot\plants_spot_3_solo\return_to_town.xml");
            //foreach (var waypoint in waypoints)
            //{
            //    combat.MoveToCoordinate(waypoint.CoordX, waypoint.CoordY, false, false);
            //}


            //q1 pincer claws
            GameData.Player.TurnInQuest(new byte[] { 0x17, 0x6a, 0x77, 0x3b });
            Thread.Sleep(50);
            GameData.Player.AcceptQuest(new byte[] { 0x4e, 0xd6, 0x74, 0x3b });
            Thread.Sleep(50);

            //q2 nephentess pollen
            GameData.Player.TurnInQuest(new byte[] { 0xef, 0x65, 0xb3, 0x55 });
            Thread.Sleep(50);
            GameData.Player.AcceptQuest(new byte[] { 0xb6, 0xd9, 0xb0, 0x55 });
            Thread.Sleep(50);

            // move to second npc
            waypoints = readWaypoints(@"C:\Users\Rainson\Documents\RoseOnBot\plants_spot_q\town_move_to_q3.xml");
            foreach (var waypoint in waypoints)
            {
                combat.MoveToCoordinate(waypoint.CoordX, waypoint.CoordY, false, false);
            }


            Thread.Sleep(50);
            // q3 stockpilling high quality pollon
            GameData.Player.TurnInQuest(new byte[] { 0x98, 0xc9, 0x5f, 0xb6 });
            Thread.Sleep(50);
            GameData.Player.AcceptQuest(new byte[] { 0xc1, 0x75, 0x5c, 0xb6 });
            Thread.Sleep(50);


            // move to third npc
            waypoints = readWaypoints(@"C:\Users\Rainson\Documents\RoseOnBot\plants_spot_q\town_move_to_q4.xml");
            foreach (var waypoint in waypoints)
            {
                combat.MoveToCoordinate(waypoint.CoordX, waypoint.CoordY, false, false);
            }

            // q4 Pollon Information research
            GameData.Player.TurnInQuest(new byte[] { 0x70, 0xde, 0x8b, 0x45 });
            Thread.Sleep(50);
            GameData.Player.AcceptQuest(new byte[] { 0x29, 0x62, 0x88, 0x45 });
            Thread.Sleep(50);

            // move to thourth
            waypoints = readWaypoints(@"C:\Users\Rainson\Documents\RoseOnBot\plants_spot_q\town_move_to_q5.xml");
            foreach (var waypoint in waypoints)
            {
                combat.MoveToCoordinate(waypoint.CoordX, waypoint.CoordY, false, false);
            }
            // q5 Improved weapon research
            GameData.Player.TurnInQuest(new byte[] { 0x3a, 0xb6, 0x23, 0x03 });
            Thread.Sleep(50);
            GameData.Player.AcceptQuest(new byte[] { 0x63, 0x0a, 0x20, 0x03 });
            Thread.Sleep(50);

            waypoints = readWaypoints(@"C:\Users\Rainson\Documents\RoseOnBot\plants_spot_3_solo\to_mobs.xml");
            //foreach (var waypoint in waypoints)
            //{
            //    combat.MoveToCoordinate(waypoint.CoordX, waypoint.CoordY, false, false);
            //}
            //GameData.Player.UseCart();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void FormMain_Load(object sender, EventArgs e)
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
            if (hpCheckTimer != null)
                hpCheckTimer.Start();
        }

        private System.Timers.Timer hpPotionCheckTimer;
        private void HpPotionCheckTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            hpPotionCheckTimer.Stop();
            if (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.3f) // low on hp 
            {
                var usableItems = GameData.UsableRecoveryItems;
                var inventory = GameData.Player.GetInventory();


                var items = inventory.Consumables.Where(x => usableItems.Any(y => y.ItemId == x.ItemId && y.ItemType == ItemType.HPPotion)).Select(x => new { inventoryItem = x, Metadata = usableItems.First(y => y.ItemId == x.ItemId) }).ToArray();
                if (items.Length > 0)
                {
                    var bestItem = items.OrderByDescending(x => x.Metadata.RestoreAmount).First();
                    var recoveryAmount = bestItem.Metadata.RestoreAmount * 20;
                    GameData.Player.UseItem(bestItem.inventoryItem.DBId);
                    Thread.Sleep(20000); // status effect duration
                }
            }
            if (hpPotionCheckTimer != null)
                hpPotionCheckTimer.Start();
        }


        Thread combatThread = null;
        private void button1_Click(object sender, EventArgs e)
        {
            GameData.Player.Targets = new List<NpcEntity>();
            if (hpCheckTimer == null)
            {
                hpCheckTimer = new System.Timers.Timer();
                hpCheckTimer.Interval = 50;
                hpCheckTimer.Enabled = true;
                hpCheckTimer.Elapsed += HpCheckTimer_Elapsed;
            }
            if (hpPotionCheckTimer == null)
            {
                hpPotionCheckTimer = new System.Timers.Timer();
                hpPotionCheckTimer.Interval = 50;
                hpPotionCheckTimer.Enabled = true;
                hpPotionCheckTimer.Elapsed += HpPotionCheckTimer_Elapsed;
            }

            if (combatThread == null)
            {
                var mode = drpMode.SelectedItem.ToString();
                GameData.Player.PartyMode = mode.StartsWith("Party");
                switch (mode)
                {
                    case "SingleTarget":
                        combatThread = new Thread(new ThreadStart(new Combat().SingleTargetMode));
                        break;
                    case "PartyAOE":
                        combatThread = new Thread(new ThreadStart(new Combat().PartyMode));
                        break;
                    case "PartySingleTarget":
                        combatThread = new Thread(new ThreadStart(new Combat().PartyModeSingleTarget));
                        break;
                    default:
                        break;
                }
                            
                combatThread.Start();
                hpCheckTimer.Start();

                button1.Text = "Stop";
            }
            else
            {
                combatThread.Interrupt();
                hpPotionCheckTimer.Stop();
                hpPotionCheckTimer = null;
                hpCheckTimer.Stop();
                hpCheckTimer = null;
                button1.Text = "Start Bot";
                combatThread = null;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var inv = GameData.Player.GetInventory();
        }
        List<ushort> capturedPlayersIds = new List<ushort>();
        private void button4_Click(object sender, EventArgs e)
        {
            var players = GameData.Player.GetPlayers();

            foreach (var player in players)
            {
                if (!capturedPlayersIds.Contains(player.DBId))
                {
                    GameData.Player.OpenShop(player.DBId);
                    GameData.Player.LastOpenedShopXCoord = player.PosX;
                    GameData.Player.LastOpenedShopYCoord = player.PosY;
                    capturedPlayersIds.Add(player.DBId);
                    Thread.Sleep(500);
                }
                else
                {

                }
            }
            var inventory = GameData.Player.GetInventory();
            foreach (var item in inventory.Consumables)
            {
                var prices = GameData.StorePrices.Where(x => x.ItemId == item.ItemId).OrderByDescending(item => item.Price).ToList();

            }
            foreach (var item in inventory.Materials)
            {
                var prices = GameData.StorePrices.Where(x => x.ItemId == item.ItemId).OrderByDescending(item => item.Price).ToList();

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var combat = new Combat();
            float x = float.Parse(tbXCoord.Text.Replace(".", ","));
            float y = float.Parse(tbYCoord.Text.Replace(".", ","));
            combat.MoveToCoordinate(x, y, false);
        }
    }
}
