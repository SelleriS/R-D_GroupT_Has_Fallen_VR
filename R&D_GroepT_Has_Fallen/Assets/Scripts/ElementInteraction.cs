using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public abstract class ElementInteraction : MonoBehaviour
{

    public WeaponType weaponType;
    private static readonly int weaponTypesBits;

    static ElementInteraction()
    {
        int numberOfWeaponTypes = Enum.GetNames(typeof(WeaponType)).Length; // Number of enum elements
        weaponTypesBits = (int) Math.Ceiling(Math.Log(numberOfWeaponTypes, 2)); //log2 of the different number of weapons. Log2 is always rounded up (=Ceiling)
    }

    public static WeaponType GetWeaponTypeOutOfData(byte[] packet)
    {
        byte[] flags = new byte[4];
        Array.Copy(packet, 3, flags, 0, 1);
        byte flagByte = flags[0];
        byte weaponTypeByte = (byte)(flagByte / Math.Pow(2, 8 - weaponTypesBits)); // bit shift right (the amount of bits that can be discarded = 3 + amount of bits that are not used)

        return (WeaponType)weaponTypeByte;
    }

    public void SetWeaponType(WeaponType weaponType)
    {
        this.weaponType = weaponType;
    }

    public static int GetWeaponTypeBits()
    {
        return weaponTypesBits;
    }
}
