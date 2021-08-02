using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public class SSI2_GridScript : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public SSI2_UIItemScript item;
        public int row;
        public int column;
        public bool rotated = false;
    }

    public List<Item> inventory;

    public SSI2_ItemScript assignedItem;

    public SSI2_InventoryScript handler;
    public SSI2_UIItemScript inventoryObjPrefab;

    public RectTransform rect;
    RectTransform gridRect;

    [Header("Grid Information")]
    public string gridName;
    public bool gridRemoveable;
    

    public int columnCount;
    public int rowCount;

    public List<string> acceptedItems;
    public List<string> rejectedItems;

    public bool[][] inventorySlots;
    int freeSpaces;

    [Header("UI")]
    public Image grid;
    public Text nameText;
    public Button removeButton;

    [Header("Grid Properties")]
    [Tooltip("When an item is picked up, does the AddItem function include this grid?")]
    public bool acceptsItemsOnPickup = true;
    [Tooltip("Can I drag items into this grid?")]
    public bool acceptsItemsOnTransfer = true;

    [Header("Grid Expansion")]
    [Tooltip("Setting this to true allows a grid to expand should an item not fit")]
    public bool expandingGrid = false;
    [Tooltip("The ratio of width to height when expanding, so we expand horizontally until we are X times the height or more, then expand the height, keep this small just to be safe")]
    public float targetWidthToHeight = 2;

    [Header("Grid Boundaries")]
    public int xOffset = 15;
    public int yOffset = -75;
    public int yGap = 100;
    public int removeButtonXOffset = 15;
    public int removeButtonYOffset = 0;

    void Awake()
    {
        inventorySlots = new bool[rowCount][];
        for (int i = 0; i < rowCount; i++)
        {
            inventorySlots[i] = new bool[columnCount];
        }
    }

    void Start()
    {
        rect = transform.GetComponent<RectTransform>();
        gridRect = grid.GetComponent<RectTransform>();

        gridRect.sizeDelta = new Vector2(50 * columnCount, 50 * rowCount);
        rect.sizeDelta = new Vector2(xOffset + 50 * columnCount, -yOffset + yGap + 50 * rowCount);

        freeSpaces = rowCount * columnCount;

        

        nameText.text = gridName;

        StartCoroutine(LateStart(0.01f));
    }

    IEnumerator LateStart(float time) //Make sure the vertical layout group has sorted everything, then figure out the bounds
    {
        yield return new WaitForSeconds(time);
        removeButton.gameObject.SetActive(gridRemoveable);
        removeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset + removeButtonXOffset + nameText.rectTransform.sizeDelta.x,
            removeButtonYOffset + nameText.GetComponent<RectTransform>().anchoredPosition.y);
        CalculateBounds();
    }

    public bool AddItem(SSI2_ItemScript item, bool ignoreConditions = false)
    {
        if ((((acceptedItems.Contains(item.itemTag) || acceptedItems.Contains(item.itemName)) || acceptedItems.Count == 0) 
            && !rejectedItems.Contains(item.itemTag) && !rejectedItems.Contains(item.itemName) || ignoreConditions)){
            int remainingQuantity = item.quantity;
            //First we're gonna check if we can stack the item with another
            if ((item.maxStack > 1 || item.maxStack == -1) && inventory.Find(x => x.item.assignedItem.itemName == item.itemName && x.item.assignedItem.maxStack != x.item.assignedItem.quantity) != null)
            {
                List<Item> itemsToStack = inventory.FindAll(x => x.item.assignedItem.itemName == item.itemName && x.item.assignedItem.maxStack != x.item.assignedItem.quantity);
                if (itemsToStack != null)
                {
                    foreach (Item itemToStack in itemsToStack)
                    {
                        int oldQuantity = itemToStack.item.assignedItem.quantity;
                        Debug.Log($"Old quantity: {oldQuantity}, Remaining: {remainingQuantity}");

                        itemToStack.item.assignedItem.quantity = Mathf.Clamp(oldQuantity + remainingQuantity, 0, 
                            itemToStack.item.assignedItem.maxStack != -1 ? itemToStack.item.assignedItem.maxStack : int.MaxValue);
                        itemToStack.item.quantityText.text = $"{itemToStack.item.assignedItem.quantity}";

                        if(item.maxStack != -1)remainingQuantity = (int)Mathf.Clamp(remainingQuantity - (itemToStack.item.assignedItem.maxStack - oldQuantity), 0, Mathf.Infinity);
                        else remainingQuantity = 0;

                        //AddToPrefab(itemToStack.item, item);
                        if(remainingQuantity <= 0)
                        {
                            break;
                        }
                    }
                }
            }

            //If not, we'll try and create its own slot
            if (remainingQuantity > 0 || (remainingQuantity == 0 && item.canBeEmpty && item.maxStack != -1))
            {
                Debug.Log($"Gonna add the rest now.{remainingQuantity}");
                int suitableInt; //0 = fits, 1 = doesn't fit, 2 = grid has been expanded, check again
                suitableInt = CheckSpot(false, item, false, remainingQuantity); //First check if the item fits regularly
                if (suitableInt == 1)
                {
                    if (!expandingGrid) suitableInt = CheckSpot(true, item, false, remainingQuantity); //Now check to see if it fits while rotated, unless we're an expanding grid
                    else
                    { //Instead, if we do expand, add the amount of rows and work from there
                        if (columnCount / rowCount >= targetWidthToHeight)
                        {
                            ExpandGrid(item.height, (int)Mathf.Clamp(item.width - columnCount, 0, Mathf.Infinity));
                        }
                        else
                        {
                            ExpandGrid((int)Mathf.Clamp(item.height - columnCount, 0, Mathf.Infinity), item.width);
                        }
                        suitableInt = CheckSpot(false, item, true, remainingQuantity);
                    }
                }
                else if (suitableInt == 2)
                    suitableInt = CheckSpot(false, item, true, remainingQuantity); //Check if it fits after an expansion, which it should

                return suitableInt == 0 ? true : false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public int CheckSpot(bool rotationState, SSI2_ItemScript item, bool expanded, int quantity) //We're returning integers now because there's 3 states this can be, fits, doesn't fit or expanded
    {
        int actualWidth = rotationState ? item.height : item.width;
        int actualHeight = rotationState ? item.width : item.height;

        int currentRowCount = rowCount;
        int currentColumnCount = columnCount; //prevents infinite expansions

        int suitable = 0;
        bool resettingDueToExpansions = false;

        if (actualHeight > currentRowCount || actualWidth > currentColumnCount || freeSpaces < actualWidth * actualHeight) //item is too big for the grid or there aren't enough slots
            if (!expandingGrid) return 1;

        for(int row = 0; row < currentRowCount; row++)
        {
            for(int column = 0; column < currentColumnCount; column++)
            {
                if (inventorySlots[row][column] || //the slot is taken
                    (actualWidth > 1 && (column + actualWidth - 1) > inventorySlots[row].Length - 1) || //we're at the edge and it won't fit there
                    (actualHeight > 1 && (row + actualHeight - 1) > inventorySlots.Length - 1) //we're at the bottom and the item won't fit there
                    )
                {
                    if (!expandingGrid)
                    {
                        suitable = 1;
                        continue;
                    }
                    else
                    {
                        if (!inventorySlots[row][column] && !expanded)
                        {
                            int rowsToAdd = row + actualHeight - currentRowCount;
                            int colsToAdd = column + actualWidth - currentColumnCount;

                            //Make sure the space we want to expand is still suitable (ie no objects will be blocking the item before/after the expansion)
                            bool checkSpace = CheckSpecificSpot(row, column, actualWidth, actualHeight); 
                            if (checkSpace)
                            {
                                bool foo = ExpandGrid((int)Mathf.Clamp(rowsToAdd, 0, Mathf.Infinity), (int)Mathf.Clamp(colsToAdd, 0, Mathf.Infinity));

                                row = currentRowCount;
                                column = currentColumnCount; //Cancel this early
                                resettingDueToExpansions = true; //time to do this check again with the new expanded grid
                                break;
                            }
                            else
                            {
                                suitable = 1;
                                continue;
                            }
                        }
                        else
                        {
                            suitable = 1;
                            continue;
                        }
                    }
                } else //we've found a potentially valid spot for the item to go
                {
                    bool suitableSpot = true;
                    for(int rowOffset = 0; rowOffset < actualHeight; rowOffset++)
                    {
                        for(int columnOffset = 0; columnOffset < actualWidth; columnOffset++)
                        {
                            if(inventorySlots[row + rowOffset][column + columnOffset])
                            {
                                suitableSpot = false;
                                break;
                            }
                        }
                    }

                    suitable = System.Convert.ToInt32(!suitableSpot);
                    if (suitable == 0) //we claiming this spot bois
                    {
                        for (int rowOffset = 0; rowOffset < actualHeight; rowOffset++)
                        {
                            for (int columnOffset = 0; columnOffset < actualWidth; columnOffset++)
                            {
                                inventorySlots[row + rowOffset][column + columnOffset] = true;
                            }
                        }
                        //now create the UI prefab, or adjust the existing one (if it exists)
                        CreatePrefab(item, row, column, rotationState, quantity, item.uiAssignedTo != null);

                        freeSpaces -= item.width * item.height;
                        return 0;
                    }
                }
            }
        }

        return resettingDueToExpansions ? 2 : suitable;
    }

    public bool CheckSpecificSpot(int row, int column, int width, int height)
    {
        for (int rowOffset = 0; rowOffset < height; rowOffset++)
        {
            if (row + rowOffset < rowCount)
            {
                for (int columnOffset = 0; columnOffset < width; columnOffset++)
                    if (column + columnOffset < columnCount)
                        if (inventorySlots[row + rowOffset][column + columnOffset]) return false;
            }
        }
        return true;
    }

    public bool MoveItem(SSI2_UIItemScript item, int row, int column)
    {
        if ((((acceptedItems.Contains(item.assignedItem.itemTag) || acceptedItems.Contains(item.assignedItem.itemName)) || acceptedItems.Count == 0)
            && !rejectedItems.Contains(item.assignedItem.itemTag) && !rejectedItems.Contains(item.assignedItem.itemName)))
        {
            int remainingQuantity = item.assignedItem.quantity;
            bool sendOverlap = false;
            object[] objs = new object[2];
            //First, we check if we're atop an item, then send out an overlap event at the end of the function (to avoid errors)
            if (inventorySlots[row][column])
            {
                foreach (Item itemToCheck in inventory)
                {
                    if (itemToCheck.item != item)
                    {
                        int actualWidth = !itemToCheck.item.rotated ? itemToCheck.item.assignedItem.width : itemToCheck.item.assignedItem.height;
                        int actualHeight = !itemToCheck.item.rotated ? itemToCheck.item.assignedItem.height : itemToCheck.item.assignedItem.width;

                        if (itemToCheck.column <= column && column < (itemToCheck.column + actualWidth) &&
                            itemToCheck.row <= row && row < (itemToCheck.row + actualHeight))
                        {
                            //Okay!
                            objs = new object[2];
                            objs[0] = item.assignedItem;
                            objs[1] = itemToCheck.item.assignedItem;
                            sendOverlap = true;
                            

                            break;
                        }
                    }
                }
            }
            //Second, we check if we're atop the same item type and if we can stack it.
            if (item.assignedItem.maxStack > 1 &&
                inventory.FindAll(x => x.item.assignedItem.itemName == item.assignedItem.itemName && x.item.assignedItem.maxStack != x.item.assignedItem.quantity) != null)
            {
                List<Item> items = inventory.FindAll(x => x.item.assignedItem.itemName == item.assignedItem.itemName
                && x.item.assignedItem.maxStack != x.item.assignedItem.quantity &&
                x.item != item);
                foreach (Item itemToCheck in items)
                {
                    int actualWidth = !itemToCheck.item.rotated ? itemToCheck.item.assignedItem.width : itemToCheck.item.assignedItem.height;
                    int actualHeight = !itemToCheck.item.rotated ? itemToCheck.item.assignedItem.height : itemToCheck.item.assignedItem.width;

                    if (itemToCheck.column <= column && column < (itemToCheck.column + actualWidth) &&
                        itemToCheck.row <= row && row < (itemToCheck.row + actualHeight))
                    {
                        //Okay!
                        int oldQuantity = itemToCheck.item.assignedItem.quantity;

                        itemToCheck.item.assignedItem.quantity = Mathf.Clamp(oldQuantity + remainingQuantity, 0,
                            itemToCheck.item.assignedItem.maxStack != -1 ? itemToCheck.item.assignedItem.maxStack : int.MaxValue);
                        itemToCheck.item.quantityText.text = $"{itemToCheck.item.assignedItem.quantity}";

                        remainingQuantity = (int)Mathf.Clamp(remainingQuantity - (itemToCheck.item.assignedItem.maxStack - oldQuantity), 0, Mathf.Infinity);

                        if (remainingQuantity == 0 && !item.assignedItem.canBeEmpty)
                        {
                            if (inventory.Find(x => x.item == item) != null)
                            {
                                DropItem(item, true);
                            }
                            Destroy(item.gameObject);
                        }
                        else
                        {
                            //Item is now empty, return it to it's original position.
                            item.assignedItem.quantity = 0;
                            item.quantityText.text = $"{item.assignedItem.quantity}";
                            return false;
                        }
                        break;
                    }
                }
            }

            if (remainingQuantity > 0) //Otherwise, continue as normal
            {
                int oldRow = 0;
                int oldColumn = 0;

                int actualWidth = item.rotated ? item.assignedItem.height : item.assignedItem.width;
                int actualHeight = item.rotated ? item.assignedItem.width : item.assignedItem.height;

                int originalWidth = 0; //if we're rotating an item inside the same grid, these'll be needed, otherwise ignore
                int originalHeight = 0;

                if (expandingGrid)
                { //If we need to expand, add the required amount of rows and columns and work from there
                    Debug.Log("We've got to expand.");
                    ExpandGrid((int)Mathf.Clamp(actualHeight - rowCount, 0, Mathf.Infinity), (int)Mathf.Clamp(actualWidth - columnCount, 0, Mathf.Infinity));
                }

                if (item.assignedGrid == this)
                {
                    oldRow = inventory[item.inventoryElement].row;
                    oldColumn = inventory[item.inventoryElement].column;

                    originalWidth = inventory[item.inventoryElement].rotated ? item.assignedItem.height : item.assignedItem.width;
                    originalHeight = inventory[item.inventoryElement].rotated ? item.assignedItem.width : item.assignedItem.height;

                    for (int rowOffset = 0; rowOffset < originalHeight; rowOffset++)
                    {
                        for (int columnOffset = 0; columnOffset < originalWidth; columnOffset++)
                        {
                            inventorySlots[oldRow + rowOffset][oldColumn + columnOffset] = false; //Set all of these to false in case we're just moving the item one slot to the side or something.
                        }
                    }
                }

                bool suitable = true; //now we're checking the space we want to check

                if (row + actualHeight - 1 > rowCount - 1 || column + actualWidth - 1 > columnCount - 1)
                { //to avoid unnecessary iteration we'll see if this item doesn't fit in this new spot first
                    suitable = false;
                }

                if (suitable)
                {
                    for (int rowOffset = 0; rowOffset < actualHeight; rowOffset++)
                    {
                        for (int columnOffset = 0; columnOffset < actualWidth; columnOffset++)
                        {
                            if (inventorySlots[row + rowOffset][column + columnOffset])
                            {
                                suitable = false;
                                break;
                            }
                        }
                    }
                }

                if (suitable || item.assignedGrid == this) //Make either the new spot or old spot true again
                {
                    int widthToUse = suitable ? actualWidth : originalWidth;
                    int heightToUse = suitable ? actualHeight : originalHeight;

                    for (int columnOffset = 0; columnOffset < widthToUse; columnOffset++)
                    {
                        for (int rowOffset = 0; rowOffset < heightToUse; rowOffset++)
                        {
                            inventorySlots[(suitable ? row : oldRow) + rowOffset][(suitable ? column : oldColumn) + columnOffset] = true;
                        }
                    }
                }

                if (suitable && item.assignedGrid == this)
                {
                    inventory[item.inventoryElement].row = row;
                    inventory[item.inventoryElement].column = column;
                    inventory[item.inventoryElement].rotated = item.rotated;
                }
                else if (item.assignedGrid != this && suitable)
                {
                    Debug.Log("An item has been moved here.");
                    item.inventoryElement = inventory.Count;
                    var newItem = new Item();
                    inventory.Add(newItem);
                    newItem.row = row;
                    newItem.column = column;
                    newItem.item = item;
                    newItem.rotated = item.rotated;
                }
                if(sendOverlap) handler.ParseSpecialEvents("overlap", objs);
                return suitable;
            }
            else
            {
                if (sendOverlap) handler.ParseSpecialEvents("overlap", objs);
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public void MoveItemAway(SSI2_UIItemScript item, int element)
    {
        int row = inventory[element].row;
        int column = inventory[element].column;

        int actualWidth = inventory[element].rotated ? item.assignedItem.height : item.assignedItem.width;
        int actualHeight = inventory[element].rotated ? item.assignedItem.width : item.assignedItem.height;

        freeSpaces += item.assignedItem.width * item.assignedItem.height;

        for (int columnOffset = 0; columnOffset < actualWidth; columnOffset++)
        {
            for (int rowOffset = 0; rowOffset < actualHeight; rowOffset++)
            {
                inventorySlots[row + rowOffset][column + columnOffset] = false; //Set all of these to false in case we're just moving the item one slot to the side or something.
            }
        }
        inventory.RemoveAt(element);

        foreach (Item remainingItem in inventory)
        {
            remainingItem.item.inventoryElement = inventory.IndexOf(remainingItem);
        }
    }

    public void DropItem(SSI2_UIItemScript item, bool destroyItem = false, bool ignoreInventory = false, bool destroyUI = true)
    {
        //This function is used when an item in this grid is removed, or transferred to another grid.
        if (!ignoreInventory)
        {
            int row = inventory[item.inventoryElement].row;
            int column = inventory[item.inventoryElement].column;

            int actualWidth = inventory[item.inventoryElement].rotated ? item.assignedItem.height : item.assignedItem.width;
            int actualHeight = inventory[item.inventoryElement].rotated ? item.assignedItem.width : item.assignedItem.height;

            for (int columnOffset = 0; columnOffset < actualWidth; columnOffset++)
            {
                for (int rowOffset = 0; rowOffset < actualHeight; rowOffset++)
                {
                    inventorySlots[row + rowOffset][column + columnOffset] = false;
                }
            }

            for (int i = item.inventoryElement + 1; i < inventory.Count; i++)
            {
                inventory[i].item.inventoryElement -= 1;
            }

            freeSpaces += item.assignedItem.width * item.assignedItem.height;

            inventory.RemoveAt(item.inventoryElement);
        }
        if (!destroyItem)
        {
            Debug.Log("Not destroying :)");
            Vector3 oldScale = item.assignedItem.transform.localScale;
            item.assignedItem.transform.SetParent(null);
            item.assignedItem.gameObject.SetActive(true);
            item.assignedItem.transform.position = handler.dropPoint.position;
            item.assignedItem.transform.localScale = oldScale;
            item.assignedItem.GetComponent<Rigidbody>().velocity = handler.dropPoint.TransformDirection(handler.dropVelocityLocal +
                new Vector3(UnityEngine.Random.Range(-handler.dropVelocityVariance.x, handler.dropVelocityVariance.x),
                    UnityEngine.Random.Range(-handler.dropVelocityVariance.y, handler.dropVelocityVariance.y),
                    UnityEngine.Random.Range(-handler.dropVelocityVariance.z, handler.dropVelocityVariance.z)));
        }
        else Destroy(item.assignedItem.gameObject);

        if(destroyUI) Destroy(item.gameObject);
    }

    public void DropItem(SSI2_ItemScript item, bool destroyItem, bool ignoreInventory = false, bool destroyUI = true)
    {
        //This function is used when an item in this grid is removed, or transferred to another grid.
        if (!ignoreInventory)
        {
            int row = inventory[item.uiAssignedTo.inventoryElement].row;
            int column = inventory[item.uiAssignedTo.inventoryElement].column;

            int actualWidth = inventory[item.uiAssignedTo.inventoryElement].rotated ? item.height : item.width;
            int actualHeight = inventory[item.uiAssignedTo.inventoryElement].rotated ? item.width : item.height;

            for (int columnOffset = 0; columnOffset < actualWidth; columnOffset++)
            {
                for (int rowOffset = 0; rowOffset < actualHeight; rowOffset++)
                {
                    inventorySlots[row + rowOffset][column + columnOffset] = false;
                }
            }

            for (int i = item.uiAssignedTo.inventoryElement + 1; i < inventory.Count; i++)
            {
                inventory[i].item.inventoryElement -= 1;
            }

            freeSpaces += item.width * item.height;

            inventory.RemoveAt(item.uiAssignedTo.inventoryElement);
        }
        if (!destroyItem)
        {
            Debug.Log("Not destroying :)");
            Vector3 oldScale = item.transform.localScale;
            item.transform.SetParent(null);
            item.gameObject.SetActive(true);
            item.transform.position = handler.dropPoint.position;
            item.transform.localScale = oldScale;
            item.GetComponent<Rigidbody>().velocity = handler.dropPoint.TransformDirection(handler.dropVelocityLocal +
                new Vector3(UnityEngine.Random.Range(-handler.dropVelocityVariance.x, handler.dropVelocityVariance.x),
                    UnityEngine.Random.Range(-handler.dropVelocityVariance.y, handler.dropVelocityVariance.y),
                    UnityEngine.Random.Range(-handler.dropVelocityVariance.z, handler.dropVelocityVariance.z)));
        }
        else Destroy(item.gameObject);

        if (destroyUI) Destroy(item.uiAssignedTo.gameObject);
    }

    public void DropItemMultiple(SSI2_UIItemScript item, int quantity, bool destroyItem, out int quantitiesDestroyed)
    {
        quantitiesDestroyed = Mathf.Clamp(quantity, 0, item.assignedItem.quantity);
        
        item.assignedItem.quantity -= quantity;
        item.quantityText.text = item.assignedItem.quantity.ToString();
        

        if (item.assignedItem.quantity <= 0 && !item.assignedItem.canBeEmpty)
        {
            int row = inventory[item.inventoryElement].row;
            int column = inventory[item.inventoryElement].column;

            int actualWidth = inventory[item.inventoryElement].rotated ? item.assignedItem.height : item.assignedItem.width;
            int actualHeight = inventory[item.inventoryElement].rotated ? item.assignedItem.width : item.assignedItem.height;

            for (int columnOffset = 0; columnOffset < actualWidth; columnOffset++)
            {
                for (int rowOffset = 0; rowOffset < actualHeight; rowOffset++)
                {
                    inventorySlots[row + rowOffset][column + columnOffset] = false;
                }
            }

            for (int i = item.inventoryElement + 1; i < inventory.Count; i++)
            {
                inventory[i].item.inventoryElement -= 1;
            }

            freeSpaces += item.assignedItem.width * item.assignedItem.height;

            inventory.RemoveAt(item.inventoryElement);

            if (!destroyItem)
            {
                Vector3 oldScale = item.assignedItem.transform.localScale;
                item.assignedItem.transform.SetParent(null);
                item.assignedItem.gameObject.SetActive(true);
                item.assignedItem.quantity = quantity;
                item.assignedItem.transform.position = handler.dropPoint.position;
                item.assignedItem.transform.localScale = oldScale;
                item.assignedItem.GetComponent<Rigidbody>().velocity = handler.dropPoint.TransformDirection(handler.dropVelocityLocal +
                    new Vector3(UnityEngine.Random.Range(-handler.dropVelocityVariance.x, handler.dropVelocityVariance.x),
                        UnityEngine.Random.Range(-handler.dropVelocityVariance.y, handler.dropVelocityVariance.y),
                        UnityEngine.Random.Range(-handler.dropVelocityVariance.z, handler.dropVelocityVariance.z)));
            }
            else Destroy(item.assignedItem.gameObject);

            Destroy(item.gameObject);
        }
        else
        {
            if (!destroyItem)
            {
                Transform prefab = Instantiate(item.assignedItem.transform, null);
                prefab.localScale = item.assignedItem.transform.localScale;
                prefab.gameObject.SetActive(true);
                prefab.position = handler.dropPoint.position;
                prefab.GetComponent<SSI2_ItemScript>().quantity = quantity;
                prefab.GetComponent<Rigidbody>().velocity = handler.dropPoint.TransformDirection(handler.dropVelocityLocal +
                    new Vector3(UnityEngine.Random.Range(-handler.dropVelocityVariance.x, handler.dropVelocityVariance.x),
                        UnityEngine.Random.Range(-handler.dropVelocityVariance.y, handler.dropVelocityVariance.y),
                        UnityEngine.Random.Range(-handler.dropVelocityVariance.z, handler.dropVelocityVariance.z)));
            }
        }
    }

    public List<SSI2_UIItemScript> DropAllItems()
    {
        List<SSI2_UIItemScript> droppedItems = new List<SSI2_UIItemScript>();
        foreach (Item item in inventory)
        {
            droppedItems.Add(item.item);
        }
        return droppedItems;
    }

    public void FetchItemsOfType(string itemTag, int quantityToFetch, bool ignoreStacks, out List<SSI2_UIItemScript> foundItems, out List<int> itemQuantities)
    {
        int remainingQuantity = quantityToFetch;
        foundItems = new List<SSI2_UIItemScript>();
        itemQuantities = new List<int>();
        foreach (Item item in inventory)
        {
            if (item.item.assignedItem.itemTag == itemTag)
            {
                foundItems.Add(item.item);
                itemQuantities.Add(Mathf.Clamp(item.item.assignedItem.quantity, 0, remainingQuantity));
                remainingQuantity -= ignoreStacks ? 1 : item.item.assignedItem.quantity;
                if (remainingQuantity <= 0 && quantityToFetch != 0) break;
            }
        }
    }

    public void FetchItemsOfName(string itemName, int quantityToFetch, bool ignoreStacks, out List<SSI2_UIItemScript> foundItems, out List<int> itemQuantities)
    {
        int remainingQuantity = quantityToFetch;
        foundItems = new List<SSI2_UIItemScript>();
        itemQuantities = new List<int>();
        foreach (Item item in inventory)
        {
            if (item.item.assignedItem.itemName == itemName)
            {
                foundItems.Add(item.item);
                itemQuantities.Add(Mathf.Clamp(item.item.assignedItem.quantity, 0, remainingQuantity));
                remainingQuantity -= ignoreStacks ? 1 : item.item.assignedItem.quantity;
                if (remainingQuantity <= 0 && quantityToFetch != 0) break;
            }
        }
    }

    public bool SnatchItem(SSI2_ItemScript item) //We're going to take this item from it's original grid and put it in this one.
    {
        item.uiAssignedTo.GetComponent<RectTransform>().SetParent(transform);
        item.uiAssignedTo.GetComponent<RectTransform>().anchoredPosition = grid.GetComponent<RectTransform>().anchoredPosition + new Vector2(1, -1);
        bool result = handler.StopMovingItem(item.uiAssignedTo, true);
        return result;
    }

    public void SnatchFrom() //Give the first item in this grid's inventory back.
    {
        if (inventory.Count > 0)
        {
            handler.SnatchItem(inventory[0].item.assignedItem);
        }
    }

    public bool ExpandGrid(int rows, int columns)
    {
        int takenSpaces = rowCount * columnCount - freeSpaces;

        rowCount += rows;
        columnCount += columns;

        for(int existingRow = 0; existingRow < inventorySlots.Length; existingRow++)
        {
            bool[] oldRow = inventorySlots[existingRow];
            bool[] newRow = new bool[columnCount];
            oldRow.CopyTo(newRow, 0);
            inventorySlots[existingRow] = newRow;
        }

        if(rows > 0) //Because arrays are faffy, we have to redefine the inventorySlots variable
        {
            bool[][] oldInv = inventorySlots;
            bool[][] newInv = new bool[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                newInv[i] = new bool[columnCount];
                for(int j = 0; j < columnCount; j++)
                {
                    newInv[i][j] = false;
                }
            }

            oldInv.CopyTo(newInv, 0);
            inventorySlots = newInv;
        }

        freeSpaces = rowCount * columnCount - takenSpaces;

        gridRect.sizeDelta = new Vector2(50 * columnCount, 50 * rowCount);
        rect.sizeDelta = new Vector2(xOffset + 50 * columnCount, -yOffset + yGap + 50 * rowCount);

        handler.UpdateBounds();
        return true;
    }

    public void RemoveGrid()
    {
        handler.RemoveGrid(this);
    }

    public void CreatePrefab(SSI2_ItemScript item, int row, int column, bool rotationState, int quantity, bool prefabExists = false) {
        SSI2_UIItemScript prefab;
        if (!prefabExists)
        {
            prefab = Instantiate(inventoryObjPrefab);
            item.transform.SetParent(prefab.transform, true);
            prefab.inventoryHandler = handler;
            prefab.assignedItem = item;
            prefab.transform.name = item.name;
            prefab.inventoryElement = inventory.Count;
            prefab.textureObj.sprite = item.texture;
            prefab.assignedItem.quantity = quantity;
        }
        else
        {
            prefab = item.uiAssignedTo;
        }
        prefab.assignedGrid = this;
        prefab.rotationState = rotationState ? 1 : 0;
        prefab.rotated = rotationState;
        

        prefab.quantityText.text = quantity.ToString();
        prefab.quantityText.gameObject.SetActive(item.maxStack == 1 ? false : true);

        
        
        item.transform.localPosition = Vector3.zero;
        item.gameObject.SetActive(false);
        item.uiAssignedTo = prefab;

        var newItem = new Item();
        newItem.item = prefab;
        newItem.row = row;
        newItem.column = column;
        newItem.rotated = rotationState;
        inventory.Add(newItem);

        RectTransform prefabRect = prefab.GetComponent<RectTransform>();

        prefab.transform.SetParent(transform);
        prefabRect.localScale = Vector3.one;
        prefabRect.localRotation = Quaternion.Euler(0, 0, 90 * prefab.rotationState);
        prefabRect.pivot = new Vector2((prefab.rotationState % 3) == 0 ? 0 : 1, prefab.rotationState <= 1 ? 1 : 0);

        prefabRect.anchoredPosition = new Vector2(xOffset + 50 * column, yOffset - 50 * row);
        prefabRect.sizeDelta = new Vector2(50 * item.width, 50 * item.height);
        prefab.gameObject.SetActive(true);
    }

    public void CreateSplitPrefab(SSI2_UIItemScript itemToCopy, int quantity)
    {
        SSI2_UIItemScript prefab = Instantiate(inventoryObjPrefab, handler.transform);
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();

        prefab.transform.name = itemToCopy.assignedItem.name;
        prefab.inventoryHandler = handler;
        prefab.assignedGrid = null;
        prefab.assignedItem = Instantiate(itemToCopy.assignedItem, prefab.transform);
        prefab.inventoryElement = 0;
        prefab.rotationState = 0;
        prefab.rotated = false;
        prefab.assignedItem.quantity = quantity;
        prefab.quantityText.text = quantity.ToString();
        prefab.textureObj.sprite = itemToCopy.assignedItem.texture;

        prefab.assignedItem.uiAssignedTo = prefab;

        prefabRect.localScale = Vector3.one;
        prefabRect.localRotation = Quaternion.Euler(0, 0, 90 * prefab.rotationState);
        prefabRect.pivot = new Vector2((prefab.rotationState % 3) == 0 ? 0 : 1, prefab.rotationState <= 1 ? 1 : 0);

        prefabRect.sizeDelta = new Vector2(50 * itemToCopy.assignedItem.width, 50 * itemToCopy.assignedItem.height);
        
        prefab.gameObject.SetActive(true);
        prefab.DragMe();
    }

    public void AddToPrefab(SSI2_UIItemScript uiObj, SSI2_ItemScript item)
    {
        /*item.transform.SetParent(uiObj.transform);
        item.transform.localPosition = Vector3.zero;
        item.gameObject.SetActive(false);*/
    }

    public void CalculateBounds()
    {
        Debug.Log("Foo.");
    }
}
