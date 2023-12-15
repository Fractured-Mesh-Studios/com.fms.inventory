using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Rotation : MonoBehaviour
{
    public enum UI_ERotation { None, Left, Right }

    [Header("Rotation")]
    public float Speed = 20.0f;
    public float Smooth = 1.0f;
    public UI_ERotation Mode = UI_ERotation.None;

    [HideInInspector] public RectTransform RectTransform;

    void Awake()
    {
        RectTransform = transform as RectTransform;
    }

    void Update()
    {
        if(Mode != UI_ERotation.None)
        {
            float Velocity;
            switch (Mode)
            {
                case UI_ERotation.Left: Velocity = Speed * 1; break;
                case UI_ERotation.Right: Velocity = Speed * -1; break;
                default: break;
            }
            Quaternion NewRotation = transform.rotation * Quaternion.Euler(new Vector3(0, 0, Speed));
            float DeltaTime = Time.deltaTime * Smooth;
            transform.rotation = Quaternion.Lerp(transform.rotation, NewRotation, DeltaTime);
            
        }
    }
}
