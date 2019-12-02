using UnityEngine;

/*
 * This class is attached to the FiveSeven prefab and is responsible to apply
 * the received changes to it.
 * Only the trigger is implemented. The method for the trigger in this class
 * defers from its parent class by the axis over which the rotation must occur
 */

public class WeaponInteraction57 : WeaponInteraction
{
    private Transform triggerTransform;
    private Transform movablePieceTransform;
    private Transform selectorSwitchLTransform;
    private Transform selectorSwitchRTransform;
    private float triggerMinRot;
    private float triggerMaxRot;
    private float movablePieceMinPos;
    private float movablePieceMaxPos;
    private float selectorSwitchMinRot;
    private float selectorSwitchMaxRot;
    private float bulletForwardForce;
    private int triggerThreshold;
    private bool triggerReleased;
    private AudioSource audioSrc;
    private ParticleSystem muzzleFlash;

    public GameObject bulletEmitter; //Drag in the Bullet Emitter from the Component Inspector.
    public GameObject bullet; //Drag in the Bullet Prefab from the Component Inspector.
    // Start is called before the first frame update
    void Start()
    {
        triggerReleased = true;
        triggerThreshold = 200;
        bulletForwardForce = 1000;
        triggerMinRot = 13;
        triggerMaxRot = -15;
        movablePieceMinPos = 0.228999f;
        movablePieceMaxPos = 0.27f;
        selectorSwitchMinRot = 15;
        selectorSwitchMaxRot = 0;
        triggerTransform = transform.Find("Trigger").transform;
        movablePieceTransform = transform.Find("Slide").transform;
        selectorSwitchLTransform = transform.Find("SelectorSwitchL").transform;
        selectorSwitchRTransform = transform.Find("SelectorSwitchR").transform;
        audioSrc = GetComponent<AudioSource>();
        muzzleFlash = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        //Trigger
        SetTrigger(triggerCurrentValue);

        //[EXTRA] Selector Switch
        SetSelectorSwitch(selectorSwitchPosition);

        //[EXTRA] Movable pieces
        //SetMovablePieces(movablePiecesCurrentValue);
    }


    // Set the position of the trigger by rotating over the X-axis (for other objects the rotation happens over the Z-axis)
    private void SetTrigger(int triggerValue)
    {
        triggerCurrentValue = Mathf.Clamp(triggerValue, 0, 255);
        float triggerXRot = triggerMinRot + (triggerMaxRot - triggerMinRot) * ((float)triggerValue / 255); // Maps the current value of the trigger given by the client (value between 0 and 255) between triggerMinRot and triggerMaxRot
        triggerTransform.localEulerAngles = new Vector3(triggerXRot, triggerTransform.localEulerAngles.y, triggerTransform.localEulerAngles.z);

        if (triggerCurrentValue >= triggerThreshold)
        {
            if (triggerReleased)
            {
                triggerReleased = false;
                FireBullet();
                audioSrc.Play();
                //muzzleFlash.Play();
                //muzzleFlare.Stop();
            }
        }
        else { triggerReleased = true; }
    }

    // [EXTRA] Set the position of the selector switch
    private void SetSelectorSwitch(SwitchPosition switchPosition)
    {
        switch (switchPosition)
        {
            case SwitchPosition.SAFE:
                selectorSwitchLTransform.localEulerAngles = new Vector3(selectorSwitchMinRot, selectorSwitchLTransform.localEulerAngles.y, selectorSwitchLTransform.localEulerAngles.z);
                selectorSwitchRTransform.localEulerAngles = new Vector3(selectorSwitchMinRot, selectorSwitchRTransform.localEulerAngles.y, selectorSwitchRTransform.localEulerAngles.z);
                break;

            case SwitchPosition.SEMI:
                selectorSwitchLTransform.localEulerAngles = new Vector3(selectorSwitchMaxRot, selectorSwitchLTransform.localEulerAngles.y, selectorSwitchLTransform.localEulerAngles.z);
                selectorSwitchRTransform.localEulerAngles = new Vector3(selectorSwitchMaxRot, selectorSwitchRTransform.localEulerAngles.y, selectorSwitchRTransform.localEulerAngles.z);
                break;
            case SwitchPosition.AUTO: // Same as SEMI, because there is no AUTO mode on a Five Seven
                selectorSwitchLTransform.localEulerAngles = new Vector3(selectorSwitchMaxRot, selectorSwitchLTransform.localEulerAngles.y, selectorSwitchLTransform.localEulerAngles.z);
                selectorSwitchRTransform.localEulerAngles = new Vector3(selectorSwitchMaxRot, selectorSwitchRTransform.localEulerAngles.y, selectorSwitchRTransform.localEulerAngles.z);
                break;
        }
    }

    // [EXTRA] Set the position of the movable pieces with the value received from the UDP packet
    private void SetMovablePieces(int movablePiecesValue)
    {
        movablePiecesCurrentValue = Mathf.Clamp(movablePiecesValue, 0, 255);
        float movablePieceXPos = movablePieceMinPos + (movablePieceMaxPos - movablePieceMinPos) * ((float)movablePiecesValue / 255);
        movablePieceTransform.localPosition = new Vector3(movablePieceXPos, movablePieceTransform.localPosition.y, movablePieceTransform.localPosition.z);
    }

    private void FireBullet()
    {
        //The Bullet instantiation happens here
        GameObject Temporary_Bullet_Handler;
        Temporary_Bullet_Handler = Instantiate(bullet, bulletEmitter.transform.position, bulletEmitter.transform.rotation) as GameObject;


        //Retrieve the Rigidbody component from the instantiated Bullet and control it
        Rigidbody Temporary_RigidBody;
        Temporary_RigidBody = Temporary_Bullet_Handler.GetComponent<Rigidbody>();

        //Tell the bullet to be "pushed" forward by an amount set by Bullet_Forward_Force
        Temporary_RigidBody.AddForce(transform.right* -1 * bulletForwardForce); // multiply with -1 because transform.left does not exist

        //Basic Clean Up, set the Bullets to self destruct after 3 Seconds
        Destroy(Temporary_Bullet_Handler, 3.0f);
    }
}
