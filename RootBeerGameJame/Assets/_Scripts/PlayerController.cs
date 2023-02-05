using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerControllerInput _input;
    private PlayerKinematicMotor _motor;

    public GameManager gm;
    
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
    private bool _jump;
    private bool _die;

    public float cameraDistance = -10.0f;

    public LayerMask layerMask;

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
        Die();
    } 

    private void LateUpdate()
    {
        CameraRotation(0.035f);
    }

    private void CameraRotation(float cameraDelay, bool force = false)
    {
        if (_input.look.sqrMagnitude >= _threshold || force)
        {
            if (_motor._state != PlayerKinematicMotor.PlayerState.SPAWNING)
            {
                float deltaTimeMultiplier = Time.deltaTime;
                _yaw += _input.look.x * deltaTimeMultiplier;
                _pitch += _input.look.y * deltaTimeMultiplier;

                _pitch = ClampAngle(_pitch, BottomClamp, TopClamp);
            }

            _mainCameraHolder.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
        }

        var distance = cameraDistance;

        if (Physics.Raycast(CameraTarget.transform.position, -_mainCamera.transform.forward, out RaycastHit _hit, -cameraDistance, layerMask))
        {
            distance = -(_hit.distance - 0.1f);
        }

        _mainCameraHolder.transform.position = CameraTarget.transform.position;
        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, _mainCameraHolder.transform.position + (_mainCameraHolder.transform.forward * distance), cameraDelay);
        _mainCamera.transform.rotation = Quaternion.Lerp(_mainCamera.transform.rotation, _mainCameraHolder.transform.rotation, cameraDelay);

        if (_cameraFoward)
            _cameraFoward.rotation = Quaternion.Euler(0.0f, _yaw, 0.0f);
    }

    public void Reset()
    {
        _yaw = 0;
        _pitch = 0;

        CameraRotation(1, true);
    }

    private void Jump()
    {
        _jump = _input.jump;

        _motor.OnJumpInput(_jump);

        gm.TryStart(_jump);
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

    private void Die()
    {
        _die = _input.die;

        _motor.OnDieInput(_die);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
