using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShellBase : MonoBehaviour
{
    // These variables and what they do are explained on the shader code side of things
    // You can see below (line 70) which shader uniforms match up with these variables
    [Range(1, 256)]
    public int shellCount = 16;

    [Range(0.0f, 1.0f)]
    public float shellLength = 0.15f;

    [Range(0.01f, 3.0f)]
    public float distanceAttenuation = 1.0f;

    [Range(1.0f, 1000.0f)]
    public float density = 100.0f;

    [Range(0.0f, 1.0f)]
    public float noiseMin = 0.0f;

    [Range(0.0f, 1.0f)]
    public float noiseMax = 1.0f;

    [Range(0.0f, 10.0f)]
    public float thickness = 1.0f;

    [Range(0.0f, 10.0f)]
    public float curvature = 1.0f;

    [Range(0.0f, 1.0f)]
    public float displacementStrength = 0.1f;

    public Color shellColor;

    [Range(0.0f, 5.0f)]
    public float occlusionAttenuation = 1.0f;

    [Range(0.0f, 1.0f)]
    public float occlusionBias = 0.0f;
    public abstract IEnumerable<Material> materials { get; }

    protected virtual void OnValidate() => UpdateMaterials();
    public virtual void UpdateMaterials()
    {
        Shader.SetGlobalInteger("_ShellCount", shellCount);
        Shader.SetGlobalFloat("_ShellLength", shellLength);
        Shader.SetGlobalFloat("_Density", density);
        Shader.SetGlobalFloat("_Thickness", thickness);
        Shader.SetGlobalFloat("_Attenuation", occlusionAttenuation);
        Shader.SetGlobalFloat("_ShellDistanceAttenuation", distanceAttenuation);
        Shader.SetGlobalFloat("_Curvature", curvature);
        Shader.SetGlobalFloat("_DisplacementStrength", displacementStrength);
        Shader.SetGlobalFloat("_OcclusionBias", occlusionBias);
        Shader.SetGlobalFloat("_NoiseMin", noiseMin);
        Shader.SetGlobalFloat("_NoiseMax", noiseMax); 
        Shader.SetGlobalColor("_ShellColor", shellColor);

        Debug.Log( Shader.GetGlobalColor("_ShellColor"));
    }
}
