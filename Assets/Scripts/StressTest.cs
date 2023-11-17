using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressTest : MonoBehaviour
{ 
    [SerializeField] GameObject _toStress; 
    public void Add()
    {
        var go = Instantiate(_toStress, Vector3.zero, Quaternion.identity, transform);
        go.SetActive(true);
    }
    public void Remove()
    {
        if(transform.childCount > 0)
        Destroy(transform.GetChild(0).gameObject);
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
    }

    void Update()
    { 
        transform.position += new Vector3(Mathf.Sin(Time.time) / 120f , 0, 0);
    }
}
