using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class OverviewVCamMovement : MonoBehaviour
{
    public CinemachineVirtualCamera vcam3;
    public CinemachineTrackedDolly dolly;
    // Start is called before the first frame update
    void Start()
    {
        dolly = vcam3.GetCinemachineComponent<CinemachineTrackedDolly>();
        dolly.m_PathPosition = 2;
    }

    // Update is called once per frame
    void Update()
    {
        dolly.m_PathPosition = dolly.m_PathPosition - 0.005f; //set speed to 0.005  
        if (dolly.m_PathPosition <= 0) {
            
            vcam3.VirtualCameraGameObject.SetActive(false);
        }
    }
}
