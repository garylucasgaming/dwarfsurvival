using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Showcase_AmmoCreator : MonoBehaviour
{
    public int pistolAmmoDestroyed = 0;
    public int ammoToDestroy = 12;
    public string ammoName;
    public Transform pistolAmmoClone;
    void destroyItem(object[] data)
    {
        SSI2_ItemScript itemData = data[0] as SSI2_ItemScript;
        int amountDestroyed = (int)data[1];
        if(itemData.itemName == ammoName)
        {
            pistolAmmoDestroyed += amountDestroyed;
            if(pistolAmmoDestroyed >= ammoToDestroy)
            {
                pistolAmmoDestroyed -= ammoToDestroy;
                Transform ammoClone = Instantiate(pistolAmmoClone, transform.position, transform.rotation);
            }
        }
    }
}
