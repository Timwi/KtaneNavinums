using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class navinumsScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMRuleSeedable RuleSeedable;

    //0 Top, 1 Left, 2 Right, 3 Bottom, 4 Center
    public KMSelectable[] Displays;
    public TextMesh[] Digits;

    static int moduleIdCounter = 1;
    int moduleId;
    bool moduleSolved = false;
    int stage = -1;
    int y;
    int x;
    int middleDisplayedDigit;

    int[][] grid = new int[][]
    {
        new int[3],
        new int[3],
        new int[3]
    };
    int[][] lookUp = new int[][]
    {
        new int[8],
        new int[8],
        new int[8],
        new int[8],
        new int[8],
        new int[8],
        new int[8],
        new int[8],
        new int[8]
    };

    List<int> directions = new List<int>();
    List<int> directionsSorted = new List<int>();
    int center;

    KMSelectable.OnInteractHandler DisplayPress(int btn)
    {
        return delegate
        {
            if (moduleSolved)
                return false;
            Displays[btn].AddInteractionPunch();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Displays[btn].transform);
            if (stage > 7)
            {
                if (btn == 4)
                    CenterPressed();
            }
            else
                DirectionPressed(btn);
            return false;
        };
    }

    void DirectionPressed(int btn)
    {
        switch (btn)
        {
            case 0:
                if (directions[0] == directionsSorted[lookUp[center - 1][stage] - 1])
                {
                    y--;
                    if (y < 0)
                        y = 2;
                    GenerateStage();
                    break;
                }
                else
                {
                    Module.HandleStrike();
                    break;
                }
            case 1:
                if (directions[1] == directionsSorted[lookUp[center - 1][stage] - 1])
                {
                    x--;
                    if (x < 0)
                        x = 2;
                    GenerateStage();
                    break;
                }
                else
                {
                    Module.HandleStrike();
                    break;
                }
            case 2:
                if (directions[2] == directionsSorted[lookUp[center - 1][stage] - 1])
                {
                    x = (x + 1) % 3;
                    GenerateStage();
                    break;
                }
                else
                {
                    Module.HandleStrike();
                    break;
                }
            case 3:
                if (directions[3] == directionsSorted[lookUp[center - 1][stage] - 1])
                {
                    y = (y + 1) % 3;
                    GenerateStage();
                    break;
                }
                else
                {
                    Module.HandleStrike();
                    break;
                }
            default:
                break;
        }
    }

    void CenterPressed()
    {
        if (middleDisplayedDigit == grid[y][x])
        {
            moduleSolved = true;
            Module.HandlePass();
            for (int i = 0; i < Digits.Length; i++)
            {
                Digits[i].text = "↑,←,→,↓,Solved".Split(',')[i];
                Digits[i].characterSize = i < 4 ? Digits[i].characterSize / 2 : Digits[i].characterSize / 4;
                Digits[i].color = new Color32(0, 255, 0, 255);
            }

        }
        else
            Module.HandleStrike();
    }

    void Start()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < Displays.Length; i++)
        {
            Displays[i].OnInteract += DisplayPress(i);
        }


        center = Random.Range(1, 10);
        Setup();
        Debug.LogFormat(@"[Navinums #{0}] Center Display is: {1}.", moduleId, center);
        GenerateStage();

    }

    void Setup()
    {
        var rnd = RuleSeedable.GetRNG();
        var gridDigits = Enumerable.Range(1, 9).ToList();
        rnd.ShuffleFisherYates(gridDigits);
        var counter = 0;
        for (int i = 0; i < grid.Length; i++)
            for (int j = 0; j < grid[i].Length; j++)
            {
                grid[i][j] = gridDigits[counter];
                if (gridDigits[counter] == center)
                {
                    y = i;
                    x = j;
                }
                counter++;
            }

        for (int i = 0; i < lookUp.Length; i++)
            for (int j = 0; j < lookUp[i].Length; j++)
                lookUp[i][j] = rnd.Next(1, 5);

        Debug.LogFormat(@"[Navinums #{0}] Using Ruleseed {1}", moduleId, rnd.Seed);
        Debug.LogFormat(@"[Navinums #{0}] Using the following grid:", moduleId);
        for (int i = 0; i < grid.Length; i++)
            Debug.LogFormat(@"[Navinums #{0}] {1}", moduleId, grid[i].Join(", "));
        Debug.LogFormat(@"[Navinums #{0}] Using the following lookups:", moduleId);
        for (int i = 0; i < lookUp.Length; i++)
            Debug.LogFormat(@"[Navinums #{0}] {1}: {2}", moduleId, i + 1, lookUp[i].Join(", "));

    }

    void GenerateStage()
    {
        stage++;
        if (stage == 8)
        {
            for (int i = 0; i < Displays.Length; i++)
                Digits[i].text = "";
            StartCoroutine(PhaseTwo());
            return;
        }
        directions.Clear();
        directionsSorted.Clear();
        directions = Enumerable.Range(0, 10).ToList();
        directions.Shuffle();
        directions = directions.Take(4).ToList();
        directionsSorted.AddRange(directions);
        directionsSorted.Sort();
        Debug.LogFormat(@"[Navinums #{0}] Stage {1} - Directions are in reading order: {2}.", moduleId, stage + 1, directions.Join(", "));
        Debug.LogFormat(@"[Navinums #{0}] Stage {1} - Correct direction to press is: {2}.", moduleId, stage + 1, directionsSorted[lookUp[center - 1][stage] - 1]);
        for (int i = 0; i < Displays.Length; i++)
            Digits[i].text = i < 4 ? directions[i].ToString() : center.ToString();
    }

    private IEnumerator PhaseTwo()
    {
        Debug.LogFormat(@"[Navinums #{0}] The correct center digit is: {1}", moduleId, grid[y][x]);
        var numbers = Enumerable.Range(1, 9).ToList().Shuffle();
        var c = 0;
        while (!moduleSolved)
        {
            middleDisplayedDigit = numbers[c];
            Digits[4].text = middleDisplayedDigit.ToString();
            c = (c + 1) % numbers.Count;
            yield return new WaitForSeconds(1f);
        }
    }

