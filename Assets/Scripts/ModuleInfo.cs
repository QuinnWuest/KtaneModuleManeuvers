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
        directions = new List<Dir>();
        foreach (Button btn in btns)
            if (btn.rule(name))
                directions.Add(btn.direction);
    }
    public override string ToString()
    {
        return modName + ": " + (directions.Count == 0 ? "ø" : directions.Select(d => d.ToString()[0]).Join(""));
    }

    public IEnumerable<Dir> Invert()
    {
        return directions.Select(d => (Dir)((int)d + 2));
    }
    public bool Includes(Dir d)
    {
        return directions.Contains(d) && !directions.Contains((Dir)((int)d + 2));
    }

    public override bool Equals(object obj)
    {
        return obj is ModuleInfo && (obj as ModuleInfo).modName == modName;
    }

}
