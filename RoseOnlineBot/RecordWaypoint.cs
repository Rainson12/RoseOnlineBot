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
            timer.Interval = 3000;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            WayPoint wayp = new WayPoint();
            //wayp.waynr = waypoints.Count;

            // aktuelle Coordsauslesen
            //wayp.coordX = charakter.x_pos;
            //wayp.coordY = charakter.y_pos;
            // wenn charr sich nicht bewegt hat
            //if (waypoints.Count > 0 && waypoints[waypoints.Count - 1].coordX != wayp.coordX && waypoints[waypoints.Count - 1].coordY != wayp.coordY)
            //{
            //    waypoints.Add(wayp);
            //    main.rtbWaypoints.Invoke(new MethodInvoker(delegate()
            //    {
            //        main.rtbWaypoints.Text += "X Coord: " + wayp.coordX.ToString() + " Y Coord: " + wayp.coordY + " Waypoint Number: " + wayp.waynr + "\r\n";
            //    }));
            //}
            //else if (waypoints.Count == 0)
            //{
            //    waypoints.Add(wayp);
            //    main.rtbWaypoints.Invoke(new MethodInvoker(delegate()
            //    {
            //        main.rtbWaypoints.Text += "X Coord: " + wayp.coordX.ToString() + " Y Coord: " + wayp.coordY + " Waypoint Number: " + wayp.waynr + "\r\n";
            //    }));
            //}
            
            //data.RecordedWaypoints = waypoints;
            timer.Start();
        }
        public void stopRecordingWay()
        {
            timer.Stop();
            timer.Close();
        }
    }
}
