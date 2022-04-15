using System;
using System.Linq;

public class BarbedStemArrow : Button
{
    public override string name { get { return "Barbed Stem Arrow"; } }
    public override Predicate<string> rule { get { return str => "AEIOUaeiou".Contains(str[0]); } }
}
