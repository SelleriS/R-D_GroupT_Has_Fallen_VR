using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;


public class tracker : MonoBehaviour
{

    private SteamVR_TrackedObject trackedObject;
    public string DeviceSerial;

    // Start is called before the first frame update
    void Start()
    {
        trackedObject = transform.GetComponent<SteamVR_TrackedObject>();

        for (uint i = 0; i < 16; i++)
        {
            uint index = i;
            ETrackedPropertyError error = new ETrackedPropertyError();
            StringBuilder sb = new StringBuilder();
            OpenVR.System.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_SerialNumber_String, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
            string probablyUniqueDeviceSerial = sb.ToString();


            //if (probablyUniqueDeviceSerial == "LHR-4C5B295D" || probablyUniqueDeviceSerial == "LHR-9C9D4ADE")
            if (probablyUniqueDeviceSerial == DeviceSerial)
            {
                trackedObject.enabled = true;
                Debug.Log("tracker found");
                trackedObject.SetDeviceIndex((int)i);
                return;
            }
        }
    }
}
