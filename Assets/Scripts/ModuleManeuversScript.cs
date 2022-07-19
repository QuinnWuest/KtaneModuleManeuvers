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
    private static Dictionary<string, ModuleManeuversSetup> setups = new Dictionary<string, ModuleManeuversSetup>();
    private ModuleManeuversSetup thisSetup;

    private List<ModuleInfo> usableInfos;
    private List<ModuleInfo> primaryMods;
    private Dictionary<string, ModuleInfo> modLookup = new Dictionary<string, ModuleInfo>();
    private List<string> solvedModules = new List<string>();


    private Coordinate endPos, currentPos;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool hasBeenSetUp;
    private State currentState = State.Active;
    private bool enteredInputYet = false;
    private int arrowPressesCount = 0;
    private int currentRecallStage = 1;

    private bool centerHeld;
    private Coroutine holdCoroutine;
    private float holdTime;

    private bool? tpLongHold = null;
    private bool autosolving;
    void Awake () {
        moduleId = moduleIdCounter++;
        centerButton.OnInteract += () => { Hold(); return false; };
        centerButton.OnInteractEnded += () => Release();
        Module.OnActivate += () => { Audio.PlaySoundAtTransform("intro", transform); };
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
        if (currentState == State.Active || (currentState == State.Input && holdTime < 1 && tpLongHold != true))
            SubmitAnswer();
        else if (currentState == State.Input)
            EnterStageRecall();
        else if (currentState == State.Recall)
            StartCoroutine(RecallStage());
        holdTime = 0;
    }
  
  IEnumerator Start()
    {
        yield return null;
        ignoredModules = ignoredModules ?? Boss.GetIgnoredModules("Module Maneuvers", new string[] { "14", "42", "501", "A>N<D", "Bamboozling Time Keeper", "Black Arrows", "Brainf---", "Busy Beaver", "Don't Touch Anything", "Floor Lights", "Forget Any Color", "Forget Enigma", "Forget Everything", "Forget Infinity", "Forget It Not", "Forget Maze Not", "Forget Me Later", "Forget Me Not", "Forget Perspective", "Forget The Colors", "Forget Them All", "Forget This", "Forget Us Not", "Iconic", "Keypad Directionality", "Kugelblitz", "Module Maneuvers", "Multitask", "OmegaDestroyer", "OmegaForest", "Organization", "Password Destroyer", "Purgatory", "RPS Judging", "Security Council", "Shoddy Chess", "Simon Forgets", "Simon's Stages", "Souvenir", "Tallordered Keys", "The Time Keeper", "Timing is Everything", "The Troll", "Turn The Key", "The Twin", "Übermodule", "Ultimate Custom Night", "The Very Annoying Button", "Whiteout" });
        string sn = Bomb.GetSerialNumber();
        if (!setups.ContainsKey(sn))
        {
            thisSetup = new ModuleManeuversSetup(moduleId, sn,
                                new List<ModuleManeuversScript>(),

                                Bomb.GetSolvableModuleNames().ToArray(),
                                //new[] { "3D Maze", "Adjacent Letters", "Adventure Game", "Alphabet", "Anagrams", "Astrology", "Battleship", "Big Button Translated", "Binary LEDs", "Bitmaps", "Bitwise Operations", "Blind Alley", "Boolean Venn Diagram", "Broken Buttons", "The Bulb", "The Button", "Caesar Cipher", "Cheap Checkout", "Chess", "Chord Qualities", "The Clock", "Colored Squares", "Color Math", "Colour Flash", "Combination Lock", "Complicated Buttons", "Complicated Wires", "Connection Check", "Coordinates", "Crazy Talk", "Creation", "Cryptography", "Double-Oh", "Emoji Math", "English Test", "Fast Math", "FizzBuzz", "Follow the Leader", "Foreign Exchange Rates", "Forget Me Not", "Friendship", "The Gamepad", "Hexamaze", "Ice Cream", "Keypad", "Laundry", "LED Encryption", "Letter Keys", "Light Cycle", "Listening", "Logic", "Maze", "Memory", "Microcontroller", "Minesweeper", "Modules Against Humanity", "Monsplode, Fight!", "Morse Code", "Morse Code Translated", "Morsematics", "Mouse In The Maze", "Murder", "Mystic Square", "Neutralization", "Number Pad", "Only Connect", "Orientation Cube", "Password", "Passwords Translated", "Perspective Pegs", "Piano Keys", "Plumbing", "Point of Order", "Probing", "Resistors", "Rhythms", "Rock-Paper-Scissors-L.-Sp.", "Round Keypad", "Rubik's Cube", "Safety Safe", "The Screw", "Sea Shells", "Semaphore", "Shape Shift", "Silly Slots", "Simon Says", "Simon Screams", "Simon States", "Skewed Slots", "Souvenir", "Square Button", "Switches", "Symbolic Password", "Text Field", "Third Base", "Tic Tac Toe", "Turn The Key", "Turn The Keys", "Two Bits", "Web Design", "Who's on First", "Who's on First Translated", "Wire Placement", "Wires", "Wire Sequence", "Word Scramble", "Word Search", "Yahtzee", "Zoo" },

                                ignoredModules);
            setups.Add(sn, thisSetup);
        }
        thisSetup = setups[sn];
        thisSetup.maneuvers.Add(this);
        if (!thisSetup.hasStarted)
            StartCoroutine(thisSetup.GetMods());
        yield return new WaitUntil(() => thisSetup.isFinished);

        GetMovements();
        GetPositions();

        hasBeenSetUp = true;

    }
    void Update()
    {
        if (!hasBeenSetUp)
            return;
        if (centerHeld)
            holdTime += Time.deltaTime;

        float multiplier = Time.deltaTime;
        multiplier *= centerHeld ? 90 : 20;
        multiplier *= solvedModules.Count % 2 == 0 ? +1 : -1;
        centerButton.transform.localEulerAngles += multiplier * Vector3.up;

        
        var currentSolves = Bomb.GetSolvedModuleNames().Except(ignoredModules);
        if (currentSolves.Count() != solvedModules.Count)
        {
            //    Log("fhowfow " + currentSolves.Count() + " " + solvedModules.Count);
            List<string> newSolves = currentSolves.ToList();
            foreach (string str in solvedModules)
                newSolves.Remove(str);
        //    Log("{0} new solves", newSolves.Count());
            foreach (string solve in newSolves)
                MoveFromMod(modLookup[solve]);
        //    Log("Should be {0} solved opposed to {1}.", currentSolves.Count(), solvedModules.Count);
            solvedModules = currentSolves.ToList();
        }
        //else Log("kjdfslfajksdfljksdkfjl " + currentSolves.Count() + " " + solvedModules.Count);


        if (solvedModules.Count == usableInfos.Count && !enteredInputYet)
        {
            enteredInputYet = true;
            currentState = State.Input;
            StartCoroutine(EnterInput());
        }

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
            Log("{0} button: {1}", (Dir)i, usedButtons[i].name);
        }
        GetComponent<KMSelectable>().Children = new[] { centerButton }.Concat(usedButtons.Select(x => x.btn)).ToArray();
        GetComponent<KMSelectable>().UpdateChildrenProperly();
    }
    void GetMovements()
    {
        string[] modNames = Bomb.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).ToArray();
