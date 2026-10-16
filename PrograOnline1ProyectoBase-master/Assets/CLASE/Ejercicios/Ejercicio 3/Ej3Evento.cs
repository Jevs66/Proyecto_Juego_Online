using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ej3Evento : MonoBehaviour
{
    public event Action OnEvento;

    void Update()
    {
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            OnEvento?.Invoke();
        }
    }
}
