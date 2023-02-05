using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollTexture : MonoBehaviour
{
    float x = 0.5f;
    float y = 0.5f;

    float time;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        x = Mathf.Sin(time) * 0.1f;
        y = y + Time.deltaTime * - 0.1f;

        //GetComponent<Renderer>().material.mainTextureOffset = new Vector2(x,y);

        GetComponent<Image>().material.mainTextureOffset = new Vector2(x, y);
    }
}
