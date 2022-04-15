using System;
using System.Linq;

public class ArrowedRectangle : Button
{
    public override string name { get { return "Arrowed Rectangle"; } }
    public override Predicate<string> rule { get { return str => !str.Any(ch => char.IsWhiteSpace(ch)); } }
}
