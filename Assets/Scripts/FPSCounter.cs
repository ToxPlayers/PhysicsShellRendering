using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class FPSCounter : MonoBehaviour
{
    static string _fpsStr;
    static public string FPSStr => _fpsStr;
    float[] _deltaBuffer = new float[50];
    int _frameIndex;
    GUIStyle _style;
    private void Start()
    { 
        StartCoroutine(FPSCount());
    } 

    private void Update()
    {
        _deltaBuffer[_frameIndex] = Time.deltaTime;
        _frameIndex = (_frameIndex + 1) % _deltaBuffer.Length;
    }

    float AverageFPS()
    {
        var totalDelta = 0f;
        foreach(var delta in _deltaBuffer)
            totalDelta += delta;
        return _deltaBuffer.Length / totalDelta;
    }

    IEnumerator FPSCount()
    {
        while (Application.isPlaying)
        { 
            yield return new WaitForSeconds(0.1f);
            _fpsStr = "FPS: " + AverageFPS().ToString("0.00");
        }
    }
}
