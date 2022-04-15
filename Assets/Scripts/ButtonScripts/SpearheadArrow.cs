using System;
using System.Linq;

public class SpearheadArrow : Button
{
    public override string name { get { return "Spearhead"; } }
    public override Predicate<string> rule { get { return str => str.Any(ch => !char.IsLetter(ch) && !char.IsWhiteSpace(ch)); } }
}
