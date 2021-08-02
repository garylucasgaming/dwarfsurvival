using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Showcase_EquipHandler2 : MonoBehaviour
{
    //This script is designed to dictate whether or not we need to transfer items when we equip them, and equipping them via a hotkey.
    public KeyCode equipKey;
    public SSI2_GridScript equipGrid;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(equipKey) && equipGrid.inventory.Count > 0)
        {
            equipGrid.handler.EquipItem(equipGrid.inventory[0].item);
        }
    }

    void EquippingItem(SSI2_ItemScript item)
    {
        // First, we're going to check if the item we're equipping is the same as the one in the grid, if not, we'll need to swap them.
        if (equipGrid.inventory.Count > 0)
        {
            if (item != equipGrid.inventory[0].item.assignedItem)
            {
                equipGrid.SnatchFrom();
                equipGrid.SnatchItem(item);
            }
        }
        else
        {
            equipGrid.SnatchItem(item);
        }
    }
}
