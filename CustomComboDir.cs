public class CustomComboDir
{
    // Class for storing direction data

    // Route direction
    public string RouteDir { get; set; }

    // Route ID
    public string RouteID { get; set; }

    public CustomComboDir()
    {
    }

    public CustomComboDir(string route_dir, string route_id)
    {
        this.RouteDir = route_dir;
        this.RouteID = route_id;
    }

    // Show only direction
    public override string ToString()
    {
        return RouteDir;
    }
}
