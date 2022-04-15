using System;
using System.Linq;

public class BarbArrow : Button
{
    public override string name { get { return "Barbed Arrow"; } }
    public override Predicate<string> rule { get { return str => !str.Any(ch => ch == 'e' || ch == 'E'); } }
}
