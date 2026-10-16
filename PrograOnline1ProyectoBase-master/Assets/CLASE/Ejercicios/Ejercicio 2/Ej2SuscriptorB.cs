using UnityEngine;
using UnityEngine.InputSystem;

public class Ej2SuscriptorB : MonoBehaviour
{
    [SerializeField] private Ej2EventoDinamico eventoDinamico;

    private bool suscrito;

    void OnEnable()
    {
        eventoDinamico.OnEvento += Reaccionar;
        suscrito = true;
    }

    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame && suscrito)
        {
            eventoDinamico.OnEvento -= Reaccionar;
            suscrito = false;

            Debug.Log("Suscriptor B se desuscribió");
        }
    }

    void OnDisable()
    {
        if (suscrito)
        {
            eventoDinamico.OnEvento -= Reaccionar;
            suscrito = false;
        }
    }

    void Reaccionar()
    {
        Debug.Log("Suscriptor B reaccionó");
    }
}
