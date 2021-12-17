using System.Diagnostics;
using UnityEngine;

public class FrameTimer : MonoBehaviour {
    private Stopwatch stopwatch;

    public int FPSTarget = 60;

    public long FrameDuration {
        get {
            if (stopwatch == null) {
                return 0;
            } else {
                return stopwatch.ElapsedMilliseconds;
            }
        }
    }

    public bool FrameHasTime {
        get {
            return FrameDuration < 1000 / FPSTarget;
        }
    }

    public void Awake() {
        stopwatch = new Stopwatch();
    }

    public void Update() {
        if (stopwatch == null) {
            stopwatch = new Stopwatch();
        }
        stopwatch.Reset();
        stopwatch.Start();
    }
}