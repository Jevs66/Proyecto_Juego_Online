using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.Addons.SimpleKCC;

[RequireComponent(typeof(Rigidbody), typeof(GroundCheck))]
public class MovementController : NetworkBehaviour
{
    private InputManager inputManager;
    private Rigidbody rbPlayer;

    [SerializeField] private SimpleKCC kcc;

    [SerializeField] private Animator _animator;

    public override void Spawned()
    {
        rbPlayer = GetComponent<Rigidbody>();
        kcc = GetComponent<SimpleKCC>();
    }
   
    
    private void Start()
    {
        inputManager = InputManager.Instance;        
    }
    
    public override void FixedUpdateNetwork()
    {
        //if (!HasInputAuthority && !HasStateAuthority)
        //    return;

        if(GetInput(out NetworkInfoData input))
        {
            kcc.SetLookRotation(0f, input.lookRotationY);

            Movement(input);

            _animator.SetBool("IsWalking", input.moveInputPressed);
            _animator.SetBool("IsRunning", input.runInputPressed);
            _animator.SetFloat("WalkingZ", input.movementAxis.y);
            _animator.SetFloat("WalkingX", input.movementAxis.x);
        }

       
    }


    #region Movimiento

    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;
    [SerializeField] private float crouchSpeed = 3.9f;

    private void Movement(NetworkInfoData input)
    {
        Quaternion lookRotation = Quaternion.Euler(0f, input.lookRotationY, 0f);

        Vector3 direction = lookRotation * new Vector3(input.movementAxis.x, 0f, input.movementAxis.y);

        kcc.Move(direction * Speed(input));
    }

    private float Speed(NetworkInfoData input)
    {
        return input.movingBackwards || input.movingOnXAxis ? walkSpeed :
            input.runInputPressed ? runSpeed : walkSpeed;
    }


    #endregion

  
}