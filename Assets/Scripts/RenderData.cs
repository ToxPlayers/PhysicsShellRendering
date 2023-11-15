using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Rendering;
using System;
using Unity.Collections;

[CreateAssetMenu(fileName = "NewRenderData", menuName = "RenderData")]
public class RenderData : ScriptableObject
{
    NativeArray<uint> TempArgsArr;
    static public int ShaderValue_InstanceData = Shader.PropertyToID("_PerInstanceData");
    public const int ArgsBufferLength = 5;
    public const int ArgsInstanceCountIndex = 1;

    public Material Material;
    public Mesh Mesh;
    public int LayerIndex;
    public bool RecieveShadows = true;
    public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.On;

    static public MaterialPropertyBlock GetInstanceDataProps(ComputeBuffer buffer)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        SetInstanceData(block, buffer);
        return block;
    }

    static public void SetInstanceData(MaterialPropertyBlock block, ComputeBuffer buffer)
    {
        block.SetBuffer(ShaderValue_InstanceData, buffer);
    }
     
    public ComputeBuffer GetArgsBuffer(int instanceCount, ComputeBuffer args = null)
    {
        if (instanceCount == 0)
            throw new ArgumentException(nameof(instanceCount) + " cant be null");

        args ??= new ComputeBuffer
            (1, ArgsBufferLength * sizeof(uint), ComputeBufferType.IndirectArguments);
        SetTempArgsBuffer((uint)instanceCount);
        args.SetData(TempArgsArr);
        return args;
    } 

    void SetTempArgsBuffer(uint instanceCount)
    {
        if (!TempArgsArr.IsCreated)
        {
            TempArgsArr = new NativeArray<uint>(ArgsBufferLength, Allocator.Persistent);
            TempArgsArr[0] = Mesh.GetIndexCount(0);
            TempArgsArr[2] = Mesh.GetIndexStart(0);
            TempArgsArr[3] = Mesh.GetBaseVertex(0);
            TempArgsArr[4] = 0;
        }
        TempArgsArr[1] = instanceCount;
    }

    public void RenderIndirect(ComputeBuffer argsBuffer, Bounds bounds, 
                                MaterialPropertyBlock properties = null, Camera camera = null)
    {
        if (!camera)
            camera = Camera.main;

#if UNITY_EDITOR 
           RenderIndirectEditorCam(argsBuffer, bounds, properties);
#endif

        Graphics.DrawMeshInstancedIndirect
           (
               Mesh,
               0,
               Material,
               bounds,
               argsBuffer,
               0,
               properties,
               ShadowCastingMode,
               RecieveShadows,
               LayerIndex,
               camera
           );
    }

#if UNITY_EDITOR
    void RenderIndirectEditorCam(ComputeBuffer argsBuffer, Bounds bounds, 
        MaterialPropertyBlock properties = null)
    {
        var cams = UnityEditor.SceneView.GetAllSceneCameras();
        foreach (var cam in cams)
        {
            Graphics.DrawMeshInstancedIndirect
           (
               Mesh,
               0,
               Material,
               bounds,
               argsBuffer,
               0,
               properties,
               ShadowCastingMode,
               RecieveShadows,
               LayerIndex,
               cam
           );
        }
    }
#endif

    private void OnDestroy()
    {
        if(TempArgsArr.IsCreated)
            TempArgsArr.Dispose();
    }
}
