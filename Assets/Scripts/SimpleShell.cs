using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimpleShell : ShellBase {
    public Mesh shellMesh;
    public Shader shellShader;
    public bool updateStatics = true; 
    private Material shellMaterial;
    private GameObject[] shells;

    public Material[] shellMaterials;
    public override IEnumerable<Material> materials => shellMaterials;

    void OnEnable() {

        shellMaterial = new Material(shellShader);
        shellMaterials = new Material[shellCount];
        shells = new GameObject[shellCount];
        for (int i = 0; i < shellCount; ++i) {
            shells[i] = new GameObject("Shell " + i.ToString());
            var mf = shells[i].AddComponent<MeshFilter>();
            var mr = shells[i].AddComponent<MeshRenderer>();

            mf.mesh = shellMesh;
            mr.material = shellMaterial;
            shells[i].transform.SetParent(transform, false);

            // In order to tell the GPU what its uniform variable values should be, we use these "Set" functions which will set the
            // values over on the GPU.
            var mat = mr.material;
            mat.enableInstancing = false;
            mat.SetInteger("_ShellIndex", i);
            shellMaterials[i] = mat;
        }
        UpdateMaterials();
    }

    public override void UpdateMaterials()
    {
        base.UpdateMaterials();
        for (int i = 0; i < shellMaterials.Length; i++)
        { 
            var mat = shellMaterials[i];
            mat.SetInteger("_ShellIndex", i);
            mat.SetInteger("_ShellCount", shellCount);
            mat.SetFloat("_ShellLength", shellLength);
            mat.SetFloat("_Density", density);
            mat.SetFloat("_Thickness", thickness);
            mat.SetFloat("_Attenuation", occlusionAttenuation);
            mat.SetFloat("_ShellDistanceAttenuation", distanceAttenuation);
            mat.SetFloat("_Curvature", curvature);
            mat.SetFloat("_DisplacementStrength", displacementStrength);
            mat.SetFloat("_OcclusionBias", occlusionBias);
            mat.SetFloat("_NoiseMin", noiseMin);
            mat.SetFloat("_NoiseMax", noiseMax);
            mat.SetColor("_ShellColor", shellColor);
        }
    }

    void OnDisable() {
        for (int i = 0; i < shells.Length; ++i)
            Destroy(shells[i]); 
        shells = null;
    }
}
