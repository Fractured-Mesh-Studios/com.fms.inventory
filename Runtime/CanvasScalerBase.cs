using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerBase : CanvasScaler {

    public bool Resolution;
    public float MinResolution = 640.0f;
    public float MaxResolution = 4096.0f;

    protected override void Start()
    {
        base.Start();
        UpdateResolution();
            
	}

    public void UpdateResolution()
    {
        if (Resolution)
        {
            Vector2 Clamp = Vector2.zero;
            Clamp.x = Mathf.Clamp(Screen.width, MinResolution, MaxResolution);
            Clamp.y = Mathf.Clamp(Screen.height, MinResolution, MaxResolution);

            referenceResolution = Clamp;
        }
    }

}
