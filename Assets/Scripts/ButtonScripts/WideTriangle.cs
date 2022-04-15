using System;
using System.Text.RegularExpressions;

public class WideTriangle : Button {

    public override string name { get { return "Wide Triangle"; } }
    public override Predicate<string> rule { get { return str => Regex.IsMatch(str, @"(Wire|Simon|Maze|Morse)", regexFlags); } }
}
