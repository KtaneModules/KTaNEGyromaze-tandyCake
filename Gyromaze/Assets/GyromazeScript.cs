using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class GyromazeScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    public KMSelectable wheel, up, down;
    public MeshRenderer[] leds;
    public Material[] ledColors;
    public Material[] metals;
    private bool usingGold;
    private int[] ledVals;
    private int startPos, endPos, curPos;
    private int rotation = 0;
    private static readonly string[][] Mazes = new string[][]
    {
        new string[] {"URL", "UL", "UD", "UR", "RL", "DL", "UR", "RL", "RL", "UL", "RD", "RL", "DL", "RD", "UDL", "RD"},
        new string[] {"URL", "UL", "UD", "UR", "DL", "R", "UL", "RD", "UL", "RD", "L", "UR", "DL", "URD", "RDL", "RDL"},
        new string[] {"UL", "UD", "UR", "URL", "DL", "UR", "DL", "RD", "UL", "D", "U", "UR", "RDL", "UDL", "RD", "RDL"},
        new string[] {"UL", "UR", "UL", "UR", "RDL", "RL", "RL", "RL", "UL", "D", "RD", "RL", "DL", "URD", "UDL", "RD"},
        new string[] {"UL", "UR", "URL", "URL", "RDL", "DL", "R", "RL", "UL", "U", "RD", "RL", "RDL", "DL", "UD", "RD"},
        new string[] {"UL", "UD", "UD", "URD", "DL", "UR", "UDL", "UR", "URL", "DL", "UD", "R", "DL", "UD", "UD", "RD"},
        new string[] {"UL", "UD", "UR", "URL", "RL", "UDL", "D", "R", "RL", "URL", "RL", "RD", "DL", "RD", "DL", "URD"},
        new string[] {"UDL", "U", "UR", "URL", "UL", "RD", "DL", "R", "L", "URD", "URL", "RL", "DL", "URD", "DL", "RD"},
        new string[] {"URL", "UL", "UD", "UR", "L", "RD", "UL", "RD", "RL", "URL", "DL", "UR", "DL", "D", "URD", "RDL"},
        new string[] {"URL", "UL", "UD", "UR", "RL", "L", "UR", "RDL", "DL", "R", "DL", "UR", "UDL", "D", "URD", "RDL"}
    };

    void Awake () {
        moduleId = moduleIdCounter++;
        wheel.OnInteract += delegate () { wheel.AddInteractionPunch(0.5f); StartCoroutine(Rotate()); return false; };
        up.OnInteract += delegate ()
        {
            up.AddInteractionPunch(0.25f);
            Move(rotation % 4);
            StartCoroutine(Rotate());
            return false;
        };
        down.OnInteract += delegate ()
        {
            down.AddInteractionPunch(0.25f);
            Move((rotation + 2) % 4);
            StartCoroutine(Rotate());
            return false;
        };

    }

    void Start ()
    {
        SetMetals();
    }

    void Move(int direction)
    {

    }

    void SetMetals()
    {
        usingGold = UnityEngine.Random.Range(0, 2) == 0;
        up.GetComponent<MeshRenderer>().material = metals[usingGold ? 1 : 0];
        down.GetComponent<MeshRenderer>().material = metals[usingGold ? 1 : 0];
        wheel.GetComponent<MeshRenderer>().material = metals[usingGold ? 1 : 0];
        leds[0].material = ledColors[Rnd.Range(0, 4)];
        leds[1].material = ledColors[Rnd.Range(0, 4)];

    }

    IEnumerator Rotate()
    {
        rotation++;
        rotation %= 4;
        yield return null;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
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
