// GameManager: game state machine, objective tracking, win/lose, restart.
// Everything else reacts to its events, so UI / audio / (later) networking stay decoupled.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastNight
{
    public enum GameState { MainMenu, Loading, Playing, Paused, GameOver, Escaped }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        static bool skipMenuOnLoad;           // set by Restart()

        [Header("Objective")]
        public int fusesRequired = 3;
        [Tooltip("Seconds before the monster may hunt at all (fairness).")]
        public float graceSeconds = 20f;

        public GameState State { get; private set; }
        public int FusesCollected { get; private set; }   // picked up (in inventory or inserted)
        public int FusesInserted { get; private set; }
        public bool GeneratorRunning { get; private set; }
        public bool GateOpen { get; private set; }
        public int NotesFound { get; private set; }
        public float PlayTime { get; private set; }
        public bool InGrace { get { return PlayTime < graceSeconds; } }

        public PlayerController Player { get; private set; }
        public MonsterAI Monster { get; private set; }

        public event Action<GameState> StateChanged;
        public event Action<string> ObjectiveChanged;
        public event Action<string, float> Message;        // text, seconds
        public event Action GeneratorStarted;
        public event Action<string, string> NoteOpened;     // title, body

        public bool IsPlaying { get { return State == GameState.Playing; } }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            GameSettings.Apply();
        }

        void Start()
        {
            if (skipMenuOnLoad) { skipMenuOnLoad = false; StartGame(); }
            else SetState(GameState.MainMenu);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(PlayerController p) { Player = p; }
        public void Register(MonsterAI m) { Monster = m; }

        void Update()
        {
            if (State == GameState.Playing)
            {
                PlayTime += Time.deltaTime;
                InputBridge.Poll();
                if (InputBridge.PausePressed) Pause();
            }
            else if (State == GameState.Paused)
            {
                InputBridge.Poll();
                if (InputBridge.PausePressed) Resume();
            }
        }

        // ---------------- flow ----------------
        public void StartGame() { StartCoroutine(LoadRoutine()); }

        IEnumerator LoadRoutine()
        {
            SetState(GameState.Loading);
            // Real work happens here: procedural audio clips are synthesised over several frames.
            yield return ProceduralAudio.GenerateAll();
            if (AudioManager.Instance != null) AudioManager.Instance.StartAmbience();
            yield return new WaitForSecondsRealtime(0.4f);
            FusesCollected = FusesInserted = NotesFound = 0;
            PlayTime = 0f;
            GeneratorRunning = GateOpen = false;
            SetState(GameState.Playing);
            Objective();
            Say("Car band. Network gayab. Gate band hai... generator chalu karna padega.", 5f);
        }

        public void Pause() { if (State == GameState.Playing) SetState(GameState.Paused); }
        public void Resume() { if (State == GameState.Paused) SetState(GameState.Playing); }

        public void Restart()
        {
            skipMenuOnLoad = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ToMainMenu()
        {
            skipMenuOnLoad = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void SetState(GameState s)
        {
            State = s;
            Time.timeScale = s == GameState.Paused ? 0f : 1f;
            bool lockCursor = s == GameState.Playing && !Application.isMobilePlatform;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;
            if (s != GameState.Playing) InputBridge.ResetAll();
            if (StateChanged != null) StateChanged(s);
        }

        // ---------------- objective ----------------
        public void OnFusePicked()
        {
            FusesCollected++;
            Say(string.Format("Fuse mila ({0}/{1})", FusesCollected, fusesRequired), 2.5f);
            Objective();
        }

        public void OnFuseInserted()
        {
            FusesInserted++;
            Objective();
        }

        public void OnGeneratorStarted()
        {
            GeneratorRunning = true;
            if (GeneratorStarted != null) GeneratorStarted();
            Say("Generator chal gaya! Gate khul raha hai... wo bhi sun chuka hai.", 4f);
            Objective();
        }

        public void OnGateOpened() { GateOpen = true; Objective(); }

        public void OnNoteRead(string title, string body)
        {
            NotesFound++;
            if (NoteOpened != null) NoteOpened(title, body);
        }

        public void PlayerEscaped() { if (IsPlaying) SetState(GameState.Escaped); }
        public void PlayerDied() { if (IsPlaying) SetState(GameState.GameOver); }

        public void Say(string text, float seconds = 3f) { if (Message != null) Message(text, seconds); }

        void Objective()
        {
            string t;
            if (GateOpen) t = "Gate se bhaago!";
            else if (GeneratorRunning) t = "Gate khul raha hai... zinda raho";
            else if (FusesCollected < fusesRequired) t = string.Format("Fuses dhoondo ({0}/{1})", FusesCollected, fusesRequired);
            else t = string.Format("Pump-house generator theek karo ({0}/{1} lage)", FusesInserted, fusesRequired);
            if (ObjectiveChanged != null) ObjectiveChanged(t);
        }
    }
}