#if UNITY_EDITOR
        modNames = new[] { "3D Maze", "Adjacent Letters", "Adventure Game", "Alphabet", "Anagrams", "Astrology", "Battleship", "Big Button Translated", "Binary LEDs", "Bitmaps", "Bitwise Operations", "Blind Alley", "Boolean Venn Diagram", "Broken Buttons", "The Bulb", "The Button", "Caesar Cipher", "Cheap Checkout", "Chess", "Chord Qualities", "The Clock", "Colored Squares", "Color Math", "Colour Flash", "Combination Lock", "Complicated Buttons", "Complicated Wires", "Connection Check", "Coordinates", "Crazy Talk", "Creation", "Cryptography", "Double-Oh", "Emoji Math", "English Test", "Fast Math", "FizzBuzz", "Follow the Leader", "Foreign Exchange Rates", "Forget Me Not", "Friendship", "The Gamepad", "Hexamaze", "Ice Cream", "Keypad", "Laundry", "LED Encryption", "Letter Keys", "Light Cycle", "Listening", "Logic", "Maze", "Memory", "Microcontroller", "Minesweeper", "Modules Against Humanity", "Monsplode, Fight!", "Morse Code", "Morse Code Translated", "Morsematics", "Mouse In The Maze", "Murder", "Mystic Square", "Neutralization", "Number Pad", "Only Connect", "Orientation Cube", "Password", "Passwords Translated", "Perspective Pegs", "Piano Keys", "Plumbing", "Point of Order", "Probing", "Resistors", "Rhythms", "Rock-Paper-Scissors-L.-Sp.", "Round Keypad", "Rubik's Cube", "Safety Safe", "The Screw", "Sea Shells", "Semaphore", "Shape Shift", "Silly Slots", "Simon Says", "Simon Screams", "Simon States", "Skewed Slots", "Souvenir", "Square Button", "Switches", "Symbolic Password", "Text Field", "Third Base", "Tic Tac Toe", "Turn The Key", "Turn The Keys", "Two Bits", "Web Design", "Who's on First", "Who's on First Translated", "Wire Placement", "Wires", "Wire Sequence", "Word Scramble", "Word Search", "Yahtzee", "Zoo" };
