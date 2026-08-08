using UnityEngine;

public class PlayerBombHolder : MonoBehaviour
{
    [SerializeField] private Transform bombHolder;
    public Transform BombHolder => bombHolder;
}
