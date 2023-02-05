using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    private Animator animator;
    public float health = 100;
    public float damageTaken = 1;

    private bool takeDamage = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public void Shake()
    {
        animator.SetTrigger("Shake");

        health -= damageTaken;

        if (health < 0)
            Destroy(gameObject);

        takeDamage = true;
    }
}
