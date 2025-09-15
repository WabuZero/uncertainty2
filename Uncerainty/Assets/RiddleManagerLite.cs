using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RiddleManagerLite : MonoBehaviour
{
    [Serializable]
    public class Riddle
    {
        // Include the correct SET of panels for this riddle (order does not matter)
        public List<SimpleColorPanel> solution = new List<SimpleColorPanel>();
    }

    [Serializable]
    public class OutcomeGroup
    {
        [Tooltip("Optional label (e.g., 'All Correct', 'R1 wrong', 'R1+R2+R4 wrong')")]
        public string label;
        [Tooltip("Enable all of these when this outcome is matched. (Drag SCENE objects here, not prefabs)")]
        public GameObject[] enable;
    }

    [Header("Riddles (create 5 entries; include the correct SET for each)")]
    public List<Riddle> riddles = new List<Riddle>();   // Expect 5

    [Header("Submit Gate")]
    [Tooltip("This object is active ONLY when UNIQUE inputs == expected count (usually 5).")]
    public GameObject submitBoolObject;

    [Header("End-of-Run Outcome Routing (32 entries for 5 riddles)")]
    [Tooltip("Index = bitmask of wrong riddles (bit0=R1, bit1=R2, bit2=R3, bit3=R4, bit4=R5). 0 = All correct. 31 = All wrong.")]
    public OutcomeGroup[] outcomeByMask = new OutcomeGroup[32];

    [Header("Finalize Behavior")]
    [Tooltip("After enabling the matched outcome, automatically restart to Riddle 1.")]
    public bool autoResetAfterFinalize = true;

    [Tooltip("Log the computed mask and details to the Console.")]
    public bool debugLogMask = true;

    [Header("Events (optional)")]
    public UnityEvent<int> onRiddleStarted;       // after StartRiddle(newIndex)
    public UnityEvent<int> onRiddleCompleted;     // fired if that riddle's submission was correct
    public UnityEvent onRiddleWrong;              // fired if that riddle's submission was wrong
    public UnityEvent<int> onRunFinalized;        // passes mask 0..31 when the 5th is submitted

    [Space(6)]
    public UnityEvent onCurrentRiddleReset;       // when inputs cleared
    public UnityEvent onAllRiddlesRestarted;      // when restarted to riddle 1 (normal restart)

    [Space(6)]
    public UnityEvent<int> onBeforeAdvance;       // before leaving current riddle
    public UnityEvent<int> onAfterAdvance;        // after StartRiddle(newIndex)

    [Space(6)]
    public UnityEvent onSubmit;                   // every time Submit() is called

    [Header("Per-Riddle Submit Events (fire on Submit for that riddle)")]
    public UnityEvent onSubmitRiddle1;
    public UnityEvent onSubmitRiddle2;
    public UnityEvent onSubmitRiddle3;
    public UnityEvent onSubmitRiddle4;
    public UnityEvent onSubmitRiddle5;

    // --- internal state ---
    private readonly List<SimpleColorPanel> _inputs = new List<SimpleColorPanel>();              // raw history (may contain duplicates)
    private readonly HashSet<SimpleColorPanel> _uniqueInputs = new HashSet<SimpleColorPanel>();  // set for orderless logic
    private int _currentIndex = 0;   // riddle index (0..4)
    private bool[] _wrong;           // length = riddles.Count (expected 5)
    private bool _finalized = false; // set true during FinalizeRun; cleared by resets

    void Start()
    {
        _wrong = new bool[Mathf.Max(1, riddles.Count)];
        EnsureIndexValid();
        StartRiddle(0);
    }

    void Update()
    {
        // Enforce submit gate every frame (unique count must match expected)
        if (submitBoolObject != null)
        {
            bool allow = (!_finalized) && (_uniqueInputs.Count == GetExpectedCount());
            if (submitBoolObject.activeSelf != allow)
                submitBoolObject.SetActive(allow);
        }

        // Optional: auto-heal index if someone modified riddles at runtime
        EnsureIndexValid();
    }

    // Panels call this when stepped on (via your proxies / GC). Order doesn't matter.
    public void RegisterStep(SimpleColorPanel panel)
    {
        if (!EnsureIndexValid()) return;
        if (_finalized) { if (debugLogMask) Debug.LogWarning("[RiddleManagerLite] RegisterStep ignored: run is finalized. Reset needed."); return; }
        if (panel == null) return;

        var sol = riddles[_currentIndex].solution;
        if (sol == null || sol.Count == 0) { if (debugLogMask) Debug.LogWarning("[RiddleManagerLite] No solution configured for this riddle."); return; }

        _inputs.Add(panel);       // keep history if you need it
        _uniqueInputs.Add(panel); // set used for gating + correctness
    }

    // Call this from your submit trigger (white panel / GC).
    public void Submit()
    {
        if (!EnsureIndexValid()) return;

        if (_finalized)
        {
            if (debugLogMask) Debug.LogWarning("[RiddleManagerLite] Submit ignored: run already finalized. Call ResetToFirstRiddle/QuietRestart first.");
            return;
        }

        onSubmit?.Invoke();

        // Per-riddle submit event (before correctness)
        switch (_currentIndex)
        {
            case 0: onSubmitRiddle1?.Invoke(); break;
            case 1: onSubmitRiddle2?.Invoke(); break;
            case 2: onSubmitRiddle3?.Invoke(); break;
            case 3: onSubmitRiddle4?.Invoke(); break;
            case 4: onSubmitRiddle5?.Invoke(); break;
        }

        var sol = riddles[_currentIndex].solution;
        if (sol == null || sol.Count == 0) { if (debugLogMask) Debug.LogWarning("[RiddleManagerLite] Submit: solution is empty."); return; }

        // ORDERLESS correctness: uniqueInputs must match the solution SET.
        bool correct = (_uniqueInputs.Count == sol.Count);
        if (correct)
        {
            for (int i = 0; i < sol.Count; i++)
            {
                var need = sol[i];
                if (need == null || !_uniqueInputs.Contains(need))
                {
                    correct = false;
                    break;
                }
            }
        }

        if (_wrong == null || _wrong.Length != riddles.Count)
            _wrong = new bool[riddles.Count];
        _wrong[_currentIndex] = !correct;

        if (correct) onRiddleCompleted?.Invoke(_currentIndex);
        else onRiddleWrong?.Invoke();

        // Clear visuals + inputs on submit
        ResetCurrentInputs();

        bool last = (_currentIndex >= riddles.Count - 1);
        onBeforeAdvance?.Invoke(_currentIndex);

        if (last)
        {
            FinalizeRun();
            return;
        }

        StartRiddle(_currentIndex + 1);
        onAfterAdvance?.Invoke(_currentIndex); // now equals the new index
    }

    // ---------- Finalize / Outcomes ----------
    private void FinalizeRun()
    {
        _finalized = true;

        int mask = ComputeWrongMask(); // 0..31
        if (debugLogMask)
        {
            string bits = Convert.ToString(mask, 2).PadLeft(Mathf.Clamp(riddles.Count, 1, 31), '0');
            Debug.Log($"[RiddleManagerLite] Finalize: wrongMask={mask} (bits {bits}, bit0=R1)");
        }

        // Enable matched outcome objects
        if (outcomeByMask != null && mask >= 0 && mask < outcomeByMask.Length)
        {
            var group = outcomeByMask[mask];
            if (group != null && group.enable != null)
            {
                for (int i = 0; i < group.enable.Length; i++)
                {
                    var go = group.enable[i];
                    if (go != null)
                    {
                        if (debugLogMask)
                            Debug.Log($"[RiddleManagerLite] Enabling outcome[{mask}] -> {go.name}");
                        go.SetActive(true);
                    }
                }
            }
            else if (debugLogMask)
            {
                Debug.LogWarning($"[RiddleManagerLite] No outcome group/objects configured for mask {mask}");
            }
        }

        onRunFinalized?.Invoke(mask);

        if (autoResetAfterFinalize)
        {
            ResetToFirstRiddle(true);
            onAfterAdvance?.Invoke(_currentIndex); // after StartRiddle(0)
        }
        else
        {
            ResetCurrentInputs();
            // Keep _finalized = true until caller restarts, to avoid accidental submits.
        }
    }

    private int ComputeWrongMask()
    {
        int count = Mathf.Min(riddles.Count, 31);
        int mask = 0;
        for (int i = 0; i < count; i++)
            if (_wrong != null && i < _wrong.Length && _wrong[i])
                mask |= (1 << i);
        return mask;
    }

    // ---------- Resets / Navigation ----------
    public void ResetCurrentInputs()
    {
        _inputs.Clear();
        _uniqueInputs.Clear();

        if (IsValid(_currentIndex) && riddles[_currentIndex].solution != null)
        {
            foreach (var p in riddles[_currentIndex].solution)
                if (p) p.ResetIndicator(); // panel script should turn off its own indicator
        }

        // Ensure submit gate flips OFF next Update
        if (submitBoolObject != null && submitBoolObject.activeSelf)
            submitBoolObject.SetActive(false);

        onCurrentRiddleReset?.Invoke();
    }

    // Normal restart (WILL fire onAllRiddlesRestarted)
    public void ResetToFirstRiddle(bool clearWrongFlags = false)
    {
        if (clearWrongFlags)
        {
            if (_wrong == null || _wrong.Length != riddles.Count)
                _wrong = new bool[riddles.Count];
            else
                Array.Clear(_wrong, 0, _wrong.Length);
        }

        _finalized = false; // re-arm submissions
        EnsureIndexValid();

        onBeforeAdvance?.Invoke(_currentIndex);
        StartRiddle(0);
        onAllRiddlesRestarted?.Invoke();
        onAfterAdvance?.Invoke(_currentIndex); // now 0
    }

    private void StartRiddle(int idx)
    {
        _currentIndex = Mathf.Clamp(idx, 0, Mathf.Max(0, riddles.Count - 1));
        ResetCurrentInputs();
        onRiddleStarted?.Invoke(_currentIndex);
    }

    private bool IsValid(int idx) => riddles != null && idx >= 0 && idx < riddles.Count;

    private bool EnsureIndexValid()
    {
        if (riddles == null || riddles.Count == 0)
        {
            _currentIndex = 0;
            if (debugLogMask) Debug.LogWarning("[RiddleManagerLite] No riddles configured.");
            return false;
        }
        if (_currentIndex < 0 || _currentIndex >= riddles.Count)
        {
            if (debugLogMask) Debug.LogWarning($"[RiddleManagerLite] _currentIndex out of range ({_currentIndex}). Clamping to 0.");
            _currentIndex = 0;
        }
        return true;
    }

    // Helpers (optional)
    public int GetCurrentRiddleIndex() => _currentIndex;
    public int GetExpectedCount() => IsValid(_currentIndex) ? (riddles[_currentIndex].solution?.Count ?? 0) : 0;
    public int GetWrongMask() => ComputeWrongMask();

    private void DebugState(string tag)
    {
        if (!debugLogMask) return;
        Debug.Log($"[RiddleManagerLite::{tag}] idx={_currentIndex}/{Mathf.Max(0, riddles.Count - 1)}, expected={GetExpectedCount()}, inputs={_inputs.Count}, unique={_uniqueInputs.Count}, finalized={_finalized}");
    }

    // ===========================================================
    // SILENT HELPERS (no events) for a totally quiet escape
    // ===========================================================
    // Turn OFF all known indicators across all riddles without firing any events.
    private void SilentClearAllIndicators()
    {
        var seen = new HashSet<SimpleColorPanel>();
        for (int r = 0; r < riddles.Count; r++)
        {
            var sol = riddles[r].solution;
            if (sol == null) continue;
            for (int i = 0; i < sol.Count; i++)
            {
                var p = sol[i];
                if (p != null && seen.Add(p))
                    p.ResetIndicator();
            }
        }
    }

    // ===========================================================
    // QUIET RESTART (selectively minimal events)
    // ===========================================================
    /// <summary>
    /// Clean restart to riddle 1 without firing onAllRiddlesRestarted.
    /// By default clears wrong flags. You can toggle whether to fire
    /// riddle-started and before/after-advance events.
    /// </summary>
    public void QuietRestartAll(bool clearWrongFlags = true, bool fireRiddleStartedEvent = false, bool fireAdvanceEvents = false)
    {
        if (clearWrongFlags)
        {
            if (_wrong == null || _wrong.Length != riddles.Count) _wrong = new bool[riddles.Count];
            else Array.Clear(_wrong, 0, _wrong.Length);
        }

        _finalized = false; // re-arm submissions

        int prevIndex = _currentIndex;

        if (fireAdvanceEvents) onBeforeAdvance?.Invoke(prevIndex);

        _currentIndex = 0;

        // Clear current inputs + indicators USING the normal method (fires onCurrentRiddleReset)
        _inputs.Clear();
        _uniqueInputs.Clear();
        SilentClearAllIndicators();
        if (submitBoolObject != null && submitBoolObject.activeSelf) submitBoolObject.SetActive(false);
        onCurrentRiddleReset?.Invoke();

        if (fireRiddleStartedEvent) onRiddleStarted?.Invoke(_currentIndex);
        if (fireAdvanceEvents) onAfterAdvance?.Invoke(_currentIndex);

        if (debugLogMask) Debug.Log("[RiddleManagerLite] QuietRestartAll completed (no onAllRiddlesRestarted).");
    }

    // ===========================================================
    // QUIET RESTART — NO EVENTS AT ALL (your escape)
    // ===========================================================
    /// <summary>
    /// Fully silent restart to riddle 1. No events are invoked.
    /// Clears wrong flags, inputs, all indicators, and submit gate.
    /// Use this for a clean 'escape' that triggers nothing.
    /// </summary>
    public void QuietRestartAllNoEvents(bool clearWrongFlags = true)
    {
        // Clear wrong flags
        if (clearWrongFlags)
        {
            if (_wrong == null || _wrong.Length != riddles.Count) _wrong = new bool[riddles.Count];
            else Array.Clear(_wrong, 0, _wrong.Length);
        }

        _finalized = false; // re-arm submissions

        // Reset index and state
        _currentIndex = 0;
        _inputs.Clear();
        _uniqueInputs.Clear();

        // Turn off ALL known indicators across all riddles
        SilentClearAllIndicators();

        // Ensure submit gate is OFF
        if (submitBoolObject != null && submitBoolObject.activeSelf)
            submitBoolObject.SetActive(false);

        // Intentionally NO EVENTS fired here.
        DebugState("QuietRestartAllNoEvents");
    }
}
