using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Showcase_Crafter : MonoBehaviour
{
    //This script is designed to showcase the overlap events, using a simple crafting system.

    public enum recipeType {NewItem = 0, FillItem = 1}
    [System.Serializable]
    public class Recipe {
        public string recipeName;
        public recipeType type;

        public string ingredient1Name;
        public int ingredient1Quantity = 1;
        public bool consumesIngredient1 = true;
        public string ingredient2Name;
        public int ingredient2Quantity = 1;
        public bool consumesIngredient2 = true;

        public int amountProduced = 1;
        public SSI2_ItemScript output;
    }

    public List<Recipe> recipes;
    public SSI2_InventoryScript handler;
    
    void Craft(object[] ingredients)
    {
        SSI2_ItemScript item1 = ingredients[0] as SSI2_ItemScript;
        SSI2_ItemScript item2 = ingredients[1] as SSI2_ItemScript;
        Debug.Log("Crafting");
        foreach(Recipe recipe in recipes)
        {
            if ((item1.itemName == recipe.ingredient1Name && item2.itemName == recipe.ingredient2Name) || (item2.itemName == recipe.ingredient1Name && item1.itemName == recipe.ingredient2Name))
            {
                //True = Item 1 -> Ing 1, Item 2 -> Ing 2. False = Item 1 -> Ing 2, Item 2 -> Ing 1
                bool swapped = !(item1.itemName == recipe.ingredient1Name && item2.itemName == recipe.ingredient2Name) ? true : false; 
                Debug.Log("Bingo, valid recipe.");
                int amount;

                if (recipe.type == recipeType.NewItem) // We need to create a new item here
                {
                    amount = !swapped ? (int)Mathf.Min(recipe.consumesIngredient1 ? Mathf.Floor(item1.quantity / recipe.ingredient1Quantity) : recipe.output.maxStack,
                        recipe.consumesIngredient2 ? Mathf.Floor(item2.quantity / recipe.ingredient2Quantity) : recipe.output.maxStack) :
                        (int)Mathf.Min(recipe.consumesIngredient2 ? Mathf.Floor(item1.quantity / recipe.ingredient2Quantity) : recipe.output.maxStack,
                        recipe.consumesIngredient1 ? Mathf.Floor(item2.quantity / recipe.ingredient1Quantity) : recipe.output.maxStack);

                    item1.quantity -= !swapped ? recipe.ingredient1Quantity : recipe.ingredient2Quantity
                        * amount * System.Convert.ToInt32(recipe.consumesIngredient1);
                    
                    item2.quantity -= !swapped ? recipe.ingredient2Quantity : recipe.ingredient1Quantity
                        * amount * System.Convert.ToInt32(recipe.consumesIngredient2);

                    item1.uiAssignedTo.quantityText.text = $"{item1.quantity}";
                    item2.uiAssignedTo.quantityText.text = $"{item2.quantity}";

                    if (item1.quantity == 0 && !item1.canBeEmpty)
                    {
                        item1.uiAssignedTo.destroyThis = true;
                        handler.DropSpecificItem(item1, true);
                    }
                    if (item2.quantity == 0 && !item2.canBeEmpty)
                    {
                        item2.uiAssignedTo.destroyThis = true;
                        handler.DropSpecificItem(item2, true);
                    }

                    handler.InsertItem(Instantiate(recipe.output.transform), amount * recipe.amountProduced);
                } else if(recipe.type == recipeType.FillItem) // Here, we're going to add to an existing item
                {
                    // True, we're adding to item 1. False, we're adding to item 2.
                    // This is probably quite a janky way of going around it, using the canBeEmpty function, but it serves it's purpose for the sample scene and can be modified.
                    SSI2_ItemScript itemAddingTo = item1.canBeEmpty ? item1 : item2;
                    SSI2_ItemScript itemRemovingFrom = item1.canBeEmpty ? item2 : item1;

                    // True, consume from ingredient 1, false, consume from ingredient 2
                    int consumeWhichItem = itemRemovingFrom.name == recipe.ingredient1Name ? recipe.ingredient1Quantity : recipe.ingredient2Quantity;

                    amount = (int)Mathf.Clamp((itemRemovingFrom.quantity / consumeWhichItem), 
                        0, Mathf.Floor((itemAddingTo.maxStack - itemAddingTo.quantity) / recipe.amountProduced));

                    itemRemovingFrom.quantity -= amount * consumeWhichItem;
                    itemRemovingFrom.uiAssignedTo.quantityText.text = $"{itemRemovingFrom.quantity}";
                    itemAddingTo.quantity += amount * recipe.amountProduced;
                    itemAddingTo.uiAssignedTo.quantityText.text = $"{itemAddingTo.quantity}";

                    if (item1.quantity == 0 && !item1.canBeEmpty)
                    {
                        item1.uiAssignedTo.destroyThis = true;
                        handler.DropSpecificItem(item1, true);
                    }
                    if (item2.quantity == 0 && !item2.canBeEmpty)
                    {
                        item2.uiAssignedTo.destroyThis = true;
                        handler.DropSpecificItem(item2, true);
                    }
                }
            }
        }
    }
}