#endif
        usableInfos = new List<ModuleInfo>(modNames.Length);
        Log("Found {0} modules on the bomb.", modNames.Length);
        foreach (string name in modNames)
        {
            ModuleInfo info = new ModuleInfo(name, usedButtons);
            usableInfos.Add(info);
            Log(info.ToString());
        }
        HashSet<string> mods = new HashSet<string>();
        foreach (ModuleInfo mod in usableInfos)
            if (mods.Add(mod.modName))
                modLookup.Add(mod.modName, mod);
    }
    void GetPositions()
    {
        currentPos = new Coordinate(0, 0);

        if (thisSetup.chosenPathForThisSN == null)
            thisSetup.chosenPathForThisSN = GetPath(thisSetup.finalOrder.Select(str => modLookup[str]).ToArray());
        List<ModuleInfo> path = thisSetup.chosenPathForThisSN;
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
        Log("Goal position: {0}.", endPos);

    }

    IEnumerator EnterInput()
    {

        
        Log("Entering input.");
        for (int i = 0; i < 4; i++)
            StartCoroutine(usedButtons[i].RotateToCenter());
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 4; i++)
        {
            int ix = i;
            usedButtons[ix].SetChildStatus(true);
            usedButtons[ix].btn.OnInteract += () => { ArrowPress((Dir)ix); return false; };
        }
    }
    void ArrowPress(Dir d)
    {
        usedButtons[(int)d].btn.AddInteractionPunch(0.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, usedButtons[(int)d].transform);
        if (currentState == State.Input)
        {
            arrowPressesCount++;
            Coordinate next = currentPos.ApplyMovement(d);
            Log("Manually moved {0} from {1} to {2}. {3}", d, currentPos, next, arrowPressesCount % 2 == 0 ? "Strike!" : "");
            currentPos = next;
            if (arrowPressesCount % 2 == 0 && !autosolving)
                Module.HandleStrike();
        }
        else if (currentState == State.Recall)
        {
            int[] offsets = { +5, +1, -5, -1 };
            currentRecallStage += offsets[(int)d];
            currentRecallStage = currentRecallStage.Clamp(1, solvedModules.Count);
            SetDisplays(currentRecallStage);
        }
    }

    void MoveFromMod(ModuleInfo mod)
    {
        Log("Solved {0}, which corresponds to directions {1}.", mod.modName, mod.directions.Select(d => d.ToString()[0]).Join(""));
        Coordinate newPos = currentPos.ApplyMovement(mod);
        if (currentPos != newPos)
        {
            Log("Moved from {0} to {1}.", currentPos, newPos);
            currentPos = newPos;
        }
    }

    void SubmitAnswer()
    {
        if (currentPos == endPos)
        {
            currentState = State.Solved;
            Log("Submitted at the goal position, module solved!");
            leftDisp.text = "!!!";
            rightDisp.text = "!!!";
            leftDisp.color = new Color32(0xFD, 0x85, 0xFF, 0xFF);
            rightDisp.color = new Color32(0xFD, 0x85, 0xFF, 0xFF);
            for (int i = 0; i < 4; i++)
                usedButtons[i].SetChildStatus(false);
            Module.HandlePass();
            Audio.PlaySoundAtTransform("outro", transform);
        }
        else
        {
            Log("Submitted at {0}. Strike!", currentPos);
            Module.HandleStrike();
        }
    }
    List<ModuleInfo> GetPath(ModuleInfo[] used)
    {
        List<ModuleInfo> path;
        int pathLength;
        if (thisSetup.orglist == null)
        {
            pathLength = Math.Max(1, used.Length / 3);
            path = used.Take(pathLength).ToList();
        }
        else
        {
            Log("ORGLISTCOUNT: " + thisSetup.orglist.Count);
            pathLength = thisSetup.orglist.Count;
            path = thisSetup.orglist.Select(x => modLookup[x]).ToList();
        }

        /*
        foreach (string info in order)
            for (int i = 0; i < 4; i++)
                if (modLookup[info].Includes((Dir)i))
                    values[(Dir)i].Add(modLookup[info]);
        var primaryEntry = values.Where(entry => entry.Value.Count == values.Max(x => x.Value.Count)).PickRandom();
        Dir primDir = primaryEntry.Key;
        List<ModuleInfo> primMods = primaryEntry.Value.Shuffle();
        primMods.RemoveMany(primMods);
        
        //int takenFromPrimaryCount = Enumerable.Range(1, Math.Min(primMods.Count, pathLength))
        //                            .SelectMany(val => Enumerable.Repeat(val, Math.Max(1, val < primMods.Count / 2 ? val : primMods.Count - val)))
        //                            .PickRandom();
        int takenFromPrimaryCount = Mathf.CeilToInt(primMods.Count / 2f);
        path.AddRange(primMods.Take(takenFromPrimaryCount));
        while (path.Count < pathLength && (laterMods.Count > 0 || firstMods.Count > 0))
        {
            if (firstMods.Count > 0)
                path.Add(firstMods.PopAt(0));
            else path.Add(laterMods.PopAt(0));
        }
        
        path.AddRange(firstMods.Take(pathLength));
        for (int i = 0; path.Count != pathLength && i < laterMods.Count; i++)
            path.Add(laterMods[i]);
        */

        Log("Selecting {0} mods for the pre-generated path.", pathLength);
        //Log("Found {0} mods with the primary chosen direction ({1}).", primMods.Count, primDir);
        return path;
    }

    void EnterStageRecall()
    {
        currentState = State.Recall;
        SetDisplays(1);
    }
    IEnumerator RecallStage()
    {
        for (int i = 0; i < 4; i++)
            usedButtons[i].SetChildStatus(false);
        Log("Recalling stages {0}–{1} (this will strike).", currentRecallStage, (currentRecallStage + 2).Clamp(0, solvedModules.Count));
        currentState = State.RecallingProcess;
        int soundIx = 0;
        for (int stage = currentRecallStage - 1; stage < solvedModules.Count && stage < currentRecallStage + 2; stage++)
        {
            SetDisplays(stage + 1);
            Audio.PlaySoundAtTransform("flash" + soundIx++, transform);
            for (int arrowPos = 0; arrowPos < 4; arrowPos++)
                usedButtons[arrowPos].SetRendererFlash(modLookup[solvedModules[stage]].directions.Contains((Dir)arrowPos));
            yield return new WaitForSeconds(0.75f);
            for (int arrowPos = 0; arrowPos < 4; arrowPos++)
                usedButtons[arrowPos].SetRendererFlash(false);
            yield return new WaitForSeconds(0.75f);
        }
        Module.HandleStrike();
        currentState = State.Input;
        leftDisp.text = endPos.x.ToString();
        rightDisp.text = endPos.y.ToString();
        for (int i = 0; i < 4; i++)
            usedButtons[i].SetChildStatus(true);
    }
    void SetDisplays(int value)
    {
        leftDisp.text = value < 100 ? "" : (value / 100).ToString();
        rightDisp.text = (value % 100).ToString();
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
    private readonly string TwitchHelpMessage = @"Use [!{0} submit] to press the center button. Use [!{0} press/move URDL] to move in those directions. Use [!{0} recall] to hold the center button. Note that movement commands will not halt the command if a strike is encountered.";
#pragma warning restore 414
    IEnumerator Press(KMSelectable btn, float wait)
    {
        btn.OnInteract();
        yield return new WaitForSeconds(wait);
    }
    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        Match m = Regex.Match(command, @"^(?:(?:PRESS|MOVE)\s+)?([URDL]+)$");
        if (command == "SUBMIT")
        {
            yield return null;
            tpLongHold = false;
            yield return Press(centerButton, 0.25f);
            centerButton.OnInteractEnded();
        }
        else if (command == "RECALL" && currentState == State.Input)
        {
            yield return null;
            tpLongHold = true;
            yield return Press(centerButton, 0.25f);
            centerButton.OnInteractEnded();
        }
        else if (m.Success)
        {
            yield return null;
            if (currentState == State.Input)
                yield return "multiple strikes";
            foreach (char ch in m.Groups[1].Value)
            {
                yield return Press(usedButtons["URDL".IndexOf(ch)].btn, 0.15f);
            }
            yield return "end multiple strikes";
        }
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        autosolving = true;
        while (currentState != State.Input)
            yield return true;
        while (currentPos.x != endPos.x)
            yield return Press(currentPos.x < endPos.x ? usedButtons[1].btn : usedButtons[3].btn, 0.15f);
        while (currentPos.y != endPos.y)
            yield return Press(currentPos.y < endPos.y ? usedButtons[0].btn : usedButtons[2].btn, 0.15f);
        tpLongHold = false;
        yield return Press(centerButton, 0.15f);
        centerButton.OnInteractEnded();
    }
}
