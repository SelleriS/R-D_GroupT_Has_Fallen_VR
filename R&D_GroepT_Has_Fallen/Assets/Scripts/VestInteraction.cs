using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VestInteraction : ElementInteraction
{
    public byte[] messageToSend;

    // Variables used for debugging
    public bool front;
    public bool back;
    public bool left;
    public bool right;
    public bool resetAll;

    // Start is called before the first frame update
    void Start()
    {
        messageToSend = new byte[4];
        ResetMessageToSend();
    }

    // Update is called once per frame
    void Update()
    {
        SensorTest();
    }

    public byte[] GetMessageToSend()
    {
        return messageToSend;
    }

    public void ResetMessageToSend()
    {
        messageToSend[0] = 0;
        messageToSend[1] = 0;
        messageToSend[2] = 0;
        messageToSend[3] = 0;
    }

    private void HitFront()
    {
        messageToSend[0] = 0xFF;
    }

    private void HitBack()
    {
        messageToSend[1] = 0xFF;
    }

    private void HitLeft()
    {
        messageToSend[2] = 0xFF;
    }

    private void HitRight()
    {
        messageToSend[3] = 0xFF;
    }

    private void SensorTest()
    {
        if (front) { HitFront(); }
        if (back) { HitBack(); }
        if (left) { HitLeft(); }
        if (right) { HitRight(); }
        if (resetAll)
        {
            ResetMessageToSend();
            resetAll = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided");
        Vector3 cameraRelative = this.transform.InverseTransformPoint(collision.transform.position);

        if (cameraRelative.x > 0)
        {
            Debug.Log("right");
            HitRight();
        }
        if (cameraRelative.x < 0)
        {
            Debug.Log("left");
            HitLeft();
        }
        if (cameraRelative.z > 0)
        {
            Debug.Log("front");
            HitFront();
        }
        if (cameraRelative.x < 0)
        {
            Debug.Log("back");
            HitBack();
        }
    }
}