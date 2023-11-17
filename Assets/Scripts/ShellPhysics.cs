using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ShellPhysics : MonoBehaviour
{
    readonly static int ShaderValue_Direction = Shader.PropertyToID("_ShellDirection");
    [SerializeField] Shell _shell;
    [SerializeField] ShellInstancing _shellInst;
    [SerializeField] Transform _jointDummy;
    [SerializeField] Vector3 _direction;
    public float ClampedMagnitude;
    public float VelocityMultiplier;
    public float GravityStrength;
    Transform _camTf;
    private void Awake()
    {
        _shellInst = GetComponent<ShellInstancing>();
        _shell = GetComponent<Shell>();
        _camTf = Camera.main.transform;
    }
    public IEnumerable<Material> d()
    {
        yield return null;
    }

    private void LateUpdate()
    {
        _direction = transform.position - _jointDummy.position;
        _direction *= VelocityMultiplier;
        _direction += Physics.gravity * GravityStrength;
        _direction = Vector3.ClampMagnitude(_direction, ClampedMagnitude);

        if (ShellInstancingSwap.InstancingEnabled)
            _shellInst.UpdateDirection = _direction;
        else
            foreach (var mat in _shell.materials)
                mat.SetVector(ShaderValue_Direction, _direction); 
    }
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return; 
        var dir = _camTf.TransformDirection(_direction);
        var rot = dir == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(dir);
        Handles.color = Color.green;
        var world = Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0.95f, 20f));
        Handles.ArrowHandleCap(0, world, rot, 1f, EventType.Repaint);
    }
#endif
}
