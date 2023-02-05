using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemBehaviour : MonoBehaviour
{
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    // Start is called before the first frame update
    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        //if (other.GetComponent<Collider>().isTrigger)
        {

            Destructible destructible = other.GetComponentInParent<Destructible>();

            if (destructible != null)
            {
                destructible.Shake();
            }

            //Debug.LogError("collision");
        }

        //Rigidbody rb = other.GetComponent<Rigidbody>();
        //int i = 0;

        //while (i < numCollisionEvents)
        //{
        //    if (rb)
        //    {
        //        Vector3 pos = collisionEvents[i].intersection;
        //        Vector3 force = collisionEvents[i].velocity * 10;
        //        rb.AddForce(force);
        //    }
        //    i++;
        //}
    }
}
