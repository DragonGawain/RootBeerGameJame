using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectorController : MonoBehaviour
{
    //manages points
    //checks if you collected something and does appropriate behaviour 
    [SerializeField] TMP_Text pointsDisplay;
    private float points;

    public float price = 5.0f;

    public Transform doors;
    bool done;

    // Start is called before the first frame update
    void Start()
    {
        points = 0;
        pointsDisplay.text = $"{points.ToString("F2")}$";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Collectables"))
        {
            if(other.gameObject.tag == "25cent")
            {
                points += .25f;
                pointsDisplay.text = $"{points.ToString("F2")}$";
                other.GetComponentInParent<AudioSource>().Play();
                Destroy(other.gameObject);
            }
        }

        if (other.gameObject.tag == "Door")
        {
            if (points >= price)
            {
                //open doors
                if(!done)
                {
                    done = true;
                    doors.gameObject.SetActive(false);
                }

            } else
            {
                GetComponent<PlayerKinematicMotor>().gm.SetText($"You need {price.ToString("F2")}$ to buy ice cream");
            }
        }

        if (other.gameObject.tag == "Goal")
        {
            GetComponent<PlayerKinematicMotor>().Win();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Door")
        {
            GetComponent<PlayerKinematicMotor>().gm.SetText("");
        }
    }
}
