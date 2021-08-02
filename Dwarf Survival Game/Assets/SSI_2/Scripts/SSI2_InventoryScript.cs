using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSI2_InventoryScript : MonoBehaviour
{
    [System.Serializable]
    public class uniqueEvent {
        public string eventName;
        public string eventMessage;
        public bool eventActivated = true;
        public Transform eventObj;
    }

    Camera mainCam;
    public SSI2_GlobalVariables globalVars;

    [Header("UI")]
    public bool hideMouseWithInventory = true;
    public RectTransform viewport;
    public Canvas canv;
    public bool inventoryOpen = true;
    public RectTransform header;
    public RectTransform gridPanel;

    // EVENTS
    [Header("Item Adding/Transferring")]
    [Header("Events")]
    [Tooltip("These events are called before an item is added")]
    public List<InventoryEvent> addingItemEvents;

    [Tooltip("These events are called after an item is added")]
    public List<InventoryEvent> itemAddedEvents;

    [Tooltip("These events are called after an item is rejected (unable to be picked up")]
    public List<InventoryEvent> rejectItemEvents;

    [Tooltip("These events are called before an item is transfered")]
    public List<InventoryEvent> transferingItemEvents;

    [Tooltip("These events are called after an item is transfered")]
    public List<InventoryEvent> transferedItemEvents;

    [Tooltip("These events are called after an item fails to transfer")]
    public List<InventoryEvent> failTransferEvents;

    [Header("Item Disposal")]
    [Tooltip("These events are called after an item is dropped")]
    public List<InventoryEvent> dropItemEvents;

    [Tooltip("These events are called after an item is destroyed")]
    public List<InventoryEvent> destroyItemEvents;

    [Header("Equipping Items")]
    [Tooltip("These events are called after an item is equipped")]
    public List<InventoryEvent> equipItemEvents;

    [Tooltip("These events are called after an item is dequipped")]
    public List<InventoryEvent> dequipItemEvents;

    public SSI2_UIItemScript equippedItem;

    [Header("Unique Events")]
    [Tooltip("These events are called when one object is dragged and dropped onto another object")]
    public List<uniqueEvent> overlapEvents;

    [Header("Inventory Grids")]
    public RectTransform gridHolder;

    public RectTransform gridPrefab;

    public float gridScale = 1f;

    [Tooltip("You may wish to add to this list if you plan on having multiple inventory grids that can be added or removed")]
    public List<SSI2_GridScript> grids;

    public bool draggingObjectAround = false;
    SSI2_UIItemScript itemDragging;

    [Header("Description Panel")]
    public RectTransform descriptionPanel;
    public Text descHeader;
    public Text descDescription;
    public RectTransform descImageBoundaries;
    public Image descImage;
    public Vector2 descOffset;
    SSI2_UIItemScript previewedItem;

    [Header("Dropping Items")]
    public bool droppingDestroysItems = false;
    [Tooltip("Dropped items will appear at this point")]
    public Transform dropPoint;
    public Vector3 dropVelocityLocal;
    public Vector3 dropVelocityVariance;

    [Header("Dropping Quantities of Items")]
    public GameObject dropQuantityPanel;
    RectTransform dropQuantityRect;
    public  Slider quantitySlider;
    public InputField quantityText;

    void Awake()
    {
        globalVars = GetComponent<SSI2_GlobalVariables>();
        dropQuantityRect = dropQuantityPanel.GetComponent<RectTransform>();
        canv = GetComponent<Canvas>();
        mainCam = Camera.main;
        foreach (SSI2_GridScript grid in grids)
            grid.handler = this;
        //foreach (SSI2_GridScript grid in grids)
            //grid.GetComponent<RectTransform>().localScale *= gridScale;
    }

    private void Update()
    {
        //canv.enabled = inventoryOpen;
        Cursor.visible = !hideMouseWithInventory || inventoryOpen;
        Cursor.lockState = !(!hideMouseWithInventory || inventoryOpen) ? CursorLockMode.Locked : CursorLockMode.None;
        header.localScale = Vector3.one * System.Convert.ToInt32(inventoryOpen);
        gridPanel.localScale = Vector3.one * gridScale * System.Convert.ToInt32(inventoryOpen);
        dropQuantityRect.localScale = Vector3.one * System.Convert.ToInt32(inventoryOpen);
        if (Input.GetKeyDown(globalVars.inventoryKey)) { 
            inventoryOpen = !inventoryOpen;
            descriptionPanel.gameObject.SetActive(false);
        }
        if(previewedItem != null)
        {
            descriptionPanel.GetComponent<SSI2_DescriptionPanelScript>().equipButtonText.text = previewedItem.equipped ? "Dequip" : "Equip";
        }
    }

    public void PickupItem(Transform itemObj)
    {
        SSI2_ItemScript item = itemObj.GetComponent<SSI2_ItemScript>();
        ParseEvents(addingItemEvents, item, item.quantity);

        bool foundSlot = false; 

        for (int i = 0; i < grids.Count; i++)
        {
            if (grids[i].acceptsItemsOnPickup)
            {
                bool result = grids[i].AddItem(item);
                if (result)
                {
                    ParseEvents(itemAddedEvents, item, item.quantity);
                    foundSlot = true;
                    break;
                }
            }
        }

        if (!foundSlot)
            ParseEvents(rejectItemEvents, item, item.quantity);
    }

    public void InsertItem(Transform itemObj, int quantity = 1) //Adding items via code
    {
        SSI2_ItemScript item = itemObj.GetComponent<SSI2_ItemScript>();
        item.quantity = quantity;
        ParseEvents(addingItemEvents, item, item.quantity);

        bool foundSlot = false;

        for (int i = 0; i < grids.Count; i++)
        {
            if (grids[i].acceptsItemsOnPickup)
            {
                bool result = grids[i].AddItem(item);
                if (result)
                {
                    ParseEvents(itemAddedEvents, item, item.quantity);
                    foundSlot = true;
                    break;
                }
            }
        }

        if (!foundSlot) //If we can't add this item, we'll drop it
        {
            grids[0].DropItem(item, droppingDestroysItems, true);
            ParseEvents(rejectItemEvents, item, item.quantity);
            ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, item, item.quantity);
        }
    }

    public void ParseEvents(List<InventoryEvent> events, SSI2_ItemScript item, int quantity)
    {
        foreach(InventoryEvent invEvent in events)
        {
            if (invEvent.eventActivated)
            {
                switch ((int)invEvent.dataToSend)
                {
                    case 0:
                        if (invEvent.includeQuantityOfItem)
                        {
                            object[] dataList = new object[2];
                            dataList[0] = invEvent.stringParam;
                            dataList[1] = quantity;
                            invEvent.obj.SendMessage(invEvent.eventMessage, dataList);
                        }
                        else invEvent.obj.SendMessage(invEvent.eventMessage, invEvent.stringParam);
                        break;
                    case 1:
                        if (invEvent.includeQuantityOfItem)
                        {
                            object[] dataList = new object[2];
                            dataList[0] = invEvent.intParam;
                            dataList[1] = quantity;
                            invEvent.obj.SendMessage(invEvent.eventMessage, dataList);
                        }
                        else invEvent.obj.SendMessage(invEvent.eventMessage, invEvent.intParam);
                        break;
                    case 2:
                        if (invEvent.includeQuantityOfItem)
                        {
                            object[] dataList = new object[2];
                            dataList[0] = invEvent.floatParam;
                            dataList[1] = quantity;
                            invEvent.obj.SendMessage(invEvent.eventMessage, dataList);
                        }
                        else invEvent.obj.SendMessage(invEvent.eventMessage, invEvent.floatParam);
                        break;
                    case 3:
                        if (invEvent.includeQuantityOfItem)
                        {
                            object[] dataList = new object[2];
                            dataList[0] = invEvent.boolParam;
                            dataList[1] = quantity;
                            invEvent.obj.SendMessage(invEvent.eventMessage, dataList);
                        }
                        else invEvent.obj.SendMessage(invEvent.eventMessage, invEvent.boolParam);
                        break;
                    case 4:
                        if (invEvent.includeQuantityOfItem)
                        {
                            object[] dataList = new object[2];
                            dataList[0] = item;
                            dataList[1] = quantity;
                            invEvent.obj.SendMessage(invEvent.eventMessage, dataList);
                        }
                        else invEvent.obj.SendMessage(invEvent.eventMessage, item);
                        break;
                }
            }
        }
    }

    public void ParseSpecialEvents(string eventType, object[] data)
    {
        switch (eventType)
        {
            case "overlap":
                Debug.Log("Calling overlaps");
                foreach(uniqueEvent uEvent in overlapEvents)
                {
                    if (uEvent.eventActivated)
                    {
                        //data[0] = Item 1 - Item being dragged
                        //data[1] = Item 2 - Item already in the inventory
                        uEvent.eventObj.SendMessage(uEvent.eventMessage, data, SendMessageOptions.DontRequireReceiver);
                    }
                }
                break;
        }
    }

    public void MovingItem(SSI2_UIItemScript item) {
        descriptionPanel.gameObject.SetActive(false);
        dropQuantityPanel.gameObject.SetActive(false);
        draggingObjectAround = true;
        itemDragging = item;
        ParseEvents(transferingItemEvents, item.GetComponent<SSI2_ItemScript>(), item.assignedItem.quantity);
    }

    public bool StopMovingItem(SSI2_UIItemScript item, bool ignoreConditions = false)
    {
        Vector2 pos; //An item has been moved, time to check their position to see if they fall within any bounds for the grids

        bool foundSpot = false;
        bool withinBounds = false;

        foreach (SSI2_GridScript grid in grids)
        {
            if ((grid.acceptsItemsOnTransfer || ignoreConditions))
            {
                item.GetComponent<RectTransform>().SetParent(grid.rect);
                pos = item.GetComponent<RectTransform>().anchoredPosition;

                if (grid.grid.rectTransform.anchoredPosition.x <= pos.x && pos.x < grid.grid.rectTransform.rect.width + grid.grid.rectTransform.anchoredPosition.x &&
                    grid.grid.rectTransform.anchoredPosition.y >= pos.y && pos.y > -grid.grid.rectTransform.rect.height + grid.grid.rectTransform.anchoredPosition.y)
                {
                    int column = (int)((pos.x - grid.xOffset) / 50);
                    int row = (int)Mathf.Abs((pos.y - grid.yOffset) / 50);

                    withinBounds = true;

                    int oldItemElement = item.inventoryElement;
                    bool fits = grid.MoveItem(item, row, column);

                    if (fits)
                    {
                        ParseEvents(transferedItemEvents, item.assignedItem, item.assignedItem.quantity);
                        item.transform.SetParent(grid.transform);
                        //It fits, now we need to snap it to the position.
                        item.GetComponent<RectTransform>().anchoredPosition = new Vector2(50 * column + grid.xOffset, -50 * row + grid.yOffset);

                        if (item.assignedGrid != grid && item.assignedGrid != null) //Item has been transferred to another grid, make sure it's properly paired up with it
                        {
                            SSI2_GridScript oldGrid = item.assignedGrid;
                            oldGrid.MoveItemAway(item, oldItemElement);
                            item.assignedGrid = grid;
                        }
                        if (item.assignedGrid == null) item.assignedGrid = grid;

                        foundSpot = true;

                        break;
                    }
                }
            }
        }

        if (!withinBounds) //Item is outside of grid bounds, drop it.
        {
            if (item.equipped) ParseEvents(dequipItemEvents, item.assignedItem, item.assignedItem.quantity);

            if (item.assignedGrid != null)
            {
                item.assignedGrid.DropItem(item, droppingDestroysItems, false);
                ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, item.assignedItem, item.assignedItem.quantity);
            }
            else //This item has no grid, call the DropItem in the first grid with the Ignore Inventory override
            {
                grids[0].DropItem(item, droppingDestroysItems, true);
                ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, item.assignedItem, item.assignedItem.quantity);
            }
        }

        if (!foundSpot && withinBounds) //Can't find spot in this grid we've searched, return to original position
        {
            if (item.assignedGrid != null)
            {
                item.transform.SetParent(item.assignedGrid.transform);
                ParseEvents(failTransferEvents, item.assignedItem, item.assignedItem.quantity);
                item.Revert();
            }
            else
            {
                grids[0].DropItem(item, droppingDestroysItems, true);
                ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, item.assignedItem, item.assignedItem.quantity);
            }
            
        }

        StartCoroutine("LateEndDrag");
        return foundSpot && withinBounds;
    }

    public IEnumerator LateEndDrag() //There isn't much use to this function, only so that if you right click to place an object down over another object, you won't start dragging that object
    {
        yield return new WaitForSeconds(0.1f);
        draggingObjectAround = false;
    }

    public void PreviewItem(SSI2_UIItemScript item)
    {
        previewedItem = item;

        //First we figure out the positions and pivots of the description panel.
        descriptionPanel.gameObject.SetActive(true);
        descriptionPanel.transform.SetAsLastSibling();

        descriptionPanel.pivot = new Vector2(0, 1);
        descriptionPanel.position = item.GetComponent<RectTransform>().position;
        descriptionPanel.anchoredPosition += new Vector2(item.GetComponent<RectTransform>().sizeDelta.x + descOffset.x, descOffset.y);
        if (descriptionPanel.anchoredPosition.x + descriptionPanel.rect.width > viewport.rect.width)
        {
            descriptionPanel.pivot = new Vector2(1, 1);
            descriptionPanel.position = item.GetComponent<RectTransform>().position + new Vector3(-descOffset.x, descOffset.y, 0);
        }
        descriptionPanel.anchoredPosition = new Vector2(descriptionPanel.anchoredPosition.x, Mathf.Clamp(descriptionPanel.anchoredPosition.y, 
            -viewport.rect.height - gridHolder.anchoredPosition.y + descriptionPanel.rect.height, -gridHolder.anchoredPosition.y));

        //Now for the visuals :)
        descHeader.text = item.assignedItem.itemName;
        descDescription.text = item.assignedItem.itemDescription;
        descImage.sprite = item.assignedItem.texture;

        RectTransform imageRect = descImage.GetComponent<RectTransform>();
        imageRect.sizeDelta = new Vector2(50 * item.assignedItem.width, 50 * item.assignedItem.height);
        if(imageRect.sizeDelta.x > descImageBoundaries.rect.width)
            imageRect.sizeDelta *= (descImageBoundaries.rect.width / imageRect.sizeDelta.x);
        if (imageRect.sizeDelta.y > descImageBoundaries.rect.height)
            imageRect.sizeDelta *= (descImageBoundaries.rect.height / imageRect.sizeDelta.y);

        //Now we check if we can equip it
        descriptionPanel.GetComponent<SSI2_DescriptionPanelScript>().equipButton.gameObject.SetActive(item.assignedItem.equippable);
        descriptionPanel.GetComponent<SSI2_DescriptionPanelScript>().equipButtonText.text = previewedItem.equipped ? "Dequip" : "Equip";
    }

    public void DropSpecificItem(SSI2_UIItemScript item, bool destroyItem)
    {
        if (item.equipped)
        {
            ParseEvents(dequipItemEvents, item.assignedItem, item.assignedItem.quantity);
            equippedItem = null;
        }

        item.assignedGrid.DropItem(item, destroyItem || droppingDestroysItems);
        ParseEvents((destroyItem || droppingDestroysItems) ? destroyItemEvents : dropItemEvents, item.assignedItem, item.assignedItem.quantity);
    }

    public void DropSpecificItem(SSI2_ItemScript item, bool destroyItem)
    {
        if (item.uiAssignedTo.equipped)
        {
            ParseEvents(dequipItemEvents, item, item.quantity);
            equippedItem = null;
        }

        item.uiAssignedTo.assignedGrid.DropItem(item, destroyItem || droppingDestroysItems);
        ParseEvents((destroyItem || droppingDestroysItems) ? destroyItemEvents : dropItemEvents, item, item.quantity);
    }

    public void DropPreviewedItem()
    {
        if ((previewedItem.assignedItem.quantity == 1 && !previewedItem.assignedItem.canBeEmpty) || previewedItem.assignedItem.dropEntireStack || previewedItem.assignedItem.quantity <= 0)
        {
            if (previewedItem.equipped)
            {
                ParseEvents(dequipItemEvents, previewedItem.assignedItem, previewedItem.assignedItem.quantity);
                equippedItem = null;
            }

            previewedItem.assignedGrid.DropItem(previewedItem, droppingDestroysItems);
            ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, previewedItem.assignedItem, previewedItem.assignedItem.quantity);
        }
        else
        {
            if ((previewedItem.assignedItem.quantity == 1 && previewedItem.assignedItem.canBeEmpty))
            {
                quantitySlider.SetValueWithoutNotify(1);
                DropQuantity();
            }
            else
            {
                descriptionPanel.gameObject.SetActive(false);
                dropQuantityPanel.gameObject.SetActive(true);
                quantitySlider.SetValueWithoutNotify(1);
                quantityText.SetTextWithoutNotify("1");
                quantitySlider.maxValue = previewedItem.assignedItem.quantity;
            }
        }
    }

    public void DropEquippedItem(bool destroyItems)
    {
        ParseEvents(dequipItemEvents, equippedItem.assignedItem, equippedItem.assignedItem.quantity);
        previewedItem.assignedGrid.DropItem(equippedItem, droppingDestroysItems);
        ParseEvents((destroyItems || droppingDestroysItems) ? destroyItemEvents : dropItemEvents, previewedItem.assignedItem, previewedItem.assignedItem.quantity);
        equippedItem = null;
    }

    public void DropQuantity()
    {
        if (previewedItem.equipped && (int)quantitySlider.value == previewedItem.assignedItem.quantity && !previewedItem.assignedItem.canBeEmpty)
        {
            ParseEvents(dequipItemEvents, previewedItem.assignedItem, previewedItem.assignedItem.quantity);
            equippedItem = null;
        }
        int quantityDropped = 0;
        previewedItem.assignedGrid.DropItemMultiple(previewedItem, (int)quantitySlider.value, droppingDestroysItems, out quantityDropped);
        ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, previewedItem.assignedItem, quantityDropped);
    }

    public void SplitQuantity()
    {
        int quantityDropped = 0;
        previewedItem.assignedGrid.DropItemMultiple(previewedItem, (int)quantitySlider.value, true, out quantityDropped);
        previewedItem.assignedGrid.CreateSplitPrefab(previewedItem, (int)quantitySlider.value);
    }

    public void DestroyPreviewedItem()
    {
        if (previewedItem.equipped)
        {
            ParseEvents(dequipItemEvents, previewedItem.assignedItem, previewedItem.assignedItem.quantity);
            equippedItem = null;
        }

        previewedItem.assignedGrid.DropItem(previewedItem, true);
        ParseEvents(destroyItemEvents, previewedItem.assignedItem, previewedItem.assignedItem.quantity);
    }

    public void EquipPreviewedItem()
    {
        if(equippedItem != null && equippedItem != previewedItem) //In case another item has been equipped
        {
            ParseEvents(dequipItemEvents, equippedItem.assignedItem, equippedItem.assignedItem.quantity);
            equippedItem.equipped = false;
            equippedItem = null;
        }

        previewedItem.equipped = !previewedItem.equipped;
        ParseEvents(previewedItem.equipped ? equipItemEvents : dequipItemEvents, previewedItem.assignedItem, previewedItem.assignedItem.quantity);
        equippedItem = previewedItem;
    }

    public void EquipItem(SSI2_UIItemScript uiItem)
    {
        if (equippedItem != null && equippedItem != previewedItem) //In case another item has been equipped
        {
            ParseEvents(dequipItemEvents, equippedItem.assignedItem, equippedItem.assignedItem.quantity);
            equippedItem.equipped = false;
            equippedItem = null;
        }

        uiItem.equipped = !uiItem.equipped;
        ParseEvents(uiItem.equipped ? equipItemEvents : dequipItemEvents, uiItem.assignedItem, uiItem.assignedItem.quantity);
        equippedItem = uiItem;
    }

    public void SnatchItem(SSI2_ItemScript item) //Plucks this item from it's current grid, then adds it to the first available spot
    {
        ParseEvents(transferingItemEvents, item, item.quantity);
        item.uiAssignedTo.assignedGrid.DropItem(item, false, false, false);
        bool foundSlot = false;

        for (int i = 0; i < grids.Count; i++)
        {
            if (grids[i].acceptsItemsOnTransfer)
            {
                bool result = grids[i].AddItem(item);
                if (result)
                {
                    ParseEvents(transferedItemEvents, item, item.quantity);
                    foundSlot = true;
                    break;
                }
            }
        }

        if (!foundSlot) //If we can't add this item, we'll drop it
        {
            grids[0].DropItem(item, droppingDestroysItems, true);
            ParseEvents(failTransferEvents, item, item.quantity);
            ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, item, item.quantity);
        }
    }

    public void DequipItem()
    {
        ParseEvents(dequipItemEvents, equippedItem.assignedItem, equippedItem.assignedItem.quantity);
    }

    public void DropAllItems()
    {
        foreach (SSI2_GridScript grid in grids)
        {
            List<SSI2_UIItemScript> results = grid.DropAllItems();
            if (results.Count != 0)
            {
                foreach (SSI2_UIItemScript result in results)
                {
                    if (result.equipped) ParseEvents(dequipItemEvents, result.assignedItem, result.assignedItem.quantity);

                    grid.DropItem(result, droppingDestroysItems);
                    ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, result.assignedItem, result.assignedItem.quantity);
                }
            }
        }
    }

    //# DROPPING, FETCHING SPECIFIC ITEMS
    public void DropItemsOfType(string tag, int quantity, bool ignoreStacks, bool destroyItems) //Dropping [quantity] amounts of a specific item, including/excluding stackables, 0 = drop all
    {
        foreach (SSI2_GridScript grid in grids)
        {
            List<SSI2_UIItemScript> itemResults;
            List<int> itemQuantities;
            grid.FetchItemsOfType(tag, quantity, ignoreStacks, out itemResults, out itemQuantities);
            if (itemResults.Count != 0)
            {
                for(int i = 0; i < itemResults.Count; i++)
                {
                    if (itemResults[i].equipped && itemResults[i].assignedItem.quantity == itemQuantities[i]) ParseEvents(dequipItemEvents, itemResults[i].assignedItem, itemResults[i].assignedItem.quantity);
                    if (itemResults[i] == previewedItem) descriptionPanel.gameObject.SetActive(false);

                    int quantityDropped = itemResults[i].assignedItem.quantity;

                    if (itemResults[i].assignedItem.quantity == itemQuantities[i] || ignoreStacks) grid.DropItem(itemResults[i], destroyItems || droppingDestroysItems);
                    else grid.DropItemMultiple(itemResults[i], itemQuantities[i], destroyItems || droppingDestroysItems, out quantityDropped);

                    ParseEvents(destroyItems || droppingDestroysItems ? destroyItemEvents : dropItemEvents, 
                        itemResults[i].assignedItem, quantityDropped);
                }
            }
        }
    }

    public void FetchItemsOfType(string tag, int quantity, bool ignoreStacks, out List<SSI2_UIItemScript> items, out List<int> quantities)
    {
        items = new List<SSI2_UIItemScript>();
        quantities = new List<int>();
        foreach (SSI2_GridScript grid in grids)
        {
            List<SSI2_UIItemScript> itemResults;
            List<int> itemQuantities;
            grid.FetchItemsOfType(tag, quantity, ignoreStacks, out itemResults, out itemQuantities);

            items.AddRange(itemResults);
            quantities.AddRange(itemQuantities);
        }
    }

    public void DropItemsOfName(string name, int quantity, bool ignoreStacks, bool destroyItems)
    {
        foreach (SSI2_GridScript grid in grids)
        {
            List<SSI2_UIItemScript> itemResults;
            List<int> itemQuantities;
            grid.FetchItemsOfName(name, quantity, ignoreStacks, out itemResults, out itemQuantities);
            if (itemResults.Count != 0)
            {
                for (int i = 0; i < itemResults.Count; i++)
                {
                    if (itemResults[i].equipped && itemResults[i].assignedItem.quantity == itemQuantities[i]) ParseEvents(dequipItemEvents, itemResults[i].assignedItem, itemResults[i].assignedItem.quantity);
                    if (itemResults[i] == previewedItem) descriptionPanel.gameObject.SetActive(false);

                    int quantityDropped = itemResults[i].assignedItem.quantity;

                    if (itemResults[i].assignedItem.quantity == itemQuantities[i] || ignoreStacks) grid.DropItem(itemResults[i], destroyItems || droppingDestroysItems);
                    else grid.DropItemMultiple(itemResults[i], itemQuantities[i], destroyItems || droppingDestroysItems, out quantityDropped);

                    ParseEvents(destroyItems || droppingDestroysItems ? destroyItemEvents : dropItemEvents,
                        itemResults[i].assignedItem, quantityDropped);

                }
            }
        }
    }

    public void FetchItemsOfName(string name, int quantity, bool ignoreStacks, out List<SSI2_UIItemScript> items, out List<int> quantities)
    {
        items = new List<SSI2_UIItemScript>();
        quantities = new List<int>();
        foreach (SSI2_GridScript grid in grids)
        {
            List<SSI2_UIItemScript> itemResults;
            List<int> itemQuantities;
            grid.FetchItemsOfName(name, quantity, ignoreStacks, out itemResults, out itemQuantities);

            items.AddRange(itemResults);
            quantities.AddRange(itemQuantities);
        }
    }

    //# CREATING, REMOVING, UPDATING GRIDS
    public void CreateGrid(SSI2_GridProperties gridToAdd, bool createdFromItem)
    {
        RectTransform newGrid = Instantiate(gridPrefab, gridHolder);
        newGrid.localScale = Vector3.one;

        SSI2_GridScript newGridScript = newGrid.GetComponent<SSI2_GridScript>();
        newGridScript.columnCount = gridToAdd.columnCount;
        newGridScript.rowCount = gridToAdd.rowCount;
        newGridScript.expandingGrid = gridToAdd.expanding;
        newGridScript.gridName = gridToAdd.gridName;
        newGridScript.gridRemoveable = gridToAdd.removeable;
        newGridScript.acceptedItems = gridToAdd.acceptedItems;

        newGridScript.handler = this;

        grids.Add(newGridScript);

        newGrid.gameObject.SetActive(true);

        if (createdFromItem)
        {
            SSI2_ItemScript item = gridToAdd.GetComponent<SSI2_ItemScript>();
            DropSpecificItem(item.uiAssignedTo, false);
            item.gameObject.SetActive(false);
            item.transform.SetParent(newGrid);

            newGridScript.assignedItem = item;
        }
    }

    public void RemoveGrid(SSI2_GridScript gridToRemove)
    {
        List<SSI2_UIItemScript> results = gridToRemove.DropAllItems();
        if (results.Count != 0)
        {
            foreach (SSI2_UIItemScript result in results)
            {
                if (result.equipped) ParseEvents(dequipItemEvents, result.assignedItem, result.assignedItem.quantity);

                gridToRemove.DropItem(result, droppingDestroysItems);
                ParseEvents(droppingDestroysItems ? destroyItemEvents : dropItemEvents, result.assignedItem, result.assignedItem.quantity);
            }
        }
        grids.Remove(gridToRemove);

        //If the grid has an assigned object, drop it
        if(gridToRemove.assignedItem != null)
        {
            Vector3 oldScale = gridToRemove.assignedItem.transform.localScale;
            gridToRemove.assignedItem.transform.SetParent(null);
            gridToRemove.assignedItem.gameObject.SetActive(true);
            gridToRemove.assignedItem.transform.position = dropPoint.position;
            gridToRemove.assignedItem.transform.localScale = oldScale;
            gridToRemove.assignedItem.GetComponent<Rigidbody>().velocity = dropPoint.TransformDirection(dropVelocityLocal +
                new Vector3(UnityEngine.Random.Range(-dropVelocityVariance.x, dropVelocityVariance.x),
                    UnityEngine.Random.Range(-dropVelocityVariance.y, dropVelocityVariance.y),
                    UnityEngine.Random.Range(-dropVelocityVariance.z, dropVelocityVariance.z)));
        }

        Destroy(gridToRemove.gameObject);
        UpdateBounds();
    }

    public void RemoveRandomGrid()
    {
        RemoveGrid(grids[UnityEngine.Random.Range(0, grids.Count)]);
    }

    public void UpdateBounds()
    {
        foreach(SSI2_GridScript grid in grids)
        {
            grid.CalculateBounds();
        }
        Debug.Log("Bounds have been updated");
        if (gridHolder.GetComponent<ContentSizeFitter>().enabled)
        {
            gridHolder.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            gridHolder.GetComponent<ContentSizeFitter>().enabled = false;
            gridHolder.GetComponent<ContentSizeFitter>().enabled = true;
        }
    }

    public void QuantitySliderChange()
    {
        quantityText.SetTextWithoutNotify(quantitySlider.value.ToString());
    }

    public void QuantityTextChange()
    {
        quantityText.SetTextWithoutNotify(Mathf.Clamp(Int32.Parse(quantityText.text), 1, previewedItem.assignedItem.maxStack).ToString());
        quantitySlider.SetValueWithoutNotify(Int32.Parse(quantityText.text));
    }
}
