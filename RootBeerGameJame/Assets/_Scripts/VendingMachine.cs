using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    public bool isAnimationOver;
    public Camera cam;

    public GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AnimationOver()
    {
        isAnimationOver = true;

        cam.gameObject.SetActive(false);

        gm.StartGame();
    }

    public void Reset()
    {
        isAnimationOver = false;
        cam.gameObject.SetActive(true);
    }
}
