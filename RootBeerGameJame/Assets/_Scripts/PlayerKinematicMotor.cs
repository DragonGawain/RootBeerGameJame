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
    private Animator _animator;

    private Vector3 _futurePosition;
    private Vector3 _direction;
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

    public enum PlayerState
    {
        DEFAULT,
    }

    public PlayerState _state;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _isGrounded = false;
        _state = PlayerState.DEFAULT;
        _animator = GetComponent<Animator>();

        _jumpTimer = _jumpDelay;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_jumpTimer < _jumpDelay)
        {
            _jumpTimer += Time.fixedDeltaTime;
        }
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {

        Vector3 outDirection;

        Vector3 slopedDirection = _direction;

        if (_isGrounded)
        {
            var slope = Quaternion.FromToRotation(Vector3.up, _groundNormal);
            slopedDirection = slope * _direction;
        }

        DummyCheck();

        CheckGounded();

        if (_state == PlayerState.DEFAULT)
        {
            CollisionCheck(slopedDirection, out outDirection, out _, 3);
            transform.position = transform.position + outDirection;
        }

        if (_state == PlayerState.DEFAULT)
        {
            CollisionCheck(velocity, out outDirection, out _, 3);
            transform.position = transform.position + outDirection;
        }
    }

    public void OnMoveInput(Vector3 direction)
    {
        _direction = new Vector3(direction.x, 0.0f, direction.y) * speed * Time.fixedDeltaTime;

        _animator.SetBool("Walking", direction != Vector3.zero);
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


            //if (_state == PlayerState.DIVING)
            //{
            //    velocity.x = velocity.x * 0.999f;
            //    velocity.z = velocity.z * 0.999f;
            //}
        }

    }
}
