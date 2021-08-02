using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SSI2_UIItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("General")]
    public Image textureObj;
    public SSI2_ItemScript assignedItem;
    public SSI2_GridScript assignedGrid;
    RectTransform rect;
    Image image;
    public SSI2_InventoryScript inventoryHandler;
    Canvas inventoryCanv;
    RectTransform inventoryCanvRect;
    public Text quantityText;

    bool hoveringOver = false;
    bool dragButtonUp = false;

    public bool dragging;
    Vector2 oldPos;
    int oldRotState = 0;

    public int inventoryElement = 0;

    [Header("Rotation")]
    public bool rotated = false;
    public SSI2_ItemScript.rotDegs degreesOfRotation;
    public int rotationState = 0;

    [Header("Other Stats")]
    public bool equipped = false;
    bool started = false;
    public bool destroyThis = false;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        StartCoroutine(LateStart(0.01f));
    }

    IEnumerator LateStart(float time) //Make sure the vertical layout group has sorted everything, then figure out the bounds
    {
        yield return new WaitForSeconds(time);
        
        inventoryCanv = inventoryHandler.GetComponent<Canvas>();
        inventoryCanvRect = inventoryCanv.GetComponent<RectTransform>();
        degreesOfRotation = assignedItem.degreesOfRotation;
        oldPos = rect.anchoredPosition;
        oldRotState = rotationState;
        started = true;
    }

    void Update()
    {
        if (hoveringOver)
        {
            if (Input.GetKeyDown(inventoryHandler.globalVars.previewKey))
            {
                if (!inventoryHandler.draggingObjectAround) inventoryHandler.PreviewItem(this);
            }
            if(Input.GetKeyDown(inventoryHandler.globalVars.dragKey))
            {
                if (!inventoryHandler.draggingObjectAround)
                {
                    oldPos = rect.anchoredPosition;
                    oldRotState = rotationState;
                    dragging = !dragging;
                    inventoryHandler.MovingItem(this);
                    dragButtonUp = false;
                }
                //Right click, drag
            }
        }
        if (Input.GetKeyUp(inventoryHandler.globalVars.dragKey))
        {
            dragButtonUp = true;
        }
        if (dragging && started && dragButtonUp)
        {
            rect.SetParent(inventoryHandler.transform);
            rect.anchoredPosition = new Vector2(Camera.main.ScreenToViewportPoint(Input.mousePosition).x * inventoryCanvRect.rect.width + 25, 
                Camera.main.ScreenToViewportPoint(Input.mousePosition).y * inventoryCanvRect.rect.height - inventoryCanvRect.rect.height - 25);
            image.raycastTarget = false;

            if (Input.GetKeyDown(inventoryHandler.globalVars.dragKey)){
                dragging = false;
                Debug.Log("NO LONGER DRAGGING");

                rect.anchoredPosition = new Vector2(Camera.main.ScreenToViewportPoint(Input.mousePosition).x * inventoryCanvRect.rect.width,
                    Camera.main.ScreenToViewportPoint(Input.mousePosition).y * inventoryCanvRect.rect.height - inventoryCanvRect.rect.height);

                image.raycastTarget = true;
                rect.SetParent(inventoryHandler.gridHolder);
                inventoryHandler.StopMovingItem(this);
            }

            if (Input.GetKeyDown(inventoryHandler.globalVars.rotateItemKey))
            {
                rotated = !rotated;

                rotationState = (rotationState + 1) % (degreesOfRotation == SSI2_ItemScript.rotDegs.Four ? 4 : 2);
                rect.localRotation = Quaternion.Euler(0, 0, 90 * rotationState);

                rect.pivot = new Vector2((rotationState % 3) == 0 ? 0 : 1, rotationState <= 1 ? 1 : 0);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoveringOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoveringOver = false;
    }

    public void DragMe()
    {
        Debug.Log("Time to get dragged");
        if (rect == null) rect = GetComponent<RectTransform>();
        oldPos = rect.anchoredPosition;
        oldRotState = rotationState;
        dragging = true;
        inventoryHandler.MovingItem(this);
        dragButtonUp = true;
    }
    public void Revert()
    {
        Debug.Log("Reverting");
        rect.anchoredPosition = oldPos;
        rotationState = oldRotState;

        rotated = rotationState % 2 == 0 ? false : true;
        rect.localRotation = Quaternion.Euler(0, 0, 90 * rotationState);

        rect.pivot = new Vector2((rotationState % 3) == 0 ? 0 : 1, rotationState <= 1 ? 1 : 0);
    }
}
