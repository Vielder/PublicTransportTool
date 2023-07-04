using GMap.NET.WindowsForms;
using System.Drawing;

namespace CmpMap
{
    public interface IMap
    {
        void InitGmapView(ref double lat, ref double lng, ref GMap.NET.WindowsForms.GMapControl map);
        void ImplementMarkersOnMap(string name, int id, double lat, double lon, Bitmap img, GMap.NET.WindowsForms.GMapControl map, ref GMapOverlay markersOverlay);
        void GpsUpdate(GMapOverlay transportOverlay, GMapControl map, string prjRoot, string bus, string type);

    }
}