using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Ej2EventoDinamico : MonoBehaviour
{
    public event Action OnEvento;

    void Update()
    {
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            OnEvento?.Invoke();
        }
    }
}
