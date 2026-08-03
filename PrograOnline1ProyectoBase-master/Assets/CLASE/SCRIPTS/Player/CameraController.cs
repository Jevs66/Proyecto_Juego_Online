using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Fusion;
using Fusion.Addons.SimpleKCC;


public class CameraController : NetworkBehaviour
{
    [Header("Camera Settings")] [SerializeField]
    private Transform player;

    [SerializeField] private float mouseSensitivity = 1;

    [FormerlySerializedAs("smooth")] [SerializeField]
    private float smoothnes;

    [SerializeField] private float maxAngleY = 80;
    [SerializeField] private float minAngleY = -80;

    private Vector2 camVelociy;
    private Vector2 smoothVelocity;

    [Header("Blob Movement")] 
    [SerializeField] private float walkingSpeed = 1f;
    
    [SerializeField, Range(0,0.1f)] private float walkingAmplitude = 0.015f; // Que tanto se mueve hacia los lados al caminar
    [SerializeField, Range(0,0.1f)] private float runningAmplitude = 0.015f; // Que tanto se mueve hacia los lados al correr
    [SerializeField, Range(0,15)] private float walkingFrequency = 10.0f; // La frecuencia con la que se mueve al caminar
    [SerializeField, Range(10,20)] private float runningFrequency = 18f; // La frecuencia con la que se mueve al correr
    [SerializeField] private float resetPosSpeed = 3.0f; // Cuando dejas de moverte que regrese al centro
    [SerializeField] private float toggleSpeed = 3.0f; // 
    
    private Vector3 startPos; // Posicion inicial de la cabeza , el centro

    [SerializeField] private bool moveHead;
    
    private Vector2 head;

    private SimpleKCC playerKcc;

    private InputManager inputManager;
    
    private void Awake()
    {
        startPos = transform.localPosition;
    }       

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
            GetComponent<CameraController>().enabled = false;
            return;
        }

        inputManager = InputManager.Instance;

        if (player == null)
        {
            MovementController movementController = GetComponentInParent<MovementController>();

            if (movementController != null)
            {
                player = movementController.transform;
            }
        }

        if (player != null)
        {
            playerKcc = player.GetComponent<SimpleKCC>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInfoData input))
        {
            RotateCamera(input);

            if (!moveHead) return;

            BlobMove(input);
            ResetPosition();
        }
    }

    private void RotateCamera(NetworkInfoData input)
    {
        Vector2 rawFrameVelocity = Vector2.Scale(input.mouseDelta, Vector2.one * mouseSensitivity);
        smoothVelocity = Vector2.Lerp(smoothVelocity, rawFrameVelocity, 1 / smoothnes); // Te mueve desde donde tengas el mouse, a la nueva posicion del mouse
        camVelociy += smoothVelocity;
        camVelociy.y = Mathf.Clamp(camVelociy.y, minAngleY, maxAngleY); // Limita la rotacion de la camara en Y. En base el movimiento del mouse.

        transform.localRotation = Quaternion.AngleAxis(-camVelociy.y, Vector3.right); // Rota la camara hacia arriba y abajo. La rotacion esta en X. 

        inputManager.SetLookRotationY(camVelociy.x);
    }
    
   private void BlobMove(NetworkInfoData input)
    {
        if (!input.moveInputPressed) // Si no presiono ningun input
        {
            return; // termina el metodo
        }
        
        if(input.moveInputPressed) // Pregunto si me estoy moviendo
        {
            if(input.movingBackwards || input.movingOnXAxis) // Me estoy moviendo hacia atras o hacia los lados?
            {
                transform.localPosition += FootStepMotion();
            }
            else //  Entonces me muevo hacia adelante
            {
                if(input.runInputPressed) // Estoy corriendo?
                {
                    transform.localPosition += RunningFootStepMotion();
                }
                else // Estoy caminando
                {
                    transform.localPosition += FootStepMotion();
                }
            }
        }

        //if(input.moveInputPressed)
        //{
        //    transform.localPosition += input.movingBackwards || input.movingOnXAxis ? FootStepMotion() : input.runInputPressed ? RunningFootStepMotion() : FootStepMotion();
        //}
        
        
        
    } 

    private void ResetPosition()
    {
        if(transform.localPosition == startPos) return; // Si la camara ya esta en la pos inicial, no hace nada
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Time.deltaTime);
}

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * walkingFrequency) * walkingAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * walkingFrequency / 2) * walkingAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
    
    private Vector3 RunningFootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * runningFrequency) * runningAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * runningFrequency / 2) * runningAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
}