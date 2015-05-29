using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MissionPlanner.Controls
{
    public partial class SITL : Form
    {
        Uri sitlurl = new Uri("http://firmware.diydrones.com/Tools/MissionPlanner/sitl/");
        string sitldirectory = Application.StartupPath + Path.DirectorySeparatorChar + "sitl" + Path.DirectorySeparatorChar;

        GMapOverlay markeroverlay;

        /*
        { "+",         MultiCopter::create },
    { "quad",      MultiCopter::create },
    { "copter",    MultiCopter::create },
    { "x",         MultiCopter::create },
    { "hexa",      MultiCopter::create },
    { "octa",      MultiCopter::create },
    { "heli",      Helicopter::create },
    { "rover",     Rover::create },
    { "crrcsim",   CRRCSim::create },
    { "jsbsim",    JSBSim::create },
    { "last_letter", last_letter::create }
             */

        ///tmp/.build/ArduCopter.elf -M+ -O-34.98106,117.85201,40,0 
        ///tmp/.build/APMrover2.elf -Mrover -O-34.98106,117.85201,40,0 
        ///tmp/.build/ArduPlane.elf -Mjsbsim -O-34.98106,117.85201,40,0 --autotest-dir ./
        ///tmp/.build/ArduCopter.elf -Mheli -O-34.98106,117.85201,40,0 


        public SITL()
        {
            InitializeComponent();

            if (!Directory.Exists(sitldirectory))
                Directory.CreateDirectory(sitldirectory);

            myGMAP1.MapProvider = /*GCSViews.FlightData.mymap.MapProvider */ GMapProviders.GoogleSatelliteMap;

            markeroverlay = new GMapOverlay("markers");
            myGMAP1.Overlays.Add(markeroverlay);

            markeroverlay.Markers.Add(new GMapMarkerWP(new PointLatLng(-34.98106,117.85201), "H"));

            MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);
        }

        private void pictureBoxplane_Click(object sender, EventArgs e)
        {
            var exepath = CheckandGetSITLImage("ArduPlane.elf");

            StartSITL(exepath, "jsbsim", BuildHomeLocation(markeroverlay.Markers[0].Position), 1);
        }

        private void pictureBoxrover_Click(object sender, EventArgs e)
        {
            var exepath = CheckandGetSITLImage("APMrover2.elf");

            StartSITL(exepath, "rover", BuildHomeLocation(markeroverlay.Markers[0].Position), 1);
        }

        private void pictureBoxquad_Click(object sender, EventArgs e)
        {
            var exepath = CheckandGetSITLImage("ArduCopter.elf");

            StartSITL(exepath, "+", BuildHomeLocation(markeroverlay.Markers[0].Position), 1);
        }

        private void pictureBoxheli_Click(object sender, EventArgs e)
        {
            var exepath = CheckandGetSITLImage("ArduHeli.elf");

            StartSITL(exepath, "heli", BuildHomeLocation(markeroverlay.Markers[0].Position), 1);
        }

        string BuildHomeLocation(PointLatLng homelocation, int heading = 0)
        {
            return String.Format("{0},{1},{2},{3}", homelocation.Lat, homelocation.Lng, srtm.getAltitude(homelocation.Lat, homelocation.Lng).alt, heading);
        }

        private string CheckandGetSITLImage(string filename)
        {
            Uri fullurl = new Uri(sitlurl, filename);

            Common.getFilefromNet(fullurl.ToString(), sitldirectory + Path.GetFileNameWithoutExtension(filename) + ".exe");

            // dependancys
            var depurl = new Uri(sitlurl, "cyggcc_s-1.dll");
            Common.getFilefromNet(depurl.ToString(), sitldirectory + depurl.Segments[depurl.Segments.Length - 1]);
            depurl = new Uri(sitlurl, "cygstdc++-6.dll");
            Common.getFilefromNet(depurl.ToString(), sitldirectory + depurl.Segments[depurl.Segments.Length - 1]);
            depurl = new Uri(sitlurl, "cygwin1.dll");
            Common.getFilefromNet(depurl.ToString(), sitldirectory + depurl.Segments[depurl.Segments.Length - 1]);

            return sitldirectory + Path.GetFileNameWithoutExtension(filename) + ".exe";
        }

        private void StartSITL(string exepath, string model, string homelocation, int speedup = 1)
        {
            //ArduCopter.elf -M+ -O-34.98106,117.85201,40,0 
            string simdir = sitldirectory + model + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(simdir);

            ProcessStartInfo exestart = new ProcessStartInfo();
            exestart.FileName = exepath;
            exestart.Arguments = String.Format("-M{0} -O{1} -s{2}", model, homelocation, speedup);
            exestart.WorkingDirectory = simdir;

            var proc = System.Diagnostics.Process.Start(exestart);

        }
    }
}
