using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

// This class represents a custom marker for a GMap control that displays a plane icon rotated to match the heading of the plane.
public class GMapMarkerPlane : GMapMarker
{
    // Declare instance variables to store the path to the project root directory, the heading of the plane, and the plane icon.
    private static string appPath = System.Windows.Forms.Application.StartupPath;
    private string prjRoot = Path.GetFullPath(appPath + "..\\..\\");
    private float heading = 0;
    private Bitmap icon;

    // This constructor creates a new instance of the marker with the specified position, heading, and icon.
    public GMapMarkerPlane(PointLatLng p, float heading, Bitmap icon) : base(p)
    {
        this.heading = heading;
        this.icon = icon;
        Size = icon.Size;
    }

    // This method overrides the OnRender method of the base class to render the marker.
    public override void OnRender(Graphics g)
    {
        Matrix temp = g.Transform;
        g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
        g.RotateTransform(-Overlay.Control.Bearing);

        // Try to rotate the graphics context by the heading of the plane
        try
        {
            g.RotateTransform(heading);
        }
        catch (Exception)
        {
            Console.WriteLine("Error with heading!");
        }

        // Draw the plane icon in the rotated graphics context
        g.DrawImageUnscaled(icon, icon.Width / -2, icon.Height / -2);
        g.Transform = temp;
    }
}
