using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModuleInfo {

    public readonly string modName;
    public readonly List<Dir> directions;

    public ModuleInfo(string name, IEnumerable<Dir> directions)
    {
        modName = name;
        this.directions = new List<Dir>(directions);
    }
    public ModuleInfo(string name, Button[] btns)
    {
        modName = name;
        foreach (Button btn in btns)
            if (btn.rule(name))
                directions.Add(btn.direction);
    }
    public override string ToString()
    {
        return modName + ": " + directions.Select(d => d.ToString()[0]).Join("");
    }

}
