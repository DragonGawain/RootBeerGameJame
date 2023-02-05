using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class OverviewVCamMovement : MonoBehaviour
{
    public CinemachineVirtualCamera vcam2;
    public CinemachineTrackedDolly dolly;
    // Start is called before the first frame update
    void Start()
    {
        dolly = vcam2.GetCinemachineComponent<CinemachineTrackedDolly>();
        dolly.m_PathPosition = 4;
    }

    // Update is called once per frame
    void Update()
    {
        dolly.m_PathPosition = dolly.m_PathPosition - 0.005f; //set speed to 0.05  

        if (dolly.m_PathPosition <= 0) {
            vcam2.VirtualCameraGameObject.SetActive(false);
        }
    }
}
