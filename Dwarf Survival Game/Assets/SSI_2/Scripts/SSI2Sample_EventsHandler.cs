using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSI2Sample_EventsHandler : MonoBehaviour
{
    public RectTransform notifPrefab;
    
    void addItem(SSI2_ItemScript item)
    {
        RectTransform prefab = Instantiate(notifPrefab, transform);
        prefab.Find("Text").GetComponent<Text>().text = $"Picked up {item.itemName}";
        prefab.GetComponent<Animation>().Play("notificationPopup");
    }

    void rejectItem(SSI2_ItemScript item)
    {
        RectTransform prefab = Instantiate(notifPrefab, transform);
        prefab.Find("Text").GetComponent<Text>().text = $"Cannot pick up {item.itemName}";
        prefab.GetComponent<Animation>().Play("notificationPopup");
    }

    void dropItem(SSI2_ItemScript item)
    {
        RectTransform prefab = Instantiate(notifPrefab, transform);
        prefab.Find("Text").GetComponent<Text>().text = $"Dropped {item.itemName}";
        prefab.GetComponent<Animation>().Play("notificationPopup");
    }

    void destroyItem(SSI2_ItemScript item)
    {
        RectTransform prefab = Instantiate(notifPrefab, transform);
        prefab.Find("Text").GetComponent<Text>().text = $"Destroyed {item.itemName}";
        prefab.GetComponent<Animation>().Play("notificationPopup");
    }

    void destroyItemQuant(object[] data)
    {
        SSI2_ItemScript item = data[0] as SSI2_ItemScript;
        int quantity = (int)data[1];

        RectTransform prefab = Instantiate(notifPrefab, transform);
        prefab.Find("Text").GetComponent<Text>().text = $"Destroyed {item.itemName}";
        prefab.GetComponent<Animation>().Play("notificationPopup");
    }

    void transferItem(SSI2_ItemScript item)
    {
        RectTransform prefab = Instantiate(notifPrefab, transform);
        prefab.Find("Text").GetComponent<Text>().text = $"Transferred {item.itemName}";
        prefab.GetComponent<Animation>().Play("notificationPopup");
    }

    void failTransferItem(SSI2_ItemScript item)
    {
        RectTransform prefab = Instantiate(notifPrefab, transform);
        prefab.Find("Text").GetComponent<Text>().text = $"Failed to transfer {item.itemName}";
        prefab.GetComponent<Animation>().Play("notificationPopup");
    }
}
