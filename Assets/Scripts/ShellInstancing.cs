using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellInstancing : ShellBase 
{ 
    ComputeBuffer _argsBuffer;
    [SerializeField] RenderData _renderData;
    public override IEnumerable<Material> materials
    { get { yield return _renderData.Material; } }    

    private void Awake()
    { 
        _argsBuffer = _renderData.GetArgsBuffer(shellCount);  
    }

    private void Update()
    {
        var bounds = new Bounds( ) { size = transform.lossyScale, center = transform.position };
        _renderData.RenderIndirect(_argsBuffer, bounds);
    } 
}
