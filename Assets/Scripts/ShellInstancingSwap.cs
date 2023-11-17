using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellInstancingSwap : MonoBehaviour
{
    static public bool InstancingEnabled;
    ShellInstancing _inst;
    Shell _shell;

    void Start()
    {
        _shell = GetComponent<Shell>();
        _inst = GetComponent<ShellInstancing>();
    }

    private void Update()
    {
        if(InstancingEnabled != _inst.enabled)
            _inst.enabled = InstancingEnabled;
        if(InstancingEnabled != !_shell.enabled)
            _shell.enabled = !InstancingEnabled;
    }

}
