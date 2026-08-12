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

    private Health health;

    public override void Spawned()
    {
        rbPlayer = GetComponent<Rigidbody>();
        kcc = GetComponent<SimpleKCC>();
        health = GetComponent<Health>();
    }
   
    
    private void Start()
    {
        inputManager = InputManager.Instance;        
    }
    
    public override void FixedUpdateNetwork()
    {
        if (health != null && health.IsDead)
        {
            StopMovement();
            return;
        }

        if (MatchManager.Instance != null &&
            MatchManager.Instance.MatchFinished)
        {
            StopMovement();
            return;
        }

        if (GetInput(out NetworkInfoData input))
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

    private void StopMovement()
    {
        kcc.Move(Vector3.zero);

        if (_animator == null)
        {
            return;
        }

        _animator.SetBool("IsWalking", false);
        _animator.SetBool("IsRunning", false);
        _animator.SetFloat("WalkingZ", 0f);
        _animator.SetFloat("WalkingX", 0f);
    }

    public void ResetPlayerPosition(Vector3 position, Quaternion rotation)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        kcc.SetPosition(position);
        kcc.SetLookRotation(0f, rotation.eulerAngles.y);      

        StopMovement();
    }


    #endregion


}