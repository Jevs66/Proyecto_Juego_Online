using UnityEngine;

public class Ej1Suscriptor : MonoBehaviour
{
    [SerializeField] private Ej1Saludo saludoEvento;

    void OnEnable()
    {
        saludoEvento.OnSaludo += MostrarSaludo;
    }

    void OnDisable()
    {
        saludoEvento.OnSaludo -= MostrarSaludo;
    }

    void MostrarSaludo()
    {
        Debug.Log("Hola, el evento fue activado");
    }
}
