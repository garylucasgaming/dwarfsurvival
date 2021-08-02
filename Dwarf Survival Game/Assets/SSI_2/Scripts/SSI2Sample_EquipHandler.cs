using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Sample_EquipHandler : MonoBehaviour
{
    //You may wish to use this script to insert your own code/refer to other objects for handling specific equipable items, such as guns, food items, keys etc

    public SSI2_InventoryScript inventoryHandler;
    public SSI2_ItemScript equippedItem;

    public Transform viewmodelCube;
    // Start is called before the first frame update

    private void Update()
    {
        if (equippedItem != null)
        {
            if (equippedItem.itemTag == "Backpack" && Input.GetKeyDown(KeyCode.Mouse0))
            {
                inventoryHandler.CreateGrid(equippedItem.GetComponent<SSI2_GridProperties>(), true);
                //inventoryHandler.DequipItem();
            }
        }
    }
    void equipItem(SSI2_ItemScript item)
    {
        equippedItem = item;
        if(item.itemTag == "Generic" || item.itemTag == "Backpack")
        {
            viewmodelCube.gameObject.SetActive(true);
            viewmodelCube.localScale = new Vector3(0.2f * item.width, 0.2f * item.height, 0.2f);
        }
    }

    // Update is called once per frame
    void dequipItem(SSI2_ItemScript item)
    {
        equippedItem = null;
        if (item.itemTag == "Generic" || item.itemTag == "Backpack")
        {
            viewmodelCube.gameObject.SetActive(false);
        }
    }
}
