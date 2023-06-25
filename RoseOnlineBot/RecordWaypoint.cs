using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using RoseOnlineBot.Models.Logic;

namespace RoseOnlineBot
{
    class RecordWaypoint
    {        
        System.Timers.Timer timer;
        public List<WayPoint> waypoints;
        FormMain main;
        public RecordWaypoint(FormMain _main)
        {
            main = _main;
        }
        public void startRecordingWay()
        {
            waypoints = new List<WayPoint>();
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            WayPoint wayp = new WayPoint();
            wayp.No = waypoints.Count;

            //aktuelle Coordsauslesen
            wayp.CoordX = GameData.Player.PosX;
            wayp.CoordY = GameData.Player.PosY;
            //wenn charr sich nicht bewegt hat
            if (waypoints.Count == 0 || (waypoints.Count > 0 && waypoints[waypoints.Count - 1].CoordX != wayp.CoordX && waypoints[waypoints.Count - 1].CoordY != wayp.CoordY))
            {
                waypoints.Add(wayp);
                main.rtbWaypoints.Invoke(new MethodInvoker(delegate ()
                {
                    main.rtbWaypoints.Text += "X Coord: " + wayp.CoordX.ToString() + " Y Coord: " + wayp.CoordY + " Waypoint Number: " + wayp.No + "\r\n";
                }));
            }
            timer.Start();
        }
        public void stopRecordingWay()
        {
            timer.Stop();
            timer.Close();
        }
    }
}
