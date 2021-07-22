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
    private static readonly string[][] mazes = new string[][]
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
    private string[] chosenMaze;
    private static readonly int[] offsets = { -4, 1, 4, -1 };
    private static readonly string dirs = "URDL";
    private static readonly string[] dirNames = { "Up", "Right", "Down", "Left" };
    private Vector3 wheelAngle = 90 * Vector3.left;
    private Vector3 roundedAngle = 90 * Vector3.left;
    private Vector3 currentAngle = 90 * Vector3.left;
    private Coroutine rotate;
    private float wheelDelta = 0;

    void Awake () {
        moduleId = moduleIdCounter++;
        wheel.OnInteract += delegate () 
        {
            wheel.AddInteractionPunch(0.5f);
            if (rotate != null)
                StopCoroutine(rotate);
            rotate = StartCoroutine(Rotate());
            return false; };
        up.OnInteract += delegate ()
        {
            up.AddInteractionPunch(0.25f);
            Move(rotation % 4);
            return false;
        };
        down.OnInteract += delegate ()
        {
            down.AddInteractionPunch(0.25f);
            Move((rotation + 2) % 4);
            return false;
        };

    }

    void Start ()
    {
        SetMetals();
        GetPositions();
    }

    void Move(int direction)
    {
        if (moduleSolved)
            return;
        Debug.LogFormat("[Gyromaze #{0}] Pressed the {1} button.", moduleId, dirNames[(direction - rotation + 4) % 4]);
        if (chosenMaze[curPos].Contains(dirs[direction]))
        {
            Debug.LogFormat("[Gyromaze #{0}] Attempted to move {1} from cell {2}. Strike!", moduleId, dirNames[direction], curPos + 1);
            curPos = startPos;
            Module.HandleStrike();
        }
        else
        {
            curPos += offsets[direction];
            Debug.LogFormat("[Gyromaze #{0}] Moved {1} to cell {2}.", moduleId, dirNames[direction], curPos + 1);
            StartCoroutine(Rotate());
            if (curPos == endPos)
                Solve();
        }
    }

    void SetMetals()
    {
        usingGold = Rnd.Range(0, 2) == 0;
        up.GetComponent<MeshRenderer>().material = metals[usingGold ? 1 : 0];
        down.GetComponent<MeshRenderer>().material = metals[usingGold ? 1 : 0];
        wheel.GetComponent<MeshRenderer>().material = metals[usingGold ? 1 : 0];
        int mazeIx = Bomb.GetSerialNumberNumbers().Last() / 2;
        if (usingGold)
            mazeIx += 5;
        chosenMaze = mazes[mazeIx];
        Debug.LogFormat("[Gyromaze #{0}] Module background is {1}, used maze is maze {2} in reading order.", moduleId, usingGold ? "gold" : "silver", mazeIx + 1);
    }
    void GetPositions()
    {
        startPos = 4 * (Bomb.GetPortCount() % 4) + (Bomb.GetBatteryCount() % 4);
        do endPos = Rnd.Range(0, 16);
        while (FindPath(startPos, endPos).Length < 4);
        leds[0].material = ledColors[endPos % 4];
        leds[1].material = ledColors[endPos / 4];
        Debug.LogFormat("[Gyromaze #{0}] We are going from position {1} to position {2} in reading order.", moduleId, startPos + 1, endPos + 1);
        Debug.LogFormat("[Gyromaze #{0}] A correct path is {1}.", moduleId, FindPath(startPos, endPos));

    }
    void Solve()
    {
        moduleSolved = true;
        Module.HandlePass();
    }

    IEnumerator Rotate()
    {
        rotation++;
        rotation %= 4;
        Debug.LogFormat("[Gyromaze #{0}] Module rotated; the uppermost button will now move you {1}.", moduleId, dirNames[rotation]);
        wheelAngle += 90 * Vector3.up;
        wheelDelta = -1 * ((currentAngle.y - roundedAngle.y) / (roundedAngle.y - wheelAngle.y)); //Reverses the lerp function.
        while (wheelDelta < 1)
        {
            wheelDelta += 2.5f * Time.deltaTime;
            currentAngle = new Vector3(-90, Mathf.Lerp(roundedAngle.y, wheelAngle.y, wheelDelta), 0);
            wheel.transform.localEulerAngles = currentAngle;
            yield return null;
        }
        wheelDelta = 0;
        roundedAngle = wheelAngle;
    }
    string FindPath(int start, int end)
    {
        if (start == end)
            return string.Empty;
        Queue<int> q = new Queue<int>();
        List<Movement> allMoves = new List<Movement>();
        q.Enqueue(start);
        while (q.Count > 0)
        {
            int subject = q.Dequeue();
            for (int i = 0; i < 4; i++)
                if (!chosenMaze[subject].Contains(dirs[i]) && !allMoves.Any(x => x.start == subject + offsets[i]))
                {
                    q.Enqueue(subject + offsets[i]);
                    allMoves.Add(new Movement(subject, subject + offsets[i], i));
                }
            if (subject == end) break;
        }
        if (allMoves.Count != 0)
        {
            Movement lastMove = allMoves.First(x => x.end == end);
            List<Movement> path = new List<Movement>() { lastMove };
            while (lastMove.start != start)
            {
                lastMove = allMoves.First(x => x.end == lastMove.start);
                path.Add(lastMove);
            }
            path.Reverse();
            string solution = string.Empty;
            for (int i = 0; i < path.Count; i++)
                solution += path[i].direction;
            return solution;
        }
        else return string.Empty;
    }
    IEnumerable<int> GetAdjacents(int pos)
    {
        if (pos % 4 != 0) yield return pos - 1;
        if (pos % 4 != 3) yield return pos + 1;
        if (pos > 3) yield return pos - 4;
        if (pos < 12) yield return pos + 4;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} UDW> to press the up button, then the down button, then the wheel.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        Match m = Regex.Match(command, @"^(?:(?:PRESS|MOVE)\s+)?([UDW]+)$");
        if (m.Success)
        {
            foreach (char move in m.Groups[1].Value)
            {
                switch (move)
                {
                    case 'U': up.OnInteract(); break;
                    case 'D': down.OnInteract(); break;
                    case 'W': wheel.OnInteract(); break;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        while (!moduleSolved)
        {
            string path = FindPath(curPos, endPos);
            foreach (char dir in path)
            {
                int movement = dirs.IndexOf(dir);
                if (movement % 2 != rotation % 2)
                {
                    wheel.OnInteract();
                    yield return new WaitForSeconds(0.2f);
                }
                if ((movement + rotation) % 4 == 0)
                    up.OnInteract();
                else down.OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
    public class Movement
    {
        public int start { get; set; }
        public int end { get; set; }
        public char direction { get; set; }
        public Movement(int s, int e, int d)
        {
            start = s;
            end = e;
            direction = "ULDR"[d];
        }
    }
}
