using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2_ItemScript : MonoBehaviour
{
    public enum rotDegs { Two = 0, Four = 1 }

    public bool equippable = false;

    public string itemName;
    public Sprite texture;
    public int width = 1;
    public int height = 1;
    public string itemDescription;
    public string itemTag;

    [Tooltip("How big a stack can this be? -1 = Infinite")]
    public int maxStack = 1;
    public int quantity = 1;
    [Tooltip("Can this display 0 as a quantity?")]
    public bool canBeEmpty = false;
    [Tooltip("When dropping this item, does it always drop everything?")]
    public bool dropEntireStack = false;

    public SSI2_UIItemScript uiAssignedTo;
    [Tooltip("Two = Rotations are either 0 or 90 degrees, Four = Rotations are either 0, 90, 180 or 270 degrees, default is Four")]
    public rotDegs degreesOfRotation = rotDegs.Four;
}
