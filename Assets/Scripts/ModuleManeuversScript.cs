using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Reflection;

public class ModuleManeuversScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBossModule Boss;

    private static string[] ignoredModules = null;

    public Button[] buttonPrefabs;
    public KMSelectable centerButton;
    public TextMesh leftDisp, rightDisp;
    public Transform[] positionParents;

    private Button[] usedButtons;
    private List<ModuleInfo> usableInfos;
    private List<ModuleInfo> solutionInfos;
    private int solvedModsCount = 0;
    private Coordinate endPos, currentPos;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private bool centerHeld;
    private Coroutine holdCoroutine;

    void Awake () {
        moduleId = moduleIdCounter++;
        centerButton.OnInteract += () => { Hold(); return false; };
        centerButton.OnInteractEnded += () => Release();
        GetButtons();
    }
    void Hold()
    {
        if (centerHeld)
            return;
        centerHeld = true;
        holdCoroutine = StartCoroutine(MoveButton(0.03f, 0.021f, 0.1f));
        centerButton.AddInteractionPunch(0.75f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, centerButton.transform);
    }
    void Release()
    {
        centerHeld = false;
        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);
        holdCoroutine = StartCoroutine(MoveButton(0.021f, 0.03f, 0.1f));
    }

    void Start ()
    {
        ignoredModules = ignoredModules ?? Boss.GetIgnoredModules("Module Maneuvers", new string[] { "14", "42", "501", "A>N<D", "Bamboozling Time Keeper", "Black Arrows", "Brainf---", "Busy Beaver", "Don't Touch Anything", "Floor Lights", "Forget Any Color", "Forget Enigma", "Forget Everything", "Forget Infinity", "Forget It Not", "Forget Maze Not", "Forget Me Later", "Forget Me Not", "Forget Perspective", "Forget The Colors", "Forget Them All", "Forget This", "Forget Us Not", "Iconic", "Keypad Directionality", "Kugelblitz", "Module Maneuvers", "Multitask", "OmegaDestroyer", "OmegaForest", "Organization", "Password Destroyer", "Purgatory", "RPS Judging", "Security Council", "Shoddy Chess", "Simon Forgets", "Simon's Stages", "Souvenir", "Tallordered Keys", "The Time Keeper", "Timing is Everything", "The Troll", "Turn The Key", "The Twin", "Übermodule", "Ultimate Custom Night", "The Very Annoying Button", "Whiteout" });
        GetMovements();
        GetPositions();
    }

    void GetButtons()
    {
        const float rotationBound = 20f;
        usedButtons = buttonPrefabs.Shuffle().Take(4).ToArray();
        for (int i = 0; i < 4; i++)
        {
            usedButtons[i] = Instantiate(usedButtons[i], positionParents[i]);
            usedButtons[i].direction = (Dir)i;
            usedButtons[i].transform.localEulerAngles = new Vector3(0, Rnd.Range(-rotationBound, +rotationBound), 0);
            usedButtons[i].SetChildStatus(false);
            Log("{0} button: {1}", (Dir)i, usedButtons[i]);
        }
        GetComponent<KMSelectable>().Children = new[] { centerButton }.Concat(usedButtons.Select(x => x.btn)).ToArray();
        GetComponent<KMSelectable>().UpdateChildrenProperly();
    }
    void GetMovements()
    {
        string[] modNames = Bomb.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).ToArray();
#if UNITY_EDITOR
        modNames = new[] { "3D Maze", "Adjacent Letters", "Adventure Game", "Alphabet", "Anagrams", "Astrology", "Battleship", "Big Button Translated", "Binary LEDs", "Bitmaps", "Bitwise Operations", "Blind Alley", "Boolean Venn Diagram", "Broken Buttons", "The Bulb", "The Button", "Caesar Cipher", "Cheap Checkout", "Chess", "Chord Qualities", "The Clock", "Colored Squares", "Color Math", "Colour Flash", "Combination Lock", "Complicated Buttons", "Complicated Wires", "Connection Check", "Coordinates", "Crazy Talk", "Creation", "Cryptography", "Double-Oh", "Emoji Math", "English Test", "Fast Math", "FizzBuzz", "Follow the Leader", "Foreign Exchange Rates", "Forget Me Not", "Friendship", "The Gamepad", "Hexamaze", "Ice Cream", "Keypad", "Laundry", "LED Encryption", "Letter Keys", "Light Cycle", "Listening", "Logic", "Maze", "Memory", "Microcontroller", "Minesweeper", "Modules Against Humanity", "Monsplode, Fight!", "Morse Code", "Morse Code Translated", "Morsematics", "Mouse In The Maze", "Murder", "Mystic Square", "Neutralization", "Number Pad", "Only Connect", "Orientation Cube", "Password", "Passwords Translated", "Perspective Pegs", "Piano Keys", "Plumbing", "Point of Order", "Probing", "Resistors", "Rhythms", "Rock-Paper-Scissors-L.-Sp.", "Round Keypad", "Rubik's Cube", "Safety Safe", "The Screw", "Sea Shells", "Semaphore", "Shape Shift", "Silly Slots", "Simon Says", "Simon Screams", "Simon States", "Skewed Slots", "Souvenir", "Square Button", "Switches", "Symbolic Password", "Text Field", "Third Base", "Tic Tac Toe", "Turn The Key", "Turn The Keys", "Two Bits", "Web Design", "Who's on First", "Who's on First Translated", "Wire Placement", "Wires", "Wire Sequence", "Word Scramble", "Word Search", "Yahtzee", "Zoo", };
