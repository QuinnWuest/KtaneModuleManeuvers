using System;
using System.Linq;

public class TeardropArrow : Button
{
    public override string name { get { return "Teardrop"; } }
    public override Predicate<string> rule { get { return str => "AEIOUaeiou".Contains(str.Last()); } }
}
