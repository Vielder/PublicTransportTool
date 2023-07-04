public class CustomList
{
    // Class for storing data of items in info listbox, for proper coloring

    // Text of the line
    public string strText { get; set; }

    // Identifier for color
    public string intID { get; set; }

    public CustomList()
    {
    }

    public CustomList(string name, string id)
    {
        this.strText = name;
        this.intID = id;
    }

    // Show only text
    public override string ToString()
    {
        return strText;
    }
}