#endif
        usableInfos = new List<ModuleInfo>(modNames.Length);
        Log("Found {0} modules on the bomb.", modNames.Length);
        foreach (string name in modNames)
        {
            ModuleInfo info = new ModuleInfo(name, usedButtons);
            usableInfos.Add(info);
            Log(info.ToString());
        }
    }
    void GetPositions()
    {
        ModuleInfo[] uses = usableInfos.Where(x => x.directions.Count > 0).ToArray();
        currentPos = new Coordinate(0, 0);
        if (uses.Count() > 0)
        {
            if (Bomb.GetModuleNames().Contains("Organization"))
            {
                //Org handler
            }
            else
            {
                List<ModuleInfo> path = GetPath(uses);
                Log("Generated path:");
                List<Coordinate> visited = new List<Coordinate>(path.Count + 1) { new Coordinate(0, 0) };
                foreach (ModuleInfo info in path)
                {
                    Log(info.ToString());
                    visited.Add(visited.Last().ApplyMovement(info.directions));
                }
                Log("Path from origin: " + visited.Join(" > "));
                endPos = visited.Last();
                leftDisp.text = endPos.x.ToString();
                rightDisp.text = endPos.y.ToString();
            }
        }

    }
    List<ModuleInfo> GetPath(ModuleInfo[] used)
    {
        int pathLength = Math.Max(1, used.Length / 3);
        List<ModuleInfo> path = new List<ModuleInfo>(pathLength);
        List<ModuleInfo> firstMods = GetPrimaryMods(used.ToList());
        List<ModuleInfo> laterMods = used.Where(x => !firstMods.Contains(x)).ToList();
        Dictionary<Dir, List<ModuleInfo>> values = new Dictionary<Dir, List<ModuleInfo>>()
                    { {Dir.Up, new List<ModuleInfo>() }, {Dir.Right, new List<ModuleInfo>() }, {Dir.Down, new List<ModuleInfo>() }, {Dir.Left, new List<ModuleInfo>()} };
        foreach (ModuleInfo info in firstMods)
            for (int i = 0; i < 4; i++)
                if (info.Includes((Dir)i))
                    values[(Dir)i].Add(info);
        var primaryEntry = values.Where(entry => entry.Value.Count == values.Max(x => x.Value.Count)).PickRandom();
        Dir primDir = primaryEntry.Key;
        List<ModuleInfo> primMods = primaryEntry.Value.Shuffle();
        firstMods.RemoveMany(primMods);
        Debug.Log(primMods.Count);
        int takenFromPrimaryCount = Enumerable.Range(1, Math.Min(primMods.Count, pathLength))
                                    .SelectMany(val => Enumerable.Repeat(val, Math.Min(1, (int)(val / 3.5))))
                                    .PickRandom();
        path.AddRange(primMods.Take(takenFromPrimaryCount));
        while (path.Count < pathLength)
        {
            if (firstMods.Count > 0)
                path.Add(firstMods.PopAt(0));
            else path.Add(laterMods.PopAt(0));
        }

        Log("Selecting {0} mods for the pre-generated path.", pathLength);
        Log("Found {0} mods with the primary chosen direction ({1}).", primMods.Count, primDir);
        return path;
    }
    List<ModuleInfo> GetPrimaryMods(List<ModuleInfo> infos)
    {
        if (Bomb.GetModuleNames().Contains("Turn The Keys"))
            infos = FilterWithTTKS(infos);
        if (Bomb.GetModuleNames().Contains("Custom Keys"))
            FilterWithCustomKeys(infos);
        if (Bomb.GetModuleNames().Contains("Mystery Module"))
            FilterWithMM(infos);
        return infos.Shuffle();
    }
    List<ModuleInfo> FilterWithTTKS(List<ModuleInfo> infos)
    {
        Log("Turn The Keys found, filtering by its ignore list.");
        string[] after = { "Maze", "Memory", "Complicated Wires", "Wire Sequence", "Cryptography", "Semaphore", "Combination Lock", "Simon Says", "Astrology", "Switches", "Plumbing" };
        return infos.Where(inf => !after.Contains(inf.modName)).ToList();
    }
    List<ModuleInfo> FilterWithCustomKeys(List<ModuleInfo> infos)
    {
        Log("Custom Keys found, filtering by its ignore list.");
        var script = ReflectionHelper.FindType("RemoteTurnTheKeysScript");
        var instance = FindObjectOfType(script);
        List<string> after  = instance.GetValue<string[]>("leftKeyAfter").Concat(
                              instance.GetValue<string[]>("rightKeyAfter")).ToList();
        return infos.Where(inf => !after.Contains(inf.modName)).ToList();
    }
    List<ModuleInfo> FilterWithMM(List<ModuleInfo> infos)
    {
        Log("Mystery module found, filtering by its module list");
        var script = ReflectionHelper.FindType("MysteryModuleScript");
        var instance = FindObjectOfType(script);
        List<string> keys = instance.GetValue<List<KMBombModule>>("keyModules").Select(x => x.ModuleDisplayName).ToList();
        return infos.Where(inf => !keys.Contains(inf.modName)).ToList();
    }

    void Update()
    {
        float multiplier = Time.deltaTime;
        multiplier *= centerHeld ? 90 : 20;
        multiplier *= solvedModsCount % 2 == 0 ? +1 : -1;
        centerButton.transform.localEulerAngles += multiplier * Vector3.up;
    }

    IEnumerator MoveButton(float locA, float locB, float duration)
    {
        float delta = Mathf.InverseLerp(locA, locB, centerButton.transform.localPosition.y);
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            yield return null;
            centerButton.transform.localPosition = Mathf.Lerp(locA, locB, delta) * Vector3.up;
        }
    }

    void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Module Maneuvers #{0}] {1}", moduleId, string.Format(msg, args));
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} foobar> to do something.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        yield return null;
    }
}
