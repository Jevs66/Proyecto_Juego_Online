using UnityEngine;

public class Ej3SuscriptorUI : MonoBehaviour
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
        Debug.Log("UI reaccionó al evento");
    }
}
