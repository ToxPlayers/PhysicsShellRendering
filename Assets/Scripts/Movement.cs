using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    bool _isDragging;
    Vector3 _dragOffset;
    Vector3 _lastDragPos;
    Vector3 _velocity;
    Rigidbody _rb;
    Camera _cam;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isDragging)
            transform.position = _lastDragPos;
    }


    Vector3 ScreenPos(Vector3 world) => _cam.WorldToScreenPoint(world);
    Vector3 WorldPos(Vector3 screen) => _cam.ScreenToWorldPoint(screen);
    private void OnMouseDown()
    { 
        _dragOffset =  Input.mousePosition - ScreenPos(transform.position);
        _isDragging = true;
    }
    private void OnMouseUp()
    {
        _isDragging = false;
    }
    private void OnMouseDrag()
    {
        transform.position = WorldPos(Input.mousePosition - _dragOffset);
        _lastDragPos = transform.position;
    }
}
