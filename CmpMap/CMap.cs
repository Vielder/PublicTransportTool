using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET.WindowsForms.Markers;
using System.IO;
using CmpSearchData;
using CmpHelpers;

namespace CmpMap
{
    public class CMap : IMap
    {
        public void GpsUpdate(GMapOverlay transportOverlay, GMapControl map, string prjRoot, string bus, string type)
        {
            transportOverlay.Markers.Clear();
            map.Overlays.Remove(transportOverlay);
            ISearchData searchData = new CSearchData();
            searchData.getGPS(prjRoot);
            IHelpers helpers = new CHelpers();
            double lat;
            double lon;
            GMapMarkerPlane marker;
            string tagText;
            using (StreamReader reader = new StreamReader(prjRoot + "/gps.csv"))
            {
                if (bus == "")
                {
                    while (!reader.EndOfStream)
                    {
                        int intValue;
                        string line = reader.ReadLine();
                        string[] fields = line.Split(',');
                        lat = helpers.parseCoordinates(fields[3]);
                        lon = helpers.parseCoordinates(fields[2]);
                        Bitmap img;
                        switch (fields[0])
                        {
                            case "1":
                                img = new Bitmap(Image.FromFile(prjRoot + "resources/troll.png"));
                                break;
                            case "2":
                                img = new Bitmap(Image.FromFile(prjRoot + "resources/bus.png"));
                                break;
                            default:
                                img = new Bitmap(Image.FromFile(prjRoot + "resources/tramm.png"));
                                break;
                        }
                        switch (fields[0])
                        {
                            case "1":
                                intValue = 800;
                                break;
                            case "2":
                                intValue = 3;
                                break;
                            default:
                                intValue = 0;
                                break;
                        }
                        marker = new GMapMarkerPlane(new PointLatLng(lat, lon), Convert.ToSingle(fields[5]), img);
                        switch (fields[0])
                        {
                            case "1":
                                tagText = $"Transport type: Troll{Environment.NewLine}Transport number: {fields[1]}{Environment.NewLine}Transport ID: {fields[6]}{Environment.NewLine}";
                                break;
                            case "2":
                                tagText = $"Transport type: Bus{Environment.NewLine}Transport number: {fields[1]}{Environment.NewLine}Transport ID: {fields[6]}{Environment.NewLine}";
                                break;
                            default:
                                tagText = $"Transport type: Tram{Environment.NewLine}Transport number: {fields[1]}{Environment.NewLine}Transport ID: {fields[6]}{Environment.NewLine}";
                                break;
                        }
                        marker.Tag = new CustomMarker(tagText, 0, "a", intValue);
                        transportOverlay.Markers.Add(marker);
                        marker.ToolTipText = tagText;
                    }
                }
                else
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = line.Split(',');

                        if (fields[1] == bus && fields[0] == type)
                        {
                            int intValue;
                            lat = helpers.parseCoordinates(fields[3]);
                            lon = helpers.parseCoordinates(fields[2]);
                            Bitmap img;
                            switch (fields[0])
                            {
                                case "1":
                                    img = new Bitmap(Image.FromFile(prjRoot + "resources/troll.png"));
                                    break;
                                case "2":
                                    img = new Bitmap(Image.FromFile(prjRoot + "resources/bus.png"));
                                    break;
                                default:
                                    img = new Bitmap(Image.FromFile(prjRoot + "resources/tramm.png"));
                                    break;
                            }
                            switch (fields[0])
                            {
                                case "1":
                                    intValue = 800;
                                    break;
                                case "2":
                                    intValue = 3;
                                    break;
                                default:
                                    intValue = 0;
                                    break;
                            }
                            marker = new GMapMarkerPlane(new PointLatLng(lat, lon), Convert.ToSingle(fields[5]), img);
                            switch (fields[0])
                            {
                                case "1":
                                    tagText = $"Transport type: Troll{Environment.NewLine}Transport number: {fields[1]}{Environment.NewLine}Transport ID: {fields[6]}{Environment.NewLine}";
                                    break;
                                case "2":
                                    tagText = $"Transport type: Buss{Environment.NewLine}Transport number: {fields[1]}{Environment.NewLine}Transport ID: {fields[6]}{Environment.NewLine}";
                                    break;
                                default:
                                    tagText = $"Transport type: Tramm{Environment.NewLine}Transport number: {fields[1]}{Environment.NewLine}Transport ID: {fields[6]}{Environment.NewLine}";
                                    break;
                            }
                            marker.Tag = new CustomMarker(tagText, 0, "a", intValue);
                            transportOverlay.Markers.Add(marker);
                            marker.ToolTipText = tagText;
                        }
                    }
                }
            }
            map.Overlays.Add(transportOverlay);
            map.Refresh();
        }

        public void ImplementMarkersOnMap(string name, int id, double lat, double lon, Bitmap img, GMapControl map, ref GMapOverlay markersOverlay)
        {
            // Create a new marker with the specified latitude, longitude, and image
            GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(lat, lon), img);
            // Set the marker's tag to a new instance of a custom marker with a name, ID, route, and type
            marker.Tag = new CustomMarker(name, id, "", 9);
            // Set the marker's tooltip text to the name of the marker
            marker.ToolTipText = name;
            // Add the marker to the markers overlay
            markersOverlay.Markers.Add(marker);

        }

        public void InitGmapView(ref double lat, ref double lng, ref GMapControl map)
        {
            // Manage map initialization by the project startup
            map.ShowCenter = false;
            map.MapProvider = GMapProviders.OpenStreetMap;
            map.DragButton = MouseButtons.Left;
            map.Position = new PointLatLng(lat, lng);

        }
    }
}
