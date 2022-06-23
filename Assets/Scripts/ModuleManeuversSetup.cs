using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KModkit;

public class ModuleManeuversSetup {

    private static readonly string[] ttksMods = { "Password", "Who’s On First", "Crazy Talk", "Keypad", "Listening", "Orientation Cube", "Morse Code", "Wires", "Two Bits", "The Button", "Colour Flash", "Round Keypad" };
    private static readonly string[] dictatorPriorityOrder = { "Mystery Module", "Custom Keys", "Turn The Keys" };

    public List<ModuleManeuversScript> maneuvers;
    public string[] ignored;
    public bool hasStarted, isFinished;

    private string[] _modNames;
    private string[] _availableMods;
    private int _modId;
    private string _sn;

    private Dictionary<string, List<string>> _pickFirstSets = new Dictionary<string, List<string>>();

    public List<string> finalOrder;
    public List<string> orglist = null;

    public List<ModuleInfo> chosenPathForThisSN;

    public ModuleManeuversSetup(int id, string sn, List<ModuleManeuversScript> maneuvers, string[] allMods, string[] ignored)
    {
        _modId = id;
        _sn = sn;
        this.maneuvers = maneuvers;
        this.ignored = ignored;
        _modNames = allMods;
        _availableMods = allMods.Where(m => !ignored.Contains(m)).ToArray();

    }
    
    public IEnumerator GetMods()
    {
        hasStarted = true;
        yield return CheckForSpecials();
        GetOrder();
        isFinished = true;
    }
    private IEnumerator CheckForSpecials()
    {
        yield return null;
        if (_modNames.Contains("Organization"))
        { 
            int pathLength = Math.Max(1, _availableMods.Length / 3);
            Log("Organization detected, solving after {0} modules.", pathLength);
            var script = ReflectionHelper.FindType("OrganizationScript");
            var instance = UnityEngine.Object.FindObjectOfType(script);
            List<string> order;
            while (instance.GetValue<List<string>>("order") == null || instance.GetValue<List<string>>("order").Count == 0)
                yield return null;
            order = instance.GetValue<List<string>>("order");
            orglist = order.Take(pathLength).ToList();
            finalOrder = orglist;
        }
        else
        {
            if (_modNames.Contains("Turn The Keys"))
            {
                Log("Turn The Keys detected: filtering TTKS-required mods first.");
                _pickFirstSets.Add("Turn The Keys", _availableMods.Where(x => ttksMods.Contains(x)).ToList());
            }
            if (_modNames.Contains("Custom Keys"))
            {
                Log("Custom Keys detected: Grabbing required mods from the module.");
                var script = ReflectionHelper.FindType("RemoteTurnTheKeysScript");
                var instance = UnityEngine.Object.FindObjectOfType(script);
                while (instance.GetValue<List<string>>("leftKeyAfter") == null || instance.GetValue<List<string>>("rightKeyAfter") == null)
                    yield return null;
                List<string> beforeTurning = instance.GetValue<List<string>>("leftKeyAfter").Concat(
                                      instance.GetValue<List<string>>("rightKeyAfter")).ToList();
                _pickFirstSets.Add("Custom Keys", _availableMods.Where(x => beforeTurning.Contains(x)).ToList());
            }
            if (_modNames.Contains("Mystery Module"))
            {
                Log("Mystery module found, filtering by its module list");
                var script = ReflectionHelper.FindType("MysteryModuleScript");
                var instance = UnityEngine.Object.FindObjectOfType(script);
                while (instance.GetValue<List<KMBombModule>>("keyModules") == null)
                    yield return null;
                List<string> keys = instance.GetValue<List<KMBombModule>>("keyModules").Select(x => x.ModuleDisplayName).ToList();
                _pickFirstSets.Add("Mystery Module", keys);
            }
        }
    }

    void GetOrder()
    {
        if (orglist != null)
        {
            finalOrder = orglist;
            return;
        }
        foreach (var set in _pickFirstSets)
            set.Value.Shuffle();
        List<string> firstMods = _pickFirstSets.OrderBy(pair => Array.IndexOf(dictatorPriorityOrder, pair.Key)).SelectMany(e => e.Value).ToList();
        finalOrder = firstMods.Concat(_availableMods.Except(firstMods)).ToList();
        Debug.Log("firstMods count: " + firstMods.Count);
        Debug.Log("_availableMods count: " + _availableMods.Count());
    }

    private void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Module Maneuvers #{0}] {1}", _modId, string.Format(msg, args));
    }

}
