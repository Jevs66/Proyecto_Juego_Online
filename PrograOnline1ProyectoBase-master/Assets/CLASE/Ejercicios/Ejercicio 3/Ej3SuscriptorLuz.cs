using UnityEngine;

public class Ej3SuscriptorLuz : MonoBehaviour
{
    [SerializeField] private Ej3Evento eventoPrincipal;

    void OnEnable()
    {
        eventoPrincipal.OnEvento += Reaccionar;
    }

    void OnDisable()
    {
        eventoPrincipal.OnEvento -= Reaccionar;
    }

    void Reaccionar()
    {
        Debug.Log("Luz reaccionó al evento");
    }
}
