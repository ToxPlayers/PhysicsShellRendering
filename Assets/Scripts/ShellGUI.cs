using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellGUI : MonoBehaviour
{
    [SerializeField] SimpleShell _shell;
    [SerializeField] ShellPhysics _physics;
    [SerializeField] SpringJoint _spring;
    Rigidbody _springRb;
    GUIStyle _thumbStyle, _sliderStyle, _textStyle;

    private void Awake()
    {
        _springRb = _spring.GetComponent<Rigidbody>();
    }

    private void OnGUI()
    {
        _sliderStyle = GUI.skin.horizontalSlider;
        _thumbStyle = GUI.skin.horizontalSliderThumb;
        _textStyle = GUI.skin.textArea;
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical("Shell", "window", GUILayout.Width(Screen.width / 3)); 
        GUI.changed = false;
        _shell.shellLength = DrawSlider("Length", _shell.shellLength, 0.1f, 1f);
        _shell.density = DrawSlider("Density", _shell.density, 10f, 800f);
        _shell.thickness = DrawSlider("Thickness", _shell.thickness, 0.5f, 10f); 
        if(GUI.changed)
            _shell.UpdateMaterials();
        GUILayout.EndVertical();
        GUILayout.Label(FPSCounter.FPSStr);
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical("Physics", "window", GUILayout.Width(Screen.width /3));
        _physics.GravityStrength = DrawSlider("Gravity Strength", _physics.GravityStrength, 0, 0.15f);
        _physics.VelocityMultiplier = DrawSlider("Velocity Strength", _physics.VelocityMultiplier, 0, 5f);
        _physics.ClampedMagnitude = DrawSlider("Max Velocity", _physics.ClampedMagnitude, 0, 5f);

        GUILayout.BeginVertical("Spring", "window");
        _spring.spring = DrawSlider("Spring Strength", _spring.spring, 18, 200f);
        _spring.damper = DrawSlider("Spring Damper", _spring.damper, 0, 200f);
        _springRb.drag = DrawSlider("Spring Drag", _springRb.drag, 0, 5f);
        GUILayout.EndVertical();

        GUILayout.EndVertical(); 

    }


    float DrawSlider(string name, float value, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, _textStyle, GUILayout.Width(140)); 
        var res = GUILayout.HorizontalSlider(value, min, max,_sliderStyle, _thumbStyle,GUILayout.ExpandWidth(true));
        GUILayout.Label(res.ToString("0.00"), _textStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        return res;
    }
}