#pragma warning disable 414
    public static string TwitchHelpMessage = @"!{0} top,left,right,bottom 0,1,2,3 n,w,e,s [press the corresponding display] | !{0} center/4/m <digit> [press the center display when the specified digit is shown]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;

        // Timed command
        if ((m = Regex.Match(command, @"^\s*(?:center|c|4|middle|m|press|submit)\s+([1-9])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            if (stage <= 7)
            {
                yield return "sendtochaterror We are not in the second stage yet.";
                yield break;
            }
            yield return null;

            var digit = int.Parse(m.Groups[1].Value);
            while (middleDisplayedDigit != digit)
                yield return "trycancel";
            yield return new[] { Displays[4] };
        }
        else
        {
            if (stage > 7)
            {
                yield return "sendtochaterror Invalid command for the second stage of this module.";
                yield break;
            }

            // Untimed command
            else if (Regex.IsMatch(command, @"^\s*(?:top|t|0|north|n|up|u)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;
                yield return new[] { Displays[0] };
            }
            else if (Regex.IsMatch(command, @"^\s*(?:left|l|1|west|w)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;
                yield return new[] { Displays[1] };
            }
            else if (Regex.IsMatch(command, @"^\s*(?:right|r|2|east|e)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;
                yield return new[] { Displays[2] };
            }
            else if (Regex.IsMatch(command, @"^\s*(?:bottom|b|3|south|s|down|d)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;
                yield return new[] { Displays[3] };
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (moduleSolved)
            yield break;

        Debug.LogFormat(@"[Navinums #{0}] Module was force solved by TP.", moduleId);
        while (stage <= 7)
        {
            var ix = directions.IndexOf(dir => dir == directionsSorted[lookUp[center - 1][stage] - 1]);
            Displays[ix].OnInteract();
            yield return new WaitForSeconds(.1f);
        }

        while (middleDisplayedDigit != grid[y][x])
            yield return true;

        Displays[4].OnInteract();
        yield return new WaitForSeconds(.1f);
    }
}
