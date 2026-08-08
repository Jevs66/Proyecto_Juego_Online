using Fusion;
using UnityEngine;

public struct NetworkInfoData : INetworkInput
{
    public Vector2 movementAxis;
    public Vector2 mouseDelta;

    public float lookRotationY;

    public bool runInputPressed;
    public bool moveInputPressed;
    public bool movingBackwards;
    public bool movingOnXAxis;
    public bool isShooting;

    public bool interactPressed;
}
