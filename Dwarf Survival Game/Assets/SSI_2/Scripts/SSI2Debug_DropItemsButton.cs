using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Debug_DropItemsButton : MonoBehaviour
{

    public SSI2_InventoryScript inventory;
    public string itemTag;
    public int quantity;
    public bool ignoreStacks = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Drop()
    {
        inventory.DropItemsOfType(itemTag, quantity, ignoreStacks, false);
    }
}
