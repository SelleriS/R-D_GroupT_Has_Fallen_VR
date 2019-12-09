using UnityEngine;

/*
 * This class is attached to the Gatling gun prefab and is responsible to apply
 * the received changes to it.
 * Only the trigger is implemented. The method for the trigger in this class
 * defers from its parent class by the axis over which the rotation must occur
 */

public class WeaponInteractionGG : WeaponInteraction
{
    private Transform triggerTransform;
    private Transform movablePieceTransform;
    private float triggerMinRot;
    private float triggerMaxRot;
    private float minRPM;
    private float maxRPM;
    private float totalDegreesRotated;
    private float bulletForwardForce;
    private AudioSource audioSrc;
    private ParticleSystem muzzleFlash;
    private bool isFlashing;

    public GameObject bulletEmitter; //Drag in the Bullet Emitter from the Component Inspector.
    public GameObject bullet; //Drag in the Bullet Prefab from the Component Inspector.

    // Start is called before the first frame update
    void Start()
    {
        triggerTransform = transform.Find("Trigger").transform;
        movablePieceTransform = transform.Find("Barrels").transform;
        minRPM = 0;
        maxRPM = 400;
        bulletForwardForce = 4000;
        audioSrc = GetComponent<AudioSource>();
        muzzleFlash = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        //Trigger
        SetTrigger(triggerCurrentValue);

        //Movable pieces
        SetMovablePieces(movablePiecesCurrentValue);
    }


    // Set the position of the trigger by rotating over the X-axis (for other objects the rotation happens over the Z-axis)
    private void SetTrigger(int triggerValue)
    {
        triggerCurrentValue = Mathf.Clamp(triggerValue, 0, 255);
        float remapRPM = minRPM + (maxRPM - minRPM) * ((float)triggerCurrentValue / 255);
        float increment = (remapRPM / (60 * 90)) * 360; // devide by 60 to have rounds per second. Devide by 90 becuse VR is 90fps. 360 because a round is 360 degrees
        float triggerYRot = triggerTransform.localEulerAngles.y + increment;
        totalDegreesRotated += increment;
        triggerTransform.localEulerAngles = new Vector3(triggerTransform.localEulerAngles.x, -triggerYRot, triggerTransform.localEulerAngles.z);
        if(triggerCurrentValue != 0 && totalDegreesRotated >= 60) //There are 6 barrels int total => shoot every 60°
        {
            FireBullet();
            audioSrc.Play();
            //EmitMuzzleFlash();
            totalDegreesRotated = 0; //Reset after every shot
        }
        isFlashing = false;
    }

    // [EXTRA] Set the position of the movable pieces with the value received from the UDP packet
    private void SetMovablePieces(int movablePiecesValue)
    {
        movablePiecesCurrentValue = Mathf.Clamp(movablePiecesValue, 0, 255);
        float remapRPM = minRPM + (maxRPM - minRPM) * ((float)movablePiecesValue / 255);
        float increment = (remapRPM / (60 * 90)) * 360;
        float movablePieceZRot = movablePieceTransform.localEulerAngles.z + increment;
        movablePieceTransform.localEulerAngles = new Vector3(movablePieceTransform.localEulerAngles.x, movablePieceTransform.localEulerAngles.y, movablePieceZRot);
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
        Temporary_RigidBody.AddForce(transform.forward *-1  * bulletForwardForce); // multiply with -1 because transform.left does not exist

        //Basic Clean Up, set the Bullets to self destruct after 3 Seconds
        Destroy(Temporary_Bullet_Handler, 3.0f);
    }

    private void EmitMuzzleFlash()
    {
        if (!isFlashing)
        {
            muzzleFlash.Emit(20);
            isFlashing = true;
        }
    }
}
