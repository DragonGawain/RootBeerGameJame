using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//https://www.youtube.com/watch?v=s-99Z_W8bcQ

public class RayCastParams
{
    public Vector3 bottom;
    public Vector3 top;
    public float radius;

    public RayCastParams(Transform t, CapsuleCollider c)
    {
        Vector3 center = t.rotation * c.center + t.position;
        radius = c.radius;
        bottom = center + t.rotation * Vector3.down * (c.height / 2 - radius);
        top = center + t.rotation * Vector3.up * (c.height / 2 - radius);
    }
}

public class PlayerKinematicMotor : MonoBehaviour
{
    public GameObject _dummy;
    private Rigidbody rb;
    private CapsuleCollider _capsuleCollider;
    public CapsuleCollider _defaultCollider;
    public CapsuleCollider _rollingCollider;
    private Animator _animator;
    private Transform _geometry;

    public GameObject _defaultCan;
    public GameObject _rollingCan;

    private Vector3 _futurePosition;
    public Vector3 _direction;
    public Vector3 _RollDirection;
    public bool _isGrounded;

    public Vector3 _groundNormal;
    private float _lastY;

    public float speed = 0.10f;
    //public int nBounces = 3;
    public float _gravity = 9.8f;

    public bool falling = false;
    public bool canJump = true;
    public Vector3 velocity;
    public float jumpForce;
    public float _maxSlope = 80;

    public float _jumpTimer = 0.0f;
    public float _jumpDelay = 0.1f;

    public float _movementLockedTimer = 0.0f;
    public float _movementLockedDelay = 0.1f;

    public float _movementBufferTimer = 0.0f;
    public float _movementBufferDelay = 0.1f;

    public bool _roll;

    public Vector3 rollForward = Vector3.zero;

    public enum PlayerState
    {
        DEFAULT,
        MOVEMENT_LOCKED,
        MOVEMENT_BUFFER,
        ROLL,
    }

    public PlayerState _state;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _capsuleCollider = _defaultCollider;
        _isGrounded = false;
        _state = PlayerState.DEFAULT;
        _animator = GetComponent<Animator>();

        _jumpTimer = _jumpDelay;
        _movementLockedTimer = _movementLockedDelay;
        _movementBufferTimer = _movementBufferDelay;

        _geometry = transform.Find("Geometry");

