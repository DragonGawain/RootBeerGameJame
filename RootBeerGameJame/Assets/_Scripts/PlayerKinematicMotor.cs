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
    public GameManager gm;

    public GameObject _dummy;
    private Rigidbody rb;
    public CapsuleCollider _capsuleCollider;
    public CapsuleCollider _defaultCollider;
    public CapsuleCollider _rollingCollider;
    private Animator _animator;
    private Transform _geometry;
    public Transform root;
    public Transform rollingRoot;

    public GameObject _defaultCan;
    public GameObject _rollingCan;
    public Transform _rollingCanTransform;

    private Vector3 _futurePosition;
    public Vector3 _direction;
    public Vector3 _RollDirection;
    public bool _isGrounded;

    public Vector3 _groundNormal;
    private float _lastY;

    public float speed = 0.10f;
    public float flySpeed = 0.10f;
    public float flyAngle = 45;
    public float rollSpeed;
    //public int nBounces = 3;
    public float _gravity = 9.8f;

    public bool falling = false;
    public bool canJump = true;
    public Vector3 velocity;
    public float jumpForce;
    public float jumpForceDown;
    public float _maxSlope = 80;

    public float _jumpTimer = 0.0f;
    public float _jumpDelay = 0.1f;

    public float _movementLockedTimer = 0.0f;
    public float _movementLockedDelay = 0.1f;

    public float _movementBufferTimer = 0.0f;
    public float _movementBufferDelay = 0.1f;

    public bool _roll;
    public bool _fly;

    public Vector3 rollForward = Vector3.zero;

    public bool tabUp = true;
    public ParticleSystem _ps;
    public ParticleSystem deathParticles;

    public float maxVelocity;

    public float currentMaxY;
    public float currentMaxYDelta = 7;

    public HealthBar healthBar;

    public Vector3 spawnPoint;

    public AudioSource walkAudio;
    public AudioSource shootAudio;
    public AudioSource ExplodeAudio;

    public enum PlayerState
    {
        DEFAULT,
        MOVEMENT_LOCKED,
        MOVEMENT_BUFFER,
        ROLL,
        FLY,
        SHOOTING,
        SHOOTING_BUFFER,
        DEAD,
        SPAWNING,
        WON
    }

    public PlayerState _state;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _capsuleCollider = _defaultCollider;
        _isGrounded = false;
        _state = PlayerState.SPAWNING;
        _animator = GetComponent<Animator>();

        _jumpTimer = _jumpDelay;
        _movementLockedTimer = _movementLockedDelay;
        _movementBufferTimer = _movementBufferDelay;

        _geometry = transform.Find("Geometry");

        _defaultCan.SetActive(false);
        _rollingCan.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_state == PlayerState.MOVEMENT_BUFFER) {
            if (_movementBufferTimer < _movementBufferDelay)
            {
                _movementBufferTimer += Time.fixedDeltaTime;
            } else
            {
                _state = PlayerState.DEFAULT;
                tabUp = !tabUp;

                healthBar.AddHealth(0.1f);
            }
        }

        if (_state == PlayerState.MOVEMENT_LOCKED)
        {
            if (_movementLockedTimer < _movementLockedDelay)
            {
                _movementLockedTimer += Time.fixedDeltaTime;
            }
            else
            {
                _state = PlayerState.MOVEMENT_BUFFER;
                _direction = Vector3.zero;
                _movementBufferTimer = 0;

                walkAudio.pitch = Random.Range(0.9f, 1.1f);
                walkAudio.Play();
            }
        }

        if (_state == PlayerState.FLY)
        {
            if (transform.position.y > currentMaxY)
            {
                velocity = Vector3.Lerp(velocity, Vector3.zero, 2 * Time.fixedDeltaTime);
            }
            else
            {
                velocity = Vector3.up * jumpForce * Time.fixedDeltaTime;
            }

            healthBar.LoseHealth(0.1f * Time.deltaTime);
        }

        if (healthBar.IsDead())
        {
            //explode
            OnDieInput(true);
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

        if (_state == PlayerState.DEFAULT && _fly && !tabUp && healthBar.Alive())
        {
            _state = PlayerState.FLY;
            StartFly();
        }
        if (_state == PlayerState.FLY && (!_fly || !healthBar.Alive()))
        {
            _state = PlayerState.DEFAULT;
            EndFly();
        }

        if (_state == PlayerState.DEFAULT && _fly && tabUp && healthBar.Alive())
        {
            _state = PlayerState.SHOOTING;
            StartShooting();
        }
        if (_state == PlayerState.SHOOTING && (!_fly || !healthBar.Alive()))
        {
            _state = PlayerState.SHOOTING_BUFFER;
            EndShooting();
        }

        if (_state == PlayerState.DEAD && _fly)
        {
            //_state = PlayerState.DEFAULT;
            //transform.position = spawnPoint + new Vector3(0, 0.1f,0);

            //_defaultCan.SetActive(true);
            //_fly = false;
            gm.Reset();

            _state = PlayerState.SPAWNING;
        }
    }

    public void Activate(bool b)
    {
        if (b)
        {
            _defaultCan.SetActive(true);

            _state = PlayerState.DEFAULT;

        }
        else
        {
            _defaultCan.SetActive(false);
        }
    }

    public void Spawn(Vector3 spawn)
    {
        spawnPoint = spawn;

        _fly = false;

        transform.position = spawnPoint + new Vector3(0, 0.3f, 0);

        GetComponent<PlayerController>().Reset();

        _state = PlayerState.SPAWNING;

        //_defaultCan.SetActive(true);
    }

    private void LateUpdate()
    {
        if (_state == PlayerState.ROLL)
        {
            if (_RollDirection.magnitude < _direction.magnitude)
            {
                //we are going slower
                _direction = Vector3.Lerp(_direction, _RollDirection, 0.005f);
            }
            else
            {
                //we are going faster -> dont slow down much
                _direction = Vector3.Lerp(_direction, _RollDirection, 0.05f);
            }
            //_direction = Vector3.Lerp(_direction, _RollDirection, 0.005f);

            _rollingCanTransform.Rotate(Vector3.down * 100 * _direction.magnitude, Space.Self);
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

        if (_state == PlayerState.DEFAULT || _state == PlayerState.MOVEMENT_LOCKED || _state == PlayerState.MOVEMENT_BUFFER || _state == PlayerState.ROLL || _state == PlayerState.FLY || _state == PlayerState.SHOOTING)
        {
            CollisionCheck(slopedDirection, out outDirection, out _, 3);
            transform.position = transform.position + outDirection;
        }

        if (_state == PlayerState.DEFAULT || _state == PlayerState.MOVEMENT_LOCKED || _state == PlayerState.MOVEMENT_BUFFER || _state == PlayerState.ROLL || _state == PlayerState.FLY || _state == PlayerState.SHOOTING)
        {
            CollisionCheck(velocity, out outDirection, out _, 3);
            transform.position = transform.position + outDirection;
        }
    }

    public void OnMoveInput(Vector3 direction)
    {
        float t = 5 * Time.deltaTime;

        if (_state == PlayerState.DEFAULT && _isGrounded && !(!tabUp && _fly))
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
            //Vector3 d = new Vector3(direction.x, 0.0f, direction.y).normalized * speed * Time.fixedDeltaTime;

            //float ratio = (Mathf.Abs(Vector3.Angle(rollForward, d) - 90) / 90);
            //int forward = Vector3.Distance(rollForward, d) < Vector3.Distance(rollForward, -d) ? 1 : -1;

            //if (direction == Vector3.zero)
            //    forward = 0;

            if (_isGrounded)
            {
                var angle = Vector3.Angle(Vector3.up, _groundNormal);

                //if (downwards)
                //{
                _RollDirection = rollForward * rollSpeed * Time.fixedDeltaTime * angle / 90;
                //}

                RollRotation();
            }

            //_RollDirection = (rollForward * ratio) * rollSpeed * Time.fixedDeltaTime * forward;
        }


        if (_state == PlayerState.FLY && !tabUp)
        {
            if (direction != Vector3.zero)
            {
                _geometry.rotation = Quaternion.Lerp( _geometry.rotation, Quaternion.LookRotation(new Vector3(direction.x, 0.0f, direction.y)), t);
            } 

            _direction = Vector3.Lerp(_direction, new Vector3(direction.x, 0.0f, direction.y).normalized * flySpeed * Time.fixedDeltaTime, t);

            root.localRotation = Quaternion.Lerp(root.localRotation, Quaternion.Euler(direction.magnitude * flyAngle, 0.0f, 0.0f), t);
        }

        if (_state == PlayerState.DEFAULT && !_isGrounded && !tabUp)
        {
            //we are falling 
            root.localRotation = Quaternion.Lerp(root.localRotation, Quaternion.identity, t);
        }

        if (_state == PlayerState.SHOOTING && tabUp)
        {
            _direction = Vector3.zero;

            healthBar.LoseHealth(0.1f * Time.deltaTime);

            if (!_isGrounded)
            {
                //shoot down
                velocity += Vector3.down * jumpForceDown * Time.fixedDeltaTime;
                velocity.y = Mathf.Max(velocity.y, maxVelocity * Time.fixedDeltaTime);
            }
            else
            {
                if (direction != Vector3.zero)
                {
                    _geometry.rotation = Quaternion.Lerp(_geometry.rotation, Quaternion.LookRotation(new Vector3(direction.x, 0.0f, direction.y)), t);
                }

                root.localRotation = Quaternion.Lerp(root.localRotation, Quaternion.Euler(direction.magnitude * 60.0f, 0.0f, 0.0f), t);

                root.localPosition = Vector3.Lerp(root.localPosition, new Vector3(0f, 0.4f * direction.magnitude, 0f), t);
            }
        }

        if (_state == PlayerState.SHOOTING_BUFFER && tabUp)
        {
            if (direction != Vector3.zero)
            {
                _geometry.rotation = Quaternion.Lerp(_geometry.rotation, Quaternion.LookRotation(new Vector3(direction.x, 0.0f, direction.y)), t * 2);
            }

            root.localRotation = Quaternion.Lerp(root.localRotation, Quaternion.identity, t * 2);

            root.localPosition = Vector3.Lerp(root.localPosition, Vector3.zero, t);

            if (Quaternion.Angle(root.localRotation, Quaternion.identity) < 0.1f)
            {
                root.localRotation = Quaternion.identity;
                _state = PlayerState.DEFAULT;
            }
        }
    }

    public void Win()
    {
        _state = PlayerState.WON;

        gm.SetText("YOU WON");

        deathParticles.Play();

        ExplodeAudio.Play();

        _ps.Stop();

        shootAudio.Stop();
        shootAudio.Stop();

        _defaultCan.SetActive(false);
    }

    public void OnJumpInput(bool jump)
    {
        //if (_isGrounded && _jumpTimer >= _jumpDelay)
        //{
        //    velocity = Vector3.up * jumpForce * Time.deltaTime;

        //    _animator.SetTrigger("Jump");

        //    _jumpTimer = 0.0f;
        //}

        //if (_state == PlayerState.DEFAULT)
        //{
        //    _state = PlayerState.FLY;
        //}

        _fly = jump;

        //if (_fly && tabUp)
        //{
        //    velocity = Vector3.up * jumpForce * Time.fixedDeltaTime;
        //} elses
        //{
        //    _fly = false;
        //}
    }

    public void OnDieInput(bool die)
    {
        if (die && _state != PlayerState.DEAD && _state != PlayerState.SPAWNING)
        {
            healthBar.LoseHealth(1.25f);

            _state = PlayerState.DEAD;

            deathParticles.Play();

            ExplodeAudio.Play();

            tabUp = true;

            _animator.SetTrigger("Reset");

            _defaultCan.SetActive(false);

            gm.SetText("PRESS A TO TRY AGAIN");

            _ps.Stop();

            shootAudio.Stop();
            shootAudio.Stop();

            _fly = false;
        }
    }

    public void StartFly()
    {
        velocity = Vector3.up * jumpForce * Time.fixedDeltaTime;

        _ps.Play();

        shootAudio.Play();
    }

    public void EndFly()
    {
        //velocity = Vector3.zero;
        //root.localRotation = Quaternion.identity;

        _ps.Stop();

        shootAudio.Stop();
    }

    public void StartShooting()
    {
        _ps.Play();

        shootAudio.Play();
    }

    public void EndShooting()
    {
        //root.localRotation = Quaternion.identity;
        _ps.Stop();

        shootAudio.Stop();
    }

    public void OnRollInput(bool roll)
    {
        //_roll = roll;
    }

    public void RollRotation()
    {
        //float bounceAngle = Vector3.Angle(_groundNormal, Vector3.down) - 90;
        Vector3 downVector = Vector3.ProjectOnPlane(Vector3.down, _groundNormal);

        Vector3 geometryVector = new Vector3(downVector.x, 0, downVector.z);


        if (downVector != Vector3.zero)
        {
            //rollingRoot.localRotation = Quaternion.identity;
            _geometry.rotation = Quaternion.LookRotation(geometryVector);
        }
        else
        {
            //rollingRoot.rotation = Quaternion.LookRotation(downVector);
        }

        //rollForward = rollingRoot.forward;
        //rollForward.y = 0;
        //rollForward.Normalize();

        rollForward = _geometry.forward;
        rollForward.Normalize();
    }

    public void StartRoll()
    {
        _capsuleCollider = _rollingCollider;

        _defaultCan.SetActive(false);
        _rollingCan.SetActive(true);

        _RollDirection = Vector3.zero;

        RollRotation(); 
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

            foreach (RaycastHit objHit in Physics.CapsuleCastAll(param.top, param.bottom, param.radius, direction, direction.magnitude).Where(c => (c.transform != transform && !c.collider.isTrigger)))
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
        if (CollisionCheck(Vector3.down * (0.1f - velocity.y), out _, out RaycastHit hit, 1) && Vector3.Angle(hit.normal, Vector3.up) < _maxSlope)
        {
            _groundNormal = hit.normal;

            currentMaxY = hit.point.y + currentMaxYDelta;

            //we had a collision with the ground
            if (!_isGrounded)
            {
                //snap
                transform.position = transform.position + Vector3.down * hit.distance + Vector3.up * 0.01f;

                _isGrounded = true;
                velocity = Vector3.zero;
                root.localRotation = Quaternion.identity;

                if (_lastY - transform.position.y > 0.25f)
                {
                    //_animator.SetTrigger("Land");
                    _lastY = transform.position.y;
                }

               // _animator.SetBool("Airborne", false);
            }
        }
        else
        {
            _isGrounded = false;

            if (_state != PlayerState.FLY)
            {
                velocity += Vector3.down * _gravity * Time.deltaTime;
                velocity.y = Mathf.Max(velocity.y, maxVelocity * Time.fixedDeltaTime);
            }

            _lastY = Mathf.Max(transform.position.y, _lastY);

            //_animator.SetBool("Airborne", true);
        }

    }
}
