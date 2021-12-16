using System.Diagnostics;
using UnityEngine;

public class FrameTimer : MonoBehaviour {
    private Stopwatch stopwatch;

    public int FPSTarget = 60;

    public long FrameDuration {
        get {
            if (this.stopwatch == null) {
                return 0;
            } else {
                return this.stopwatch.ElapsedMilliseconds;
            }
        }
    }

    public bool FrameHasTime {
        get {
            return FrameDuration < 1000 / FPSTarget;
        }
    }

    void Awake() {
        this.stopwatch = new Stopwatch();
    }

    void Update() {
        // For whatever reason, .Restart() wasn't recognized.
        this.stopwatch.Reset();
        this.stopwatch.Start();
    }
}