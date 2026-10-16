using UnityEngine;

public class Ej2SuscriptorA : MonoBehaviour
{
    [SerializeField] private Ej2EventoDinamico eventoDinamico;

    void OnEnable()
    {
        eventoDinamico.OnEvento += Reaccionar;
    }

    void OnDisable()
    {
        eventoDinamico.OnEvento -= Reaccionar;
    }

    void Reaccionar()
    {
        Debug.Log("Suscriptor A reaccionó");
    }
}
