using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModuleManeuversSetup {

    private static readonly string[] ttksMods = { "Password", "Who’s On First", "Crazy Talk", "Keypad", "Listening", "Orientation Cube", "Morse Code", "Wires", "Two Bits", "The Button", "Colour Flash", "Round Keypad" };


    public List<ModuleManeuversScript> maneuvers;
	public KMBombModule[] allMods;
    public string[] ignored;
    private KMBombModule[] availableMods;
    private string[] modNames;
    public bool hasStarted, isFinished;

    private List<string> pickFirst = new List<string>();
    private List<string> pickLater = new List<string>();

    public ModuleManeuversSetup(List<ModuleManeuversScript> maneuvers, KMBombModule[] allMods, string[] ignored)
    {
        this.maneuvers = maneuvers;
        this.allMods = allMods;
        this.ignored = ignored;
        modNames = allMods.Select(x => x.ModuleDisplayName).ToArray();
        availableMods = allMods.Where(m => !ignored.Contains(m.ModuleDisplayName)).ToArray();
    }
    
    public IEnumerator GetMods()
    {
        hasStarted = true;
        yield return CheckForSpecials();
        isFinished = true;
    }
    private IEnumerator CheckForSpecials()
    {
        if (modNames.Contains("Turn The Keys"))
        {

        }
    }
    

}
