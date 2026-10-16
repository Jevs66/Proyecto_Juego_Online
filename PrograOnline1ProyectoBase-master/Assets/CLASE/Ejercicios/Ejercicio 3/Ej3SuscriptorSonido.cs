using UnityEngine;

public class Ej3SuscriptorSonido : MonoBehaviour
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
        Debug.Log("Sonido reaccionó al evento");
    }
}
