using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class Shell : ShellBase 
{ 
    public Mesh shellMesh;
    public Shader shellShader;
    public bool updateStatics = true; 
    private Material shellMaterial;
    private GameObject[] shells;

    public Material[] shellMaterials;
    public override IEnumerable<Material> materials => shellMaterials;

    void OnEnable() 
    {
        OnDisable();
        shellMaterial = new Material(shellShader);
        shellMaterials = new Material[SHELL_COUNT];
        shells = new GameObject[SHELL_COUNT];
        for (int i = 0; i < SHELL_COUNT; ++i) 
        {
            shells[i] = new GameObject($"Shell[{i}]");
            shells[i].transform.SetParent(transform);
            shells[i].transform.localPosition = new Vector3();

            var mf = shells[i].AddComponent<MeshFilter>();
            var mr = shells[i].AddComponent<MeshRenderer>();

            mf.mesh = shellMesh;
            if(mr.material.shader != shellMaterial)
                mr.material = new Material(shellMaterial);

            var mat = mr.material;
            mat.SetInteger("_ShellIndex", i);
            shellMaterials[i] = mat;
        }
        UpdateMaterials();
    }

    GameObject[] _tempArr;
    private void OnDisable()
    {
        if (_tempArr == null || _tempArr.Length < transform.childCount)
            _tempArr = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            _tempArr[i] = transform.GetChild(i).gameObject;
        for (int i = 0; i < _tempArr.Length; i++)
        {
            if (Application.isPlaying)
                Destroy(_tempArr[i]);
            else DestroyImmediate(_tempArr[i]);
        }
        shells = null; 
    } 
}
