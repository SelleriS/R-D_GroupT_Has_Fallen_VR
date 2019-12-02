using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VestInteraction : ElementInteraction
{
    private byte[] messageToSend;

    // Variables used for debugging
    public bool front;
    public bool back;
    public bool left;
    public bool right;

    // Start is called before the first frame update
    void Start()
    {
        messageToSend = new byte[4];
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

    private void UpdateMessageToSend()
    {

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
        else if (back) { HitBack(); }
        else if (left) { HitLeft(); }
        else if (right) { HitRight(); }
    }

}
