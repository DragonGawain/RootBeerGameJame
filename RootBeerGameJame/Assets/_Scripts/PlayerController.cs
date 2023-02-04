//using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControllerInput _input;
    private PlayerKinematicMotor _motor;
    
    public GameObject CameraTarget;
    private GameObject _mainCamera;
    private GameObject _mainCameraHolder;
    private Transform _cameraFoward;

    private Transform _geometry;

    public float TopClamp = 80.0f;
    public float BottomClamp = 0.0f;

    private float _yaw;
    private float _pitch;

    private const float _threshold = 0.01f;

    private Vector3 _direction;

    private bool _roll;

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            _mainCameraHolder = GameObject.FindGameObjectWithTag("MainCameraHolder");
            _cameraFoward = transform.Find("CameraForward");
        }
    }

    private void Start()
    {
        _input = transform.GetComponent<PlayerControllerInput>();
        _motor = transform.GetComponent<PlayerKinematicMotor>();

        _geometry = transform.Find("Geometry");
    }

    private void FixedUpdate()
    {
        Jump();
        Move();
        Roll();
    } 

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold)
        {
            float deltaTimeMultiplier = Time.deltaTime;
            _yaw += _input.look.x * deltaTimeMultiplier;
            _pitch += _input.look.y * deltaTimeMultiplier;

            _pitch = ClampAngle(_pitch, BottomClamp, TopClamp);

            _mainCameraHolder.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
        }

        float cameraDistance = -15.0f;
        //float cameraDistance = -6.0f;
        float cameraDelay = 0.035f;

        _mainCameraHolder.transform.position = CameraTarget.transform.position;
        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, _mainCameraHolder.transform.position + (_mainCameraHolder.transform.forward * cameraDistance), cameraDelay);
        _mainCamera.transform.rotation = Quaternion.Lerp(_mainCamera.transform.rotation, _mainCameraHolder.transform.rotation, cameraDelay);

        if (_cameraFoward)
            _cameraFoward.rotation = Quaternion.Euler(0.0f, _yaw, 0.0f);
    }

    private void Jump()
    {
        if (_input.jump)
        {
            _motor.OnJumpInput();
        }
    }

    private void Move()
    {
        _direction = (Quaternion.Euler(0.0f, 0.0f, -_yaw) * _input.move);

        _motor.OnMoveInput(_direction);
    }

    private void Roll()
    {
        _roll = _input.roll;

        _motor.OnRollInput(_roll);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
