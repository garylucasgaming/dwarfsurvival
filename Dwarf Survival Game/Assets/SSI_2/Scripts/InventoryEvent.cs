using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[System.Serializable]
public class InventoryEvent
{
    public enum messageType { String = 0, Integer = 1, Decimal = 2, Bool = 3, ItemData = 4 }

    public string eventName;
    public string eventMessage;
    [Tooltip("Turn this off if you don't want the event to call, useful for debugging.")]
    public bool eventActivated = true;
    public Transform obj;
    public bool includeQuantityOfItem = false;
    public messageType dataToSend;
    public string stringParam;
    public int intParam;
    public float floatParam;
    public bool boolParam;

    
}
