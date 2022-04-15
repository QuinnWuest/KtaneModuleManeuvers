using System;
using System.Linq;

public class SquareButton : Button
{
    public override string name { get { return "Square"; } }
    public override Predicate<string> rule { get { return str => str.Last() == 's' || str.Last() == 'S'; } }
}
