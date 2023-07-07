using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CmpMap;
using GMap.NET.WindowsForms;
using Npgsql;
using CmpSearchData;
using CmpHelpers;
using System.Reflection;
using CmpPathFinder;

namespace PublicTransportTool
{
    public partial class Form1 : Form
    {
        public NpgsqlConnection conn = new NpgsqlConnection("Database=dbuser;Host=coopmc.asuscomm.com;Username=dbuser;Password=963741789db");

        //global overlays, stops and routes
        private GMapOverlay stopsOverlay = new GMapOverlay("stops");
        private GMapOverlay transportOverlay = new GMapOverlay("trasports");
        private List<CustomComboStop> AllStops = new List<CustomComboStop>();
        private List<CustomComboRoute> AllRoutes = new List<CustomComboRoute>();
        private CustomComboRoute chosenTransport;

        //path for initial app
        private static string appPath = Application.StartupPath;
        private string prjRoot = Path.GetFullPath(appPath + "..\\..\\..\\");

        //comboboxes control variable
        private bool lahtAllow = false;
        private bool sihtAllow = false;
        private bool liinAllow = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            // This block is made for dynamic resize of controls inside Windows Forms
            if (Height <= 480)
                Height = 480;

            if (Width <= 720)
                Width = 720;

            map.Size = new Size(Width, Height);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //initialize location for map
            CultureInfo culture = new CultureInfo("et-EE");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            //opening connection with DB
            conn.OpenAsync();
            //comboboxes settings
            cmbLaht.DropDownHeight = 100;
            cmbSiht.DropDownHeight = 100;
            cmbLiin.DropDownHeight = 100;
            cmbSiht.DropDownStyle = ComboBoxStyle.Simple;
            cmbSiht.Enabled = false;
            cmbSuund.Items.Add("");
            cmbSuund.DropDownStyle = ComboBoxStyle.Simple;
            cmbSuund.Enabled = false;
            lstInfo.DrawMode = DrawMode.OwnerDrawFixed;
            cmbLiin.DrawMode = DrawMode.OwnerDrawFixed;
            //initial overlays for maps
            map.Overlays.Add(transportOverlay);
            map.Overlays.Add(stopsOverlay);
            //timers initializing for auto update
            tmrUpdate.Interval = 2500;
            tmrUpdate.Enabled = true;
            tmrGPSUpdate.Interval = 4000;
            tmrGPSUpdate.Enabled = true;
            //starting coordinates for map
            double lat = 59.436962;
            double lng = 24.753574;
            // Init map call via prjMap component using GMap 
            IMap EditMap;
            EditMap = new CMap();
            EditMap.InitGmapView(ref lat, ref lng, ref map);
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            // first loading of data from DB
            tmrUpdate.Enabled = false;
            AllStops = LoadStops("", "", "", "", "filterStops");

            // adding stops
            string strTemp = "";
            foreach (CustomComboStop stops in AllStops)
            {
                // filling stops comboboxes
                if (strTemp != stops.StopName)
                {
                    cmbLaht.Items.Add(stops);
                    cmbSiht.Items.Add(stops);
                    strTemp = stops.StopName;
                }
                // putting markers on maps overlay
                putMarker(stops);
            }

            map.Overlays.Add(stopsOverlay);
            map.Overlays.Add(transportOverlay);

            // adding routes
            AllRoutes = LoadRoutes("", "", "", "", "filterRoutes");
            strTemp = "";
            int intTemp = 9;
            foreach (CustomComboRoute route in AllRoutes)
            {
                if (strTemp != route.RouteNum || intTemp != route.RouteType)
                {
                    // filling routes comboboxes
                    cmbLiin.Items.Add(route);
                    strTemp = route.RouteNum;
                    intTemp = route.RouteType;
                }
            }

            map.Zoom += 0.00001;
            map.Zoom -= 0.00001;
        }

