using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ShellInstancing : ShellBase 
{
    public struct InstanceData
    {
        public float4x4 mat;
        public float3 dir;
    }
    public Vector3 UpdateDirection { set => _job.data.dir = value; }
    WriterJob _job;

    [SerializeField] RenderData _renderData;
    int ShaderValue_InstanceProp = Shader.PropertyToID("_PerInstanceData");
    ComputeBuffer _argsBuffer;
    MaterialPropertyBlock _instanceBlock;
    ComputeBuffer _posBuffer;
    bool _isSetup;

    public override IEnumerable<Material> materials
    { get { yield return _renderData.Material; } } 

    private void OnEnable()
    {
        OnDisable();  
        _argsBuffer = _renderData.GetArgsBuffer(SHELL_COUNT, _argsBuffer); 
        _renderData.LayerIndex = 6;
        _instanceBlock ??= new MaterialPropertyBlock();
        _posBuffer = new ComputeBuffer(SHELL_COUNT, sizeof(float) * 4 * 4 + sizeof(float) * 3,
                            ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        UpdateInstanceBufferJob();
        _isSetup = true;

    }

    void UpdateInstanceBufferJob()
    {
        _job.writer = _posBuffer.BeginWrite<InstanceData>(0, SHELL_COUNT);
        _job.data.mat = transform.localToWorldMatrix;
        _job.Schedule(SHELL_COUNT, 32).Complete();
        _posBuffer.EndWrite<InstanceData>(SHELL_COUNT);
        _instanceBlock.SetBuffer(ShaderValue_InstanceProp, _posBuffer);
        _lastInstanceUpdate = _job.data;
    }

    InstanceData _lastInstanceUpdate;

    bool NeedUpdate()
    {
        return !_job.data.mat.Equals(transform.localToWorldMatrix) ||
               !_job.data.dir.Equals(_lastInstanceUpdate.dir);
    }
    private void LateUpdate()
    { 
        if (!_isSetup)
            return;
         
        if (NeedUpdate())
            UpdateInstanceBufferJob();

        var bounds = new Bounds() { size = transform.lossyScale, center = Vector3.zero };
        _renderData.RenderIndirect(_argsBuffer, bounds, _instanceBlock);
    } 

    private void OnDisable()
    {
        _argsBuffer?.Release();
        _posBuffer?.Release();
        _argsBuffer = null;
        _isSetup = false;
    }

    [BurstCompile]
    public struct WriterJob : IJobParallelFor
    {
        public NativeArray<InstanceData> writer;
        public InstanceData data;
        public void Execute(int index)
        {
            writer[index] = data;
        }
    }

}
