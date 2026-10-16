using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ej1Saludo : MonoBehaviour
{
    public event Action OnSaludo;

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Saludar();
        }
    }

    void Saludar()
    {
        OnSaludo?.Invoke();
    }
}
