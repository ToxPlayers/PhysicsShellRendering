using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ShellPhysics : MonoBehaviour
{
    readonly static int ShaderValue_Direction = Shader.PropertyToID("_ShellDirection");
    ShellBase _shell;
    [SerializeField] Transform _jointDummy;
    [SerializeField] Vector3 _direction;
    public float ClampedMagnitude;
    public float VelocityMultiplier;
    public float GravityStrength; 
    private void Start()
    {
        _shell = GetComponent<ShellBase>(); 
    }
     
    private void LateUpdate()
    { 
        _direction = _jointDummy.position - transform.position;
        _direction *= VelocityMultiplier;
        _direction += Physics.gravity * GravityStrength;
        _direction = Vector3.ClampMagnitude(_direction, ClampedMagnitude);
        foreach (var mat in _shell.materials)
            mat.SetVector(ShaderValue_Direction, _direction);
    }

#if UNITY_EDITOR 
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        var rot = _direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(_direction);
        Handles.color = Color.green;
        var world = Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0.95f, 20f));
        Handles.ArrowHandleCap(0, world, rot, 1f, EventType.Repaint);
    }
#endif
}
