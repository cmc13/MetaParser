namespace MetaParser.Models;

public class ViewString
{
    public string String { get; set; }

    public static implicit operator string(ViewString vs) => vs.String;
    public static explicit operator ViewString(string s) => new ViewString { String = s };

    public override string ToString() => String;
}
