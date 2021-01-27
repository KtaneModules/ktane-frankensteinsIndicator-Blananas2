using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class frankensteinsIndicatorScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    int chosenIndicator = -1;
    private List<string> indNames = new List<string> { "BOB", "CAR", "CLR", "FRK", "FRQ", "IND", "NSA", "MSA", "SIG", "SND", "TRN" };
    public Material[] eyeColors;
    public GameObject[] Corneas;
    public TextMesh NameTagName;
    string emptySpaces = "##.#.###.....#.......#..#..#.......#.....###.#.##";
    int targetMood = -1;
    int targetsY = -1;
    int targetsX = -1;
    public List<int> invalidMoods = new List<int> { };
    private List<string> emoticons = new List<string> { "", "", ":}", "", ":D", "", "", "", "B|", "O.O", ":)", ";)", ">)", "", "O_O", "L)", "B(", "8)", ":|", "8}", "D8", "", ">{", "D<", "", "8(", "8{", "", ">#", ":", "8]", ">(", ">o", ":[", "DB", "", "8|", ">|", ":(", ":]", ":o", "", "", "", ">]", "", ":{", "", "" };
    int currentMood = -1;
    int currentX = -1;
    int currentY = -1;
    float leftEyeSize = 1;
    float rightEyeSize = 1;
    public TextMesh LittleGuy;
    int movement = -1;
    private List<string> movementNames = new List<string> { "Up", "Right", "Down", "Left" };
    private List<string> coords = new List<string> { "A1", "B1", "C1", "D1", "E1", "F1", "G1", "A2", "B2", "C2", "D2", "E2", "F2", "G2", "A3", "B3", "C3", "D3", "E3", "F3", "G3", "A4", "B4", "C4", "D4", "E4", "F4", "G4", "A5", "B5", "C5", "D5", "E5", "F5", "G5", "A6", "B6", "C6", "D6", "E6", "F6", "G6", "A7", "B7", "C7", "D7", "E7", "F7", "G7", };
    bool youScrewedUp = false;
    int beforeMovement = -1;
    string theseAreBad = "";

    public GameObject[] Eyelids; //Left, Right
    public GameObject[] HalfOpens; //Left, Right
    public GameObject[] Happys; //Small, Medium, Large
    public GameObject[] Sads; //Small, Medium, Large
    public GameObject[] Oos; //Small, Medium, Large
    public GameObject[] Misc; //Straight, Blocked, Fucked
    public GameObject[] EyesInGeneral; //Left, Right

    public KMSelectable[] TheEyes;
    public KMSelectable Nametag;

    void Awake () {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable eye in TheEyes) {
            KMSelectable pressedEye = eye;
            eye.OnInteract += delegate () { eyePress(pressedEye); return false; };
        }

        Nametag.OnInteract += delegate () { nametagPress(); return false; };

        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    // Use this for initialization
    void Start () {
        chosenIndicator = UnityEngine.Random.Range(0, 11);
        Corneas[0].GetComponent<MeshRenderer>().material = eyeColors[chosenIndicator];
        Corneas[1].GetComponent<MeshRenderer>().material = eyeColors[chosenIndicator];
        NameTagName.text = "";

        if (Bomb.GetBatteryCount() == 4 && Bomb.GetBatteryHolderCount() == 2) { //row 1
            targetsY = 0;
        } else if (Bomb.GetBatteryCount() == 3 && Bomb.GetBatteryHolderCount() == 2) { //row 2
            targetsY = 1;
        } else if (Bomb.GetBatteryCount() == 0 && Bomb.GetBatteryHolderCount() == 0) { //row 3
            targetsY = 2;
        } else if (Bomb.GetBatteryCount() == 1 && Bomb.GetBatteryHolderCount() == 1) { //row 5
            targetsY = 4;
        } else if (Bomb.GetBatteryCount() == 2 && Bomb.GetBatteryHolderCount() == 2) { //row 6
            targetsY = 5;
        } else if (Bomb.GetBatteryCount() == 2 && Bomb.GetBatteryHolderCount() == 1) { //row 7
            targetsY = 6;
        } else { //row 4
            targetsY = 3;
        }

        switch (mod(Bomb.GetOnIndicators().Count() - Bomb.GetOffIndicators().Count() + 700, 7)) {
            case 3: targetMood = targetsY * 7; targetsX = 0; break; //A
            case 2: targetMood = targetsY * 7 + 1; targetsX = 1; break; //B
            case 1: targetMood = targetsY * 7 + 2; targetsX = 2; break; //C
            case 0: targetMood = targetsY * 7 + 3; targetsX = 3; break; //D
            case 6: targetMood = targetsY * 7 + 4; targetsX = 4; break; //E
            case 5: targetMood = targetsY * 7 + 5; targetsX = 5; break; //F
            case 4: targetMood = targetsY * 7 + 6; targetsX = 6; break; //G
            default: Debug.Log("Damnit."); break;
        }

        while (emptySpaces[targetMood] == '#') {
            switch (mod(Bomb.GetPortCount(), 4)) {
                case 0: targetMood -= 7; if (targetMood < 0) { targetMood += 49; } break;
                case 1: targetMood += 1; if (mod(targetMood, 7) == 0) { targetMood = mod(targetMood + 42, 49); } break;
                case 2: targetMood += 7; if (targetMood > 49) { targetMood -= 49; } break;
                case 3: targetMood -= 1; if (mod(targetMood, 7) == 6) { targetMood = mod(targetMood + 7, 49); } break;
                default: Debug.Log("Damnit."); break;
            }
        }

        Debug.LogFormat("[Frankenstein's Indicator #{0}] The target mood is at {1}.", moduleId, coords[targetMood]);
        targetsY = targetMood / 7;
        targetsX = mod(targetMood, 7);

        if (Bomb.GetSerialNumberLetters().Any(ch => "AEIOU".Contains(ch))) {
            //knight
            invalidMoods.Add(mod(targetsY + 2, 7) * 7 + mod(mod(targetsX + 1, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 2, 7) * 7 + mod(mod(targetsX + 6, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 5, 7) * 7 + mod(mod(targetsX + 1, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 5, 7) * 7 + mod(mod(targetsX + 6, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 1, 7) * 7 + mod(mod(targetsX + 2, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 1, 7) * 7 + mod(mod(targetsX + 5, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 6, 7) * 7 + mod(mod(targetsX + 2, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 6, 7) * 7 + mod(mod(targetsX + 5, 7) + 49, 49));
        } else {
            //bishop
            invalidMoods.Add(mod(targetsY + 1, 7) * 7 + mod(mod(targetsX + 1, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 2, 7) * 7 + mod(mod(targetsX + 2, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 3, 7) * 7 + mod(mod(targetsX + 3, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 1, 7) * 7 + mod(mod(targetsX + 6, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 2, 7) * 7 + mod(mod(targetsX + 5, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 3, 7) * 7 + mod(mod(targetsX + 4, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 6, 7) * 7 + mod(mod(targetsX + 1, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 5, 7) * 7 + mod(mod(targetsX + 2, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 4, 7) * 7 + mod(mod(targetsX + 3, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 6, 7) * 7 + mod(mod(targetsX + 6, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 5, 7) * 7 + mod(mod(targetsX + 5, 7) + 49, 49));
            invalidMoods.Add(mod(targetsY + 4, 7) * 7 + mod(mod(targetsX + 4, 7) + 49, 49));
        }

        for (int i = 0; i < invalidMoods.Count(); i++) {
            theseAreBad = theseAreBad + coords[invalidMoods[i]] + " ";
        }

        Debug.LogFormat("[Frankenstein's Indicator #{0}] Invalid moods: {1}", moduleId, theseAreBad);

        currentMood = UnityEngine.Random.Range(0, 49);
        while (invalidMoods.IndexOf(currentMood) != -1 || emptySpaces[currentMood] == '#') {
            currentMood = UnityEngine.Random.Range(0, 49);
        }
        currentY = currentMood / 7;
        currentX = mod(currentMood, 7);

        Debug.LogFormat("[Frankenstein's Indicator #{0}] The mood they started at is {1}.", moduleId, coords[currentMood]);
        ShowFace();
	}

    void OnActivate()
    {
        NameTagName.text = indNames[chosenIndicator];
    }

    void ShowFace () {
        leftEyeSize = 1;
        rightEyeSize = 1;
        for (int i = 0; i < 3; i++) {
            if (i != 2) {
                Eyelids[i].gameObject.SetActive(false);
                HalfOpens[i].gameObject.SetActive(false);
            }
            Happys[i].gameObject.SetActive(false);
            Sads[i].gameObject.SetActive(false);
            Oos[i].gameObject.SetActive(false);
            Misc[i].gameObject.SetActive(false);
        }

        switch (currentMood) {
            case 2: Happys[1].gameObject.SetActive(true); break;
            case 4: Happys[2].gameObject.SetActive(true); break;
            case 8: leftEyeSize = 1.2f; rightEyeSize = 1.2f; Misc[0].gameObject.SetActive(true); break;
            case 9: rightEyeSize = 2; Misc[2].gameObject.SetActive(true); break;
            case 10: Happys[0].gameObject.SetActive(true); break;
            case 11: Eyelids[1].gameObject.SetActive(true); Happys[0].gameObject.SetActive(true); break;
            case 12: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Happys[0].gameObject.SetActive(true); break;
            case 14: leftEyeSize = 2; Misc[2].gameObject.SetActive(true); break;
            case 15: leftEyeSize = 1.1f; Happys[0].gameObject.SetActive(true); break;
            case 16: leftEyeSize = 1.2f; rightEyeSize = 1.2f; Sads[0].gameObject.SetActive(true); break;
            case 17: Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Happys[0].gameObject.SetActive(true); break;
            case 18: leftEyeSize = 1.1f; rightEyeSize = 1.1f; Misc[0].gameObject.SetActive(true); break;
            case 19: Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Happys[1].gameObject.SetActive(true); break;
            case 20: Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Sads[2].gameObject.SetActive(true); break;
            case 22: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Sads[1].gameObject.SetActive(true); break;
            case 23: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Sads[2].gameObject.SetActive(true); break;
            case 25: Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Sads[0].gameObject.SetActive(true); break;
            case 26: Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Sads[1].gameObject.SetActive(true); break;
            case 28: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Misc[1].gameObject.SetActive(true); break;
            case 29: break; //do nothing
            case 30: Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Oos[2].gameObject.SetActive(true); break;
            case 31: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Sads[0].gameObject.SetActive(true); break;
            case 32: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Oos[0].gameObject.SetActive(true); break;
            case 33: Oos[1].gameObject.SetActive(true); break;
            case 34: leftEyeSize = 0.9f; rightEyeSize = 0.9f; Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Sads[2].gameObject.SetActive(true); break;
            case 36: Eyelids[0].gameObject.SetActive(true); Eyelids[1].gameObject.SetActive(true); Misc[0].gameObject.SetActive(true); break;
            case 37: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Misc[0].gameObject.SetActive(true); break;
            case 38: Sads[0].gameObject.SetActive(true); break;
            case 39: Oos[2].gameObject.SetActive(true); break;
            case 40: Oos[0].gameObject.SetActive(true); break;
            case 44: HalfOpens[0].gameObject.SetActive(true); HalfOpens[1].gameObject.SetActive(true); Oos[2].gameObject.SetActive(true); break;
            case 46: Sads[1].gameObject.SetActive(true); break;
            default: Debug.Log("Damnit."); break;
        }

        LittleGuy.text = emoticons[currentMood];

        EyesInGeneral[0].transform.localScale = new Vector3(leftEyeSize, leftEyeSize, leftEyeSize);
        EyesInGeneral[1].transform.localScale = new Vector3(rightEyeSize, rightEyeSize, rightEyeSize);
	}

    void eyePress (KMSelectable pressedEye) {
        beforeMovement = currentMood;
        youScrewedUp = false;
        pressedEye.AddInteractionPunch();
        if (pressedEye == TheEyes[0]) { //left
            if (mod((int)Math.Floor(Bomb.GetTime()), 2) == 0) { //even
                //LEFT
                currentX -= 1;
                movement = 3;
            } else { //odd
                //DOWN
                currentY += 1;
                movement = 2;
            }
        } else { //right
            if (mod((int)Math.Floor(Bomb.GetTime()), 2) == 0) { //even
                //RIGHT
                currentX += 1;
                movement = 1;
            } else { //odd
                //Up
                currentY -= 1;
                movement = 0;
            }
        }
        if (currentX <= -1) {
            currentX += 7;
        }
        if (currentX >= 7) {
            currentX -= 7;
        }
        if (currentY <= -1) {
            currentY += 7;
        }
        if (currentY >= 7) {
            currentY -= 7;
        }
        currentMood = currentY * 7 + currentX;

        while (emptySpaces[currentMood] == '#') {
            switch (movement) {
                case 0: currentMood -= 7; if (currentMood < 0) { currentMood += 49; } break;
                case 1: currentMood += 1; if (mod(currentMood, 7) == 0) { currentMood = mod(currentMood + 42, 49); } break;
                case 2: currentMood += 7; if (currentMood > 49) { currentMood -= 49; } break;
                case 3: currentMood -= 1; if (mod(currentMood, 7) == 6) { currentMood = mod(currentMood + 7, 49); } break;
                default: Debug.Log("Damnit."); break;
            }

            if (invalidMoods.IndexOf(currentMood) != -1) {
                Debug.LogFormat("[Frankenstein's Indicator #{0}] Trying to move {1} would result in an invalid mood at {2}. Strike!", moduleId, movementNames[movement], coords[currentMood]);
                GetComponent<KMBombModule>().HandleStrike();
                youScrewedUp = true;
            }
        }
        currentY = currentMood / 7;
        currentX = mod(currentMood, 7);
        if (invalidMoods.IndexOf(currentMood) != -1) {
            Debug.LogFormat("[Frankenstein's Indicator #{0}] Trying to move {1} would result in an invalid mood at {2}. Strike!", moduleId, movementNames[movement], coords[currentMood]);
            GetComponent<KMBombModule>().HandleStrike();
            youScrewedUp = true;
        } else {
            if (beforeMovement != currentMood) {
                Debug.LogFormat("[Frankenstein's Indicator #{0}] You moved {1}, you are now at {2}.", moduleId, movementNames[movement], coords[currentMood]);
            }
            ShowFace();
        }

        if (youScrewedUp) {
            currentMood = beforeMovement;
            currentY = currentMood / 7;
            currentX = mod(currentMood, 7);
        }
    }

    void nametagPress () {
        if (currentMood == targetMood) {
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Frankenstein's Indicator #{0}] You submitted on {1}, which is correct. Module solved.", moduleId, coords[currentMood]);
        } else {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Frankenstein's Indicator #{0}] You submitted on {1}, which is incorrect. Strike!", moduleId, coords[currentMood]);
        }
    }

    private int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <left/right> <even/odd> [Presses the left or right eye when the last digit of the timer is even or odd] | !{0} submit [Presses the nametag]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Nametag.OnInteract();
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 3)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 3)
            {
                string[] eyes = new string[] { "left", "right" };
                string[] parities = new string[] { "even", "odd" };
                if (!eyes.Contains(parameters[1].ToLower()))
                {
                    yield return "sendtochaterror!f The specified eye '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                if (!parities.Contains(parameters[2].ToLower()))
                {
                    yield return "sendtochaterror!f The specified parity '" + parameters[2] + "' is invalid!";
                    yield break;
                }
                while ((int)Bomb.GetTime() % 2 != Array.IndexOf(parities, parameters[2].ToLower())) { yield return "trycancel"; }
                TheEyes[Array.IndexOf(eyes, parameters[1].ToLower())].OnInteract();
            }
            else if (parameters.Length == 2)
            {
                string[] eyes = new string[] { "left", "right" };
                if (eyes.Contains(parameters[1].ToLower()))
                    yield return "sendtochaterror Please specify a parity to press the eye on!";
                else
                    yield return "sendtochaterror!f The specified eye '" + parameters[1] + "' is invalid!";
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify an eye and parity to press the eye on!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        var q = new Queue<int[]>();
        var allMoves = new List<Movement>();
        var startPoint = new int[] { currentX, currentY };
        var targets = new int[] { targetsX, targetsY };
        q.Enqueue(startPoint);
        while (q.Count > 0)
        {
            var next = q.Dequeue();
            if (next[0] == targets[0] && next[1] == targets[1])
                goto readyToSubmit;
            string paths = "";
            int[] modoffset = new int[] { -1, 1, 1, -1 };
            bool skip = false;
            while (emptySpaces[mod(next[1] + modoffset[0], 7) * 7 + next[0]] == '#') { if (invalidMoods.Contains(mod(next[1] + modoffset[0], 7) * 7 + next[0])) skip = true; modoffset[0]--; }
            if (!invalidMoods.Contains(mod(next[1] + modoffset[0], 7) * 7 + next[0]) && !skip) { paths += "U"; }
            skip = false;
            while (emptySpaces[next[1] * 7 + mod(next[0] + modoffset[1], 7)] == '#') { if (invalidMoods.Contains(next[1] * 7 + mod(next[0] + modoffset[1], 7))) skip = true; modoffset[1]++; }
            if (!invalidMoods.Contains(next[1] * 7 + mod(next[0] + modoffset[1], 7)) && !skip) { paths += "R"; }
            skip = false;
            while (emptySpaces[mod(next[1] + modoffset[2], 7) * 7 + next[0]] == '#') { if (invalidMoods.Contains(mod(next[1] + modoffset[2], 7) * 7 + next[0])) skip = true; modoffset[2]++; }
            if (!invalidMoods.Contains(mod(next[1] + modoffset[2], 7) * 7 + next[0]) && !skip) { paths += "D"; }
            skip = false;
            while (emptySpaces[next[1] * 7 + mod(next[0] + modoffset[3], 7)] == '#') { if (invalidMoods.Contains(next[1] * 7 + mod(next[0] + modoffset[3], 7))) skip = true; modoffset[3]--; }
            if (!invalidMoods.Contains(next[1] * 7 + mod(next[0] + modoffset[3], 7)) && !skip) { paths += "L"; }
            var cell = paths;
            var allDirections = "URDL";
            var offsets = new int[,] { { 0, modoffset[0] }, { modoffset[1], 0 }, { 0, modoffset[2] }, { modoffset[3], 0 } };
            for (int i = 0; i < 4; i++)
            {
                var check = new int[] { mod(next[0] + offsets[i, 0], 7), mod(next[1] + offsets[i, 1], 7) };
                if (cell.Contains(allDirections[i]) && !allMoves.Any(x => x.start[0] == check[0] && x.start[1] == check[1]))
                {
                    q.Enqueue(new int[] { mod(next[0] + offsets[i, 0], 7), mod(next[1] + offsets[i, 1], 7) });
                    allMoves.Add(new Movement { start = next, end = new int[] { mod(next[0] + offsets[i, 0], 7), mod(next[1] + offsets[i, 1], 7) }, direction = i });
                }
            }
        }
        throw new InvalidOperationException("There is a bug in autosolve generation.");
        readyToSubmit:
        if (allMoves.Count != 0) // Checks for position already being target
        {
            var target = new int[] { targetsX, targetsY };
            var lastMove = allMoves.First(x => x.end[0] == target[0] && x.end[1] == target[1]);
            var relevantMoves = new List<Movement> { lastMove };
            while (lastMove.start != startPoint)
            {
                lastMove = allMoves.First(x => x.end[0] == lastMove.start[0] && x.end[1] == lastMove.start[1]);
                relevantMoves.Add(lastMove);
            }
            for (int i = 0; i < relevantMoves.Count; i++)
            {
                while ((int)Bomb.GetTime() % 2 != ((relevantMoves[relevantMoves.Count - 1 - i].direction % 2 == 0) ? 1 : 0)) { yield return true; }
                TheEyes[(relevantMoves[relevantMoves.Count - 1 - i].direction < 2) ? 1 : 0].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
        Nametag.OnInteract();
    }
    class Movement
    {
        public int[] start;
        public int[] end;
        public int direction;
    }

}
