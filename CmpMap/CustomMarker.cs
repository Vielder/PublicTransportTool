public class CustomMarker
{
    // class for storing all markers (stops, transport) data

    // name of stop/route
    public string Name { get; set; }

    // stop ID
    public int StopID { get; set; }

    // route ID
    public string Route { get; set; }

    // route type (buss, tramm, troll)
    public int RouteType { get; set; }

    public CustomMarker(string name, int stop_id, string route, int type)
    {
        Name = name;
        StopID = stop_id;
        Route = route;
        RouteType = type;
    }
}
