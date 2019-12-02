using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/*
 * This class is the parent class that is responsible to decode and apply 
 * the received changes on the weapon to which it is linked to.
 * It is the child classes that are linked to the weapon prefabs.
 * The common methods of the child classes are implemnted in this class.
 * Every child class has a specific start method to set transformation parameters.
 * Some child classes have extra methods implemented to apply changes to different
 * features of the weapon in question.
 * Other child classes override common methods because a variation on the method
 * must be executed.
 */

public abstract class WeaponInteraction : ElementInteraction
{
    public IPAddress ipAddress;
    public SwitchPosition selectorSwitchPosition;
    public int triggerCurrentValue;
    public int movablePiecesCurrentValue;
    public bool magazinePresent;
    public int magazineID;
    public int amountOfBulletsInMagazine;
    public bool weaponLoaded; // If the criteria are met to load the weapon and the weapon gets loaded, this variable is set.

    private DateTime lastUpdate; // Used to check if a weapon has been active


    // Translate the data and edit the parameters of the virtual weapon
    public void PacketTranslater(byte[] packet) // if reading problems occur, this can be due to the endian (the sequence in which the bits are send, LSB first or last?)
    {
        // Check if the data is still send by the same type of weapon. It should be impossible for a weapon to change type (this is just in case)
        // If the client keeps sending packets with the wrong weapontype tag, it will be considered as inactive and the weapon will get deleted after a certain time
        if (GetWeaponTypeOutOfData(packet) == weaponType)
        {
            // Create storage containers
            byte[] triggerValue = new byte[4];  
            byte[] movablePiecesValue = new byte[4];
            byte[] magazineIDValue = new byte[4];
            byte[] flags = new byte[4];


            // Store data in containers
            Array.Copy(packet, 0, triggerValue, 0, 1); //Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
            Array.Copy(packet, 1, movablePiecesValue, 0, 1);
            Array.Copy(packet, 2, magazineIDValue, 0, 1);
            Array.Copy(packet, 3, flags, 0, 1);

            // Set current values
            triggerCurrentValue = BitConverter.ToInt32(triggerValue, 0);
            movablePiecesCurrentValue = BitConverter.ToInt32(movablePiecesValue, 0);// BitConverter start reading at a certain index till the end. 
            magazineID = BitConverter.ToInt32(magazineIDValue, 0);
            byte flagByte = flags[0];

            magazinePresent = !(flagByte % 2 == 0); // Last bit in the flag byte shows the mag status. So, if there is no mag present the value of the flag is even.

            byte switchPositionByte = (byte)(flagByte * Math.Pow(2, GetWeaponTypeBits())); // bit shift left the enough of times to get rid of the used MSB's to encode the weapontype
            switchPositionByte /= (byte)Math.Pow(2, GetWeaponTypeBits() + 1); // BSR enough times to set the bits needed to recognize the selector switches position as LSB's
            selectorSwitchPosition = (SwitchPosition)switchPositionByte;

            lastUpdate = DateTime.Now;// Use of this has to be reconsidered => it takes too much time
        }
    }

    public DateTime GetLastUpdate()
    {
        return lastUpdate;
    }

    // Setter for ipAdress
    public void SetIPAddress(IPAddress ipAddress)
    {
        this.ipAddress = ipAddress;
    }

}
