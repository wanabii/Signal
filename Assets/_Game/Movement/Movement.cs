using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class Movement : MonoBehaviour
{
    
    [SerializeField] private float speed;
    [SerializeField] private InputActionReference _actionReference;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _gravity;
    
    private Transform _cameraTransform;
    private InputAction _action;
    private Vector2 _keyPerformed;
    private float _verticalVelocity;
    
    
    private void OnEnable()
    {
        _action = _actionReference.action;
        _action.performed += OnMovePerformed;
        _action.canceled += OnMoveCanceled;
    }

    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        _keyPerformed = Vector2.zero;
    }

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
    }
    
    private void OnMovePerformed(InputAction.CallbackContext obj)
    {
        _keyPerformed = obj.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * _keyPerformed.y + right * _keyPerformed.x;
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        Vector3 motion = move * speed;
        motion.y = _verticalVelocity;

        _characterController.Move(motion * Time.deltaTime);
    }
}
