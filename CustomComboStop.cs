public class CustomComboStop
{
    // Class for storing all stops data

    // Stop name
    public string StopName { get; set; }

    // Stop ID
    public int StopID { get; set; }

    // Stop latitude
    public double StopLat { get; set; }

    // Stop longitude
    public double StopLon { get; set; }

    public CustomComboStop()
    {
    }

    public CustomComboStop(string name, int stop_id, double stop_lan, double stop_lon)
    {
        this.StopName = name;
        this.StopID = stop_id;
        this.StopLat = stop_lan;
        this.StopLon = stop_lon;
    }

    // Show only stop name
    public override string ToString()
    {
        return StopName;
    }
}
