using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingBar : MonoBehaviour {

    public RectTransform BarTransform;

    private float percent;

    // Update is called once per frame
    public void Update() {
        Vector2 max = BarTransform.anchorMax;
        max.x = percent;
        BarTransform.anchorMax = max;
    }

    public void SetPercent(float f) {
        percent = f;
    }
}
