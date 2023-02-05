using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectorController : MonoBehaviour
{
    //manages points
    //checks if you collected something and does appropriate behaviour 
    [SerializeField] TMP_Text pointsDisplay;
    private int points;

    // Start is called before the first frame update
    void Start()
    {
        points = 0;
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
                points += 25;
                pointsDisplay.text = points.ToString();
                other.GetComponentInParent<AudioSource>().Play();
                Destroy(other.gameObject);
            }
        }
    }
}