        _defaultCan.SetActive(true);
        _rollingCan.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_jumpTimer < _jumpDelay)
        {
            _jumpTimer += Time.fixedDeltaTime;
        }

        if (_state == PlayerState.MOVEMENT_BUFFER) {
            if (_movementBufferTimer < _movementBufferDelay)
            {
                _movementBufferTimer += Time.deltaTime;
            } else
            {
                _state = PlayerState.DEFAULT;
            }
        }

        if (_state == PlayerState.MOVEMENT_LOCKED)
        {
            if (_movementLockedTimer < _movementLockedDelay)
            {
                _movementLockedTimer += Time.deltaTime;
            }
            else
            {
                _state = PlayerState.MOVEMENT_BUFFER;
                _movementBufferTimer = 0;
            }
        }
    }

    private void Update()
    {
        if (_state == PlayerState.DEFAULT && _roll)
        {
            _state = PlayerState.ROLL;
            StartRoll();
        }
        if (_state == PlayerState.ROLL && !_roll)
        {
            _state = PlayerState.DEFAULT;
            EndRoll();
        }
    }

    private void LateUpdate()
    {
        if (_state == PlayerState.ROLL)
        {
            _direction = Vector3.Lerp(_direction, _RollDirection, 0.01f);
        }

        Vector3 outDirection;

        Vector3 slopedDirection = _direction;

        if (_isGrounded)
        {
            var slope = Quaternion.FromToRotation(Vector3.up, _groundNormal);
            slopedDirection = slope * _direction;
        }

        DummyCheck();

        CheckGounded();

        if (_state == PlayerState.DEFAULT || _state == PlayerState.MOVEMENT_LOCKED || _state == PlayerState.ROLL)
        {
            CollisionCheck(slopedDirection, out outDirection, out _, 3);
            transform.position = transform.position + outDirection;
        }

        if (_state == PlayerState.DEFAULT || _state == PlayerState.MOVEMENT_LOCKED || _state == PlayerState.ROLL)
        {
            CollisionCheck(velocity, out outDirection, out _, 3);
            transform.position = transform.position + outDirection;
        }
    }

    public void OnMoveInput(Vector3 direction)
    {
        if (_state == PlayerState.DEFAULT)
        {
            float magnitude = direction.magnitude;

            if (direction != Vector3.zero && magnitude > 0.75f)
            {
                _direction = new Vector3(direction.x, 0.0f, direction.y).normalized * speed * Time.fixedDeltaTime;
                _state = PlayerState.MOVEMENT_LOCKED;
                _movementLockedTimer = 0.0f;

                _animator.SetTrigger("PlayerWalk");

                _geometry.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0.0f, direction.y));
            }
            else
            {
                _direction = Vector3.zero;
            }
        }

        if (_state == PlayerState.ROLL)
        {
            Vector3 d = new Vector3(direction.x, 0.0f, direction.y).normalized * speed * Time.fixedDeltaTime;

            float ratio = (Mathf.Abs(Vector3.Angle(rollForward, d) - 90) / 90);
            int forward = Vector3.Distance(rollForward, d) < Vector3.Distance(rollForward, -d) ? 1 : -1;

            if (direction == Vector3.zero)
                forward = 0;

            _RollDirection = (rollForward * ratio) * speed * Time.fixedDeltaTime * forward;
        }
    }

    public void OnJumpInput()
    {
        if (_isGrounded && _jumpTimer >= _jumpDelay)
        {
            velocity = Vector3.up * jumpForce * Time.deltaTime;

            _animator.SetTrigger("Jump");

            _jumpTimer = 0.0f;
        }
    }

    public void OnRollInput(bool roll)
    {
        _roll = roll;
    }

    public void StartRoll()
    {
        _capsuleCollider = _rollingCollider;

        _defaultCan.SetActive(false);
        _rollingCan.SetActive(true);

        rollForward = _geometry.forward;
    }

    public void EndRoll()
    {
        _capsuleCollider = _defaultCollider;

        _defaultCan.SetActive(true);
        _rollingCan.SetActive(false);

        rollForward = Vector3.zero;
    }

    public bool CollisionCheck(Vector3 direction, out Vector3 outDirection, out RaycastHit hit, int nBounces)
    {
        outDirection = direction;
        hit = new RaycastHit();

        for (int i = 0; i < nBounces; i++)
        {
            var param = new RayCastParams(transform, _capsuleCollider);

            var hitSomething = false;
            var closest = new RaycastHit() { distance = Mathf.Infinity };

            foreach (RaycastHit objHit in Physics.CapsuleCastAll(param.top, param.bottom, param.radius, direction, direction.magnitude).Where(c => c.transform != transform))
            {
                if (objHit.distance < closest.distance)
                {
                    closest = objHit;
                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                float bounceAngle = Vector3.Angle(closest.normal, direction) - 90;
                outDirection = Vector3.ProjectOnPlane(direction, closest.normal).normalized * direction.magnitude * ((90 - bounceAngle) / 90);
                hit = closest;
            }
            else
            {
                break;
            }
        }

        //true if we had a collision
        return outDirection != direction;
    }

    public void DummyCheck()
    {
        var range = 2.5f;
        var param = new RayCastParams(transform, _capsuleCollider);

        var hitSomething = false;
        var closest = new RaycastHit() { distance = Mathf.Infinity };
        
        var slope = Quaternion.FromToRotation(Vector3.up, _groundNormal);

        var normalledDirection = slope * _direction;

        //var normalledDirection = Vector3.ProjectOnPlane(_direction, _groundNormal).normalized * _direction.magnitude;

        foreach (RaycastHit objHit in Physics.CapsuleCastAll(param.top, param.bottom, param.radius, normalledDirection, range).Where(c => c.transform != transform))
        {
            if (objHit.distance < closest.distance)
            {
                closest = objHit;
                hitSomething = true;
            }
        }

        if (hitSomething)
        {
            var dir = normalledDirection.normalized * closest.distance;
            _dummy.transform.localPosition = new Vector3(dir.x, dir.y + 1.0f, dir.z);
        }
        else
        {
            var dir = normalledDirection.normalized * range;
            _dummy.transform.localPosition = new Vector3(dir.x, dir.y + 1.0f, dir.z);
        }
    }

    public void CheckGounded()
    {
        if (CollisionCheck(Vector3.down * 0.1f, out _, out RaycastHit hit, 1) && Vector3.Angle(hit.normal, Vector3.up) < _maxSlope)
        {
            _groundNormal = hit.normal;

            //we had a collision with the ground
            if (!_isGrounded)
            {
                //snap
                transform.position = transform.position + Vector3.down * hit.distance + Vector3.up * 0.01f;

                _isGrounded = true;
                velocity = Vector3.zero;

                if (_lastY - transform.position.y > 0.25f)
                {
                    _animator.SetTrigger("Land");
                    _lastY = transform.position.y;
                }

                _animator.SetBool("Airborne", false);
            }
        }
        else
        {
            _isGrounded = false;

            velocity += Vector3.down * _gravity * Time.deltaTime;

            _lastY = Mathf.Max(transform.position.y, _lastY);

            _animator.SetBool("Airborne", true);
        }

    }
}