        private List<CustomList> LoadTimes(string strStop, string strRoute, string strDir, string time, string strTask)
        {
            // init CmpSearchData
            ISearchData searchDb = new CSearchData();
            NpgsqlDataReader reader;

            switch (strTask)
            {
                // case of search
                case "closestTimes":
                    // find closest 3 departure times of chosen routes from chosen stop
                    DateTime curTime = DateTime.Now;
                    IHelpers helpers = new CHelpers();
                    List<CustomList> Times = new List<CustomList>();
                    reader = searchDb.searchInDb(strStop, strRoute, strDir, time, strTask, conn);
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Times.Add(new CustomList()
                            {
                                strText = reader.GetValue(0) + "  " + helpers.timeHandler((string)reader.GetValue(0), curTime.ToString("HH:mm:ss")),
                                intID = 9

                            });
                        }
                    }
                    else
                    {
                        // if no more times this day
                        Times.Add(new CustomList()
                        {
                            strText = "No data found!",
                            intID = 9
                        });
                    }
                    reader.DisposeAsync();
                    return Times;

                case "allRoutesTimes":
                    // find times of all routes passing chosen stop
                    List<CustomList> AllRoutesTimes = new List<CustomList>();
                    curTime = DateTime.Now;
                    helpers = new CHelpers();
                    reader = searchDb.searchInDb(strStop, "", "", "", strTask, conn);
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            AllRoutesTimes.Add(new CustomList()
                            {
                                strText = reader.GetValue(0) + " " + reader.GetValue(1) + " - " + reader.GetValue(2),
                                intID = (int)reader.GetValue(3)
                            });
                        }
                    }
                    else
                    {
                        AllRoutesTimes.Add(new CustomList()
                        {
                            strText = "No data found!",
                            intID = 9
                        });
                    }
                    reader.DisposeAsync();
                    return AllRoutesTimes;

                case "allStopsTimes":
                    // find times on all stops for chosen route
                    curTime = DateTime.Now;
                    helpers = new CHelpers();
                    List<CustomList> AllStopsTimes = new List<CustomList>();
                    reader = searchDb.searchInDb("", strRoute, strDir, "", strTask, conn);
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            AllStopsTimes.Add(new CustomList()
                            {
                                strText = reader.GetValue(0) + " " + reader.GetValue(1) + "  " + helpers.timeHandler((string)reader.GetValue(0), curTime.ToString("HH:mm:ss")),
                                intID = 9
                            });
                        }
                    }
                    else
                    {
                        AllStopsTimes.Add(new CustomList()
                        {
                            strText = "No data found!",
                            intID = 9
                        });
                    }
                    reader.DisposeAsync();
                    return AllStopsTimes;
            }

            return null;
        }

        private List<CustomComboStop> LoadStops(string strStop, string strRoute, string strDir, string time, string strTask)
        {
            // init CmpSearchData
            ISearchData searchDb;
            searchDb = new CSearchData();
            switch (strTask)
            {
                // case of search
                case "filterStops":
                    // find stops from preexisting string
                    List<CustomComboStop> Stops = new List<CustomComboStop>();
                    NpgsqlDataReader reader = searchDb.searchInDb(strStop, "", "", "", strTask, conn);
                    while (reader.Read())
                    {
                        Stops.Add(new CustomComboStop()
                        {
                            StopName = (string)reader.GetValue(0),
                            StopID = (int)reader.GetValue(1),
                            StopLat = (double)reader.GetValue(2),
                            StopLon = (double)reader.GetValue(3)
                        });
                    }
                    // disconnect
                    reader.DisposeAsync();
                    return Stops;

                case "relatedStops":
                    // find stops related to chosen route
                    List<CustomComboStop> RelatedStops = new List<CustomComboStop>();
                    reader = searchDb.searchInDb("", strRoute, strDir, "", strTask, conn);
                    while (reader.Read())
                    {
                        RelatedStops.Add(new CustomComboStop()
                        {
                            StopName = (string)reader.GetValue(0),
                            StopID = (int)reader.GetValue(1),
                            StopLat = (double)reader.GetValue(2),
                            StopLon = (double)reader.GetValue(3)
                        });
                    }
                    reader.DisposeAsync();
                    return RelatedStops;
            }

            return null;

        }

        private List<CustomComboRoute> LoadRoutes(string strStop, string strRoute, string strDir, string time, string strTask)
        {
            // init CmpSearchData
            ISearchData searchDb = new CSearchData();
            NpgsqlDataReader reader;
            switch (strTask)
            {
                // case of search
                case "filterRoutes":
                    // find routes from preexisting string
                    List<CustomComboRoute> Routes = new List<CustomComboRoute>();
                    reader = searchDb.searchInDb(strStop, strRoute, strDir, "", strTask, conn);
                    while (reader.Read())
                    {
                        Routes.Add(new CustomComboRoute()
                        {
                            RouteNum = (string)reader.GetValue(0),
                            RouteDir = (string)reader.GetValue(1),
                            RouteID = (string)reader.GetValue(2),
                            RouteType = (int)reader.GetValue(3)
                        });
                    }
                    reader.DisposeAsync();
                    return Routes;

                case "relatedRoutes":
                    // find routes related to chosen stop
                    List<CustomComboRoute> RelatedRoutes = new List<CustomComboRoute>();
                    reader = searchDb.searchInDb(strStop, "", "", "", strTask, conn);
                    while (reader.Read())
                    {
                        RelatedRoutes.Add(new CustomComboRoute()
                        {
                            RouteNum = (string)reader.GetValue(0),
                            RouteDir = (string)reader.GetValue(1),
                            RouteID = (string)reader.GetValue(2),
                            RouteType = (int)reader.GetValue(3)
                        });
                    }
                    reader.DisposeAsync();
                    return RelatedRoutes;
            }

            return null;

        }

        private void cmbLaht_KeyPress(object sender, KeyPressEventArgs e)
        {
            // control of typing in combobox, allows text changing
            lahtAllow = true;
        }

        private void cmbLaht_TextChanged(object sender, EventArgs e)
        {
            // control if text changed while typing in combobox
            if (lahtAllow && (cmbLaht.Text.Length != 1))
            {
                // refilling stops in combobox
                cmbLaht.Items.Clear();
                cmbLaht.DroppedDown = true;
                // finding stops containing typed text
                cmbLaht.Items.AddRange(filterStopLocal(cmbLaht.Text).ToArray());
                // setting text cursor on the end of string
                cmbLaht.Select(cmbLaht.Text.Length, 0);
                // holding mouse cursor on screen
                Cursor.Current = Cursors.Default;
            }
        }

        private void cmbLaht_DropDownClosed(object sender, EventArgs e)
        {
            // disabling text changing function
            lahtAllow = false;
        }

        private void cmbLaht_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // accepting chosen stop
            lahtAllow = false;
            CustomComboStop peatus = cmbLaht.SelectedItem as CustomComboStop;

            // put chosen stop name in the text box of combobox
            if (peatus != null)
            {
                cmbLaht.Text = peatus.StopName;
            }

            cmbLaht.DroppedDown = false;
        }

        private void cmbLaht_SelectedIndexChanged(object sender, EventArgs e)
        {
            // action after stop accepting
            if (cmbLaht.SelectedItem != null && cmbLaht.Text != "")
            {
                CustomComboStop peatus = cmbLaht.SelectedItem as CustomComboStop;
                cmbLaht.DroppedDown = false;

                // clearing stops overlay on map
                stopsOverlay.Markers.Clear();
                map.Overlays.Remove(stopsOverlay);

                // put markers of chosen stop
                putMarker(peatus);
                map.Overlays.Add(stopsOverlay);

                // find related routes for chosen stop
                string liin = "";
                int type = 9;
                List<CustomComboRoute> Routes = new List<CustomComboRoute>();
                Routes = LoadRoutes(peatus.StopName, "", "", "", "relatedRoutes");
                Routes.Add(new CustomComboRoute() { RouteNum = "", RouteDir = "", RouteID = "", RouteType = 9 });

                List<CustomList> itemsList = new List<CustomList>();
                cmbLiin.Items.Clear();
                lstInfo.Items.Clear();

                // current time
                itemsList.Add(new CustomList() { strText = DateTime.Now.ToString(), intID = 9 });
                itemsList.Add(new CustomList() { strText = "", intID = 9 });

                foreach (CustomComboRoute item in Routes)
                {
                    // refilling of routes combobox
                    itemsList.Add(new CustomList() { strText = item.RouteNum + " - " + item.RouteDir, intID = item.RouteType });

                    if (liin != item.RouteNum || type != item.RouteType)
                    {
                        cmbLiin.Items.Add(item);
                        liin = item.RouteNum;
                        type = item.RouteType;
                    }
                }

                // showing result to user
                lstInfo.Items.AddRange(itemsList.ToArray());

                // enabling second stop combobox
                cmbSiht.Enabled = true;
                cmbSiht.SelectedItem = null;
                cmbSiht.DropDownStyle = ComboBoxStyle.DropDown;
            }
            else
            {
                // error preventing close of combobox
                cmbSiht.Enabled = false;
                cmbSiht.SelectedItem = null;
                cmbSiht.DropDownStyle = ComboBoxStyle.Simple;
            }
        }

        private void putMarker(CustomComboStop item)
        {
            // adding stop marker to overlay
            if (item != null)
            {
                // init resource images location and PrjMap
                IMap EditMap;
                EditMap = new CMap();
                Bitmap img = new Bitmap(Image.FromFile(prjRoot + @"resources\stop.png"));
                EditMap.ImplementMarkersOnMap(item.StopName, item.StopID, item.StopLat, item.StopLon, img, map, ref stopsOverlay);
            }
        }

        private List<CustomComboStop> filterStopLocal(string strStop)
        {
            // searching stops from local stops based on preexisting string
            stopsOverlay.Markers.Clear();
            map.Overlays.Remove(stopsOverlay);
            string peatus = "";
            List<CustomComboStop> itemsStops = new List<CustomComboStop>();
            foreach (CustomComboStop item in AllStops)
            {
                if (item.StopName.ToLower().Contains(strStop.ToLower()))
                {
                    // find distinct stop names
                    if (peatus != item.StopName)
                    {
                        itemsStops.Add(item);
                        peatus = item.StopName;
                    }
                    // adding markers
                    putMarker(item);
                }
            }
            map.Overlays.Add(stopsOverlay);
            // error preventing additional line, no 0 index in combobox
            itemsStops.Add(new CustomComboStop() { StopName = "", StopID = 0, StopLat = 0, StopLon = 0 });
            return itemsStops;
        }

        private void cmbSiht_KeyPress(object sender, KeyPressEventArgs e)
        {
            // control of typing in combobox, allows text changing
            sihtAllow = true;
        }

        private void cmbSiht_TextChanged(object sender, EventArgs e)
        {
            // control if text changed while typing in combobox
            if (sihtAllow && cmbSiht.Text.Length != 1)
            {
                // refilling stops in combobox
                cmbSiht.Items.Clear();
                cmbSiht.DroppedDown = true;
                cmbSiht.Items.AddRange(filterStopLocal(cmbSiht.Text).ToArray());
                cmbSiht.Select(cmbSiht.Text.Length, cmbSiht.Text.Length);
                Cursor.Current = Cursors.Default;
            }
        }

        private void cmbSiht_DropDownClosed(object sender, EventArgs e)
        {
            // disabling text changing function
            sihtAllow = false;
        }

        private void cmbSiht_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // accepting chosen stop and closing combobox, disabling text changing function
            sihtAllow = false;
            CustomComboStop peatus = cmbSiht.SelectedItem as CustomComboStop;
            if (peatus != null)
            {
                cmbSiht.Text = peatus.StopName;
            }
            cmbSiht.DroppedDown = false;
        }

        private void cmbSiht_SelectedIndexChanged(object sender, EventArgs e)
        {
            stopsOverlay.Markers.Clear();
            map.Overlays.Remove(stopsOverlay);
            CustomComboStop peatus = cmbSiht.SelectedItem as CustomComboStop;
            // add marker of stop on map
            putMarker(peatus);
            if (cmbLaht.SelectedItem != null && cmbLaht.Text != "")
            {
                peatus = cmbLaht.SelectedItem as CustomComboStop;
                putMarker(peatus);
            }
            map.Overlays.Add(stopsOverlay);
        }

        private void cmbLiin_KeyPress(object sender, KeyPressEventArgs e)
        {
            // control if typing in combobox, allow text changing function
            liinAllow = true;
        }

        private void cmbLiin_TextChanged(object sender, EventArgs e)
        {
            // control if text changed because of typing
            if (liinAllow)
            {
                cmbLiin.Items.Clear();
                cmbLiin.DroppedDown = true;
                // searching routes based on written text
                cmbLiin.Items.AddRange(filterRouteLocal(cmbLiin.Text).ToArray());
                cmbLiin.Select(cmbLiin.Text.Length, 0);
                Cursor.Current = Cursors.Default;
            }
        }

        private List<CustomComboRoute> filterRouteLocal(string strLiin)
        {
            // search routes from global routes variable
            List<CustomComboRoute> itemsRoutes = new List<CustomComboRoute>();
            string num = "";
            int type = 9;
            foreach (CustomComboRoute route in AllRoutes)
            {
                // search distinct route names and types
                if (route.RouteNum.ToLower().Contains(strLiin.ToLower()) && (num != route.RouteNum || type != route.RouteType))
                {
                    itemsRoutes.Add(route);
                    num = route.RouteNum;
                    type = route.RouteType;
                }
            }
            // error preventing
            itemsRoutes.Add(new CustomComboRoute() { RouteNum = "", RouteDir = "", RouteID = "", RouteType = 9 });
            return itemsRoutes;
        }

        private void cmbLiin_DropDownClosed(object sender, EventArgs e)
        {
            // disable text change function
            liinAllow = false;
        }

        private void cmbLiin_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // accept choice of route
            liinAllow = false;
            CustomComboRoute liin = cmbLiin.SelectedItem as CustomComboRoute;
            if (liin != null)
            {
                cmbLiin.Text = liin.RouteNum;
            }
            cmbLiin.DroppedDown = false;
        }

        private void cmbLiin_SelectedIndexChanged(object sender, EventArgs e)
        {
            // action after choosing the route, filling directions of route
            if (cmbLiin.SelectedItem != null && cmbLiin.Text != "")
            {
                cmbLiin.DroppedDown = false;
                cmbSuund.Items.Clear();
                List<CustomComboDir> itemsDir = new List<CustomComboDir>();
                CustomComboRoute route = cmbLiin.SelectedItem as CustomComboRoute;
                // find distinct directions from global routes
                foreach (CustomComboRoute direction in AllRoutes)
                {
                    if (route.RouteNum == direction.RouteNum && route.RouteType == direction.RouteType)
                    {
                        itemsDir.Add(new CustomComboDir() { RouteDir = direction.RouteDir, RouteID = direction.RouteID });
                    }
                }
                itemsDir.Add(new CustomComboDir() { RouteDir = "", RouteID = "" });
                // refill directions combobox
                cmbSuund.Items.AddRange(itemsDir.ToArray());
                cmbSuund.Enabled = true;
                cmbSuund.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbSuund.DroppedDown = true;
            }
        }

        private void lstInfo_DrawItem(object sender, DrawItemEventArgs e)
        {
            // coloring info listbox lines with colors of route type
            Brush background;
            try
            {
                CustomList item = (CustomList)lstInfo.Items[e.Index];
                // choose color based on type
                switch (item.intID)
                {
                    case 0: // tramm
                        background = Brushes.Coral;
                        break;
                    case 3: // buss
                        background = Brushes.LightGreen;
                        break;
                    case 800: // troll
                        background = Brushes.LightBlue;
                        break;
                    default: // default
                        background = Brushes.White;
                        break;
                }
                e.DrawBackground();
                e.Graphics.FillRectangle(background, e.Bounds);
                e.Graphics.DrawString(lstInfo.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds);
            }
            catch (Exception)
            {
                Console.WriteLine("Error on lstInfo_DrawItem!");
            }
        }

        private void cmbLiin_DrawItem(object sender, DrawItemEventArgs e)
        {
            // coloring of routes combobox based on type of route
            Brush background;
            CustomComboRoute item = (CustomComboRoute)cmbLiin.Items[e.Index];
            chosenTransport = item;
            switch (item.RouteType)
            {
                case 0: // tramm
                    background = Brushes.Coral;
                    break;
                case 3: // buss
                    background = Brushes.LightGreen;
                    break;
                case 800: // troll
                    background = Brushes.LightBlue;
                    break;
                default: // default
                    background = Brushes.White;
                    break;
            }
            e.DrawBackground();
            e.Graphics.FillRectangle(background, e.Bounds);
            e.Graphics.DrawString(item.RouteNum, e.Font, Brushes.Black, e.Bounds);
        }

        private void cmbSuund_SelectedIndexChanged(object sender, EventArgs e)
        {
            // search of all stops in order related to route and direction
            if (cmbSuund.Text != null && cmbSuund.Text != "" && cmbLiin.SelectedItem != null && cmbLiin.Text != "")
            {
                cmbSuund.DroppedDown = false;
                // clearing map and stops comboboxes
                stopsOverlay.Markers.Clear();
                map.Overlays.Remove(stopsOverlay);
                lstInfo.Items.Clear();
                cmbSiht.Items.Clear();
                cmbLaht.Items.Clear();
                CustomComboDir itemDir = (CustomComboDir)cmbSuund.SelectedItem;
                // find in DB related stops
                List<CustomComboStop> Stops = LoadStops("", itemDir.RouteID, itemDir.RouteDir, "", "relatedStops");
                List<CustomList> itemsList = new List<CustomList>();
                foreach (CustomComboStop item in Stops)
                {
                    itemsList.Add(new CustomList() { strText = item.StopName, intID = 9 });
                    putMarker(item);
                }
                itemsList.Add(new CustomList() { strText = "", intID = 9 });
                Stops.Add(new CustomComboStop() { StopName = "", StopID = 0 });
                // put stops on map and comboboxes
                lstInfo.Items.AddRange(itemsList.ToArray());
                cmbLaht.Items.AddRange(Stops.ToArray());
                cmbSiht.Items.AddRange(Stops.ToArray());
                map.Overlays.Add(stopsOverlay);
            }
        }

        private void map_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            // choosing stop or transport by clicking on marker on map
            CustomMarker mark = (CustomMarker)item.Tag;
            List<CustomComboRoute> routs = cmbLiin.Items.Cast<CustomComboRoute>().ToList();
            List<CustomComboStop> stops = cmbLaht.Items.Cast<CustomComboStop>().ToList();

            // deciding if marker is stop or route
            if (mark.Route == "")
            {
                // stop
                foreach (CustomComboStop s in stops)
                {
                    // selecting stop and put selection into combobox
                    if (s.StopName == mark.Name)
                    {
                        cmbLaht.SelectedItem = s;
                    }
                }
            }
            else
            {
                // route
                foreach (CustomComboRoute r in routs)
                {
                    // select route and put into combobox
                    if (r.RouteType == mark.RouteType && r.RouteNum == mark.Name)
                    {
                        cmbLiin.SelectedItem = r;
                    }
                }
            }
        }


        private async Task<List<string>> FindPathAsync(string Laht, string Siht, bool Wheelchair, int TransfersAmount, string Time)
        {
            // init PrjPathFinder
            IPathFinder pathFinder;
            pathFinder = new CPathFinder();
            List<string> strList = new List<string>();
            strList = await pathFinder.FindRouteAsync(Laht, Siht, TransfersAmount, Wheelchair, Time, conn);
            return strList;
        }

        private void tmrGPSUpdate_Tick(object sender, EventArgs e)
        {
            // Create a new instance of the CMap class
            CMap cmap = new CMap();

            // Initialize the type variable as a string with a default value of "0"
            string type = "0";

            // Check if the cmbLiin Text property is not empty
            if (!string.IsNullOrEmpty(cmbLiin.Text))
            {
                // Select a case based on the chosenTransport.RouteType value
                switch (chosenTransport.RouteType)
                {
                    // If the value is "0" (Tramm), set the type variable to "3"
                    case 0:
                        type = "3";
                        break;
                    // If the value is "800" (Troll), set the type variable to "1"
                    case 800:
                        type = "1";
                        break;
                    // If the value is "3" (Bus), set the type variable to "2"
                    case 3:
                        type = "2";
                        break;
                    // For any other value, set the type variable to "0"
                    default:
                        type = "0";
                        break;
                }
            }

            // Call the GpsUpdate method of the cmap object, passing in the transportOverlay, map, prjRoot, cmbLiin Text, and type values as arguments
            cmap.GpsUpdate(transportOverlay, map, prjRoot, cmbLiin.Text, type);
        }

        private async void btnOtsi_Click(object sender, EventArgs e)
        {
            // action for Search button based on active comboboxes
            DateTime curTime = DateTime.Now;
            List<CustomList> itemsList = new List<CustomList>();

            lstInfo.Items.Clear();
            lstInfo.Items.Add(new CustomList() { strText = DateTime.Now.ToString(), intID = 9 });
            lstInfo.Items.Add(new CustomList() { strText = "", intID = 9 });

            if (cmbSuund.Text != "" && cmbLiin.Text != "" && cmbLaht.Text != "" && cmbSiht.Text == "")
            {
                // shows closest 3 times of selected route on selected stop
                CustomComboDir itemDir = (CustomComboDir)cmbSuund.SelectedItem;
                itemsList = LoadTimes(cmbLaht.Text, itemDir.RouteID, itemDir.RouteDir, curTime.ToLongTimeString(), "closestTimes");
                lstInfo.Items.AddRange(itemsList.ToArray());
            }
            else if (cmbLaht.Text != "" && cmbLiin.Text == "" && cmbSiht.Text == "")
            {
                // shows all times of all routes on selected stop
                itemsList = LoadTimes(cmbLaht.Text, "", "", curTime.ToLongTimeString(), "allRoutesTimes");
                lstInfo.Items.AddRange(itemsList.ToArray());
            }
            else if (cmbLaht.Text == "" && cmbSiht.Text == "" && cmbSuund.Text != "" && cmbLiin.Text != "")
            {
                // shows all times on all stops of selected route
                CustomComboDir itemDir = (CustomComboDir)cmbSuund.SelectedItem;
                itemsList = LoadTimes("", itemDir.RouteID, itemDir.RouteDir, "", "allStopsTimes");
                lstInfo.Items.AddRange(itemsList.ToArray());
            }
            else if (cmbLaht.Text != "" && cmbSiht.Text != "")
            {
                // find way
                cmbSiht.DroppedDown = false;
                cmbLaht.DroppedDown = false;
                cmbLiin.Items.Clear();
                List<string> strList = await FindPathAsync(cmbLaht.Text, cmbSiht.Text, checkWheelchair.Checked, (int)nudTransfersAmount.Value, curTime.ToLongTimeString());
                if (strList.Count < 1)
                {
                    // if not found
                    lstInfo.Items.Add(new CustomList() { strText = "Pole leitud...", intID = 9 });
                }
                else
                {
                    foreach (string line in strList)
                    {
                        lstInfo.Items.Add(new CustomList() { strText = line, intID = 9 });
                    }
                }
            }
        }
    }
}
