using System;
using System.Linq;

public class RoundButton : Button
{
    public override string name { get { return "Circle"; } }
    public override Predicate<string> rule { get { return str => "PBVKXQJZ".Contains(char.ToUpper(str[0])); } }
}
