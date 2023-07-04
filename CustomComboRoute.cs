public class CustomComboRoute
{
    // Class for storing all route data

    // Route name/number
    public string RouteNum { get; set; }

    // Route direction
    public string RouteDir { get; set; }

    // Route ID
    public string RouteID { get; set; }

    // Route type (bus, tram, trolleybus)
    public int RouteType { get; set; }

    public CustomComboRoute()
    {
    }

    public CustomComboRoute(string number, string route_dir, string route_id, int route_type)
    {
        this.RouteNum = number;
        this.RouteDir = route_dir;
        this.RouteID = route_id;
        this.RouteType = route_type;
    }

    // Show only route name
    public override string ToString()
    {
        return RouteNum;
    }
}
