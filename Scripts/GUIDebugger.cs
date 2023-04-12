using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIDebugger : MonoBehaviour
{
    public static GUIDebugger Instance { get; private set; }

    private float fps;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Update()
    {
        fps = 1f / Time.deltaTime;
    }
    
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 120, 100), $"{fps} FPS\n" +
                                              $"{Time.deltaTime} Frame\n" +
                                              $"{Application.targetFrameRate}\n" +
                                              $"{QualitySettings.vSyncCount}");
    }
}
