using System;
using System.Text.RegularExpressions;

public class RectangularButton : Button
{
    public override string name { get { return "Flat Rectangle"; } }
    public override Predicate<string> rule { get { return str => str.ToLowerInvariant().Contains("the"); } }
}
