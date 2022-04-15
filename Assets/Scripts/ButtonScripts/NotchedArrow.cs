using System;
using System.Linq;

public class NotchedArrow : Button
{
    public override string name { get { return "Notched Arrow"; } }
    public override Predicate<string> rule { get { return str => char.ToUpperInvariant(str.First()) == char.ToUpperInvariant(str.Last()); } }
}
