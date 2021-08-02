using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Showcase_ItemInserter : MonoBehaviour
{
    //This showcase script demonstrates the InsertItem function added in version 1.2
    //To use it, assign the handler script in the Inspector, then run your scene.

    public SSI2_InventoryScript handler;
    void Start()
    {
        handler.InsertItem(transform);
        Destroy(this);
    }
}
