using UnityEngine;

public class GenericFunctions : MonoBehaviour
{
   public string RandomName(int nameLenght)
    {
        string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZÑ0123456789";
        string nombre = "";

        for (int i = 0; i < nameLenght; i++) 
        { 
            int index = Random.Range(0, caracteres.Length);
            nombre += caracteres[index];
        }

        return nombre;
    }
}
