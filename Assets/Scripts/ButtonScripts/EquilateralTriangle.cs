using System;
using System.Linq;

public class EquilateralTriangle : Button
{
    public override string name { get { return "Equilateral Triangle"; } }
    public override Predicate<string> rule { get { return str => str.Count(ch => "AEIOUaeiou".Contains(ch)) < 2; } }
}
