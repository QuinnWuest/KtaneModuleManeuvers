using System;
using System.Linq;

public class FlatStemArrow : Button
{
    public override string name { get { return "Flat Stem Arrow"; } }
    public override Predicate<string> rule { get { return str => str.Count(ch => "AEIOUaeiou".Contains(ch)) >= 3; } }
}
