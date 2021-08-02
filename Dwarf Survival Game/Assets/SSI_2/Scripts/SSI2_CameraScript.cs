using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSI2_CameraScript : MonoBehaviour
{
    public Transform playerCharacter;
    public _FirstPersonController controllerScript;
    public SSI2_InventoryScript inventoryHandler;
    public RectTransform canv;

    [Header("Direct Item Interaction")]
    [Tooltip("The object that raycasts will come from, will set itself to this object if unassigned")]
    public Transform raycastPoint;
    public float raycastLength = 2f;
    public float descriptionRaycastLength = 3f;
    public LayerMask raycastLayerMask;
    public Text promptText;

    /*[Header("Nearby Item Interaction")]
    [Tooltip("The number to subtract from the player's y-position when checking for nearby items on the floor, " +
        "set to -1 if calculating automatically from Capsule Collider or Character Controller")]
    public float sphereCastOffset = -1f;
    public float nearbyItemRadius = 3f;
    public LayerMask nearbyItemLayerMask;
    [Tooltip("If false, ignore walls when searching for nearby objects")]
    public bool nearbyChecksIgnoreObstructions = false;*/

    [Header("Description Display")]
    public bool showDescriptionOnRaycast = true;
    public RectTransform descriptionPanel;
    public Text descPanelHeader;
    public Text descPanelDescription;
    public Image descPanelImage;
    public RectTransform descPanelBounds;

    void Awake()
    {
        controllerScript = playerCharacter.GetComponent<_FirstPersonController>();
        if (raycastPoint == null)
            raycastPoint = transform;
        /*if(sphereCastOffset == -1)
        {
            if (playerCharacter.GetComponent<CapsuleCollider>())
                sphereCastOffset = playerCharacter.GetComponent<CapsuleCollider>().height / 2;
            else if(playerCharacter.GetComponent<CharacterController>())
                sphereCastOffset = playerCharacter.GetComponent<CharacterController>().height / 2;
        }*/
    }

    void Update()
    {
        controllerScript.mouseLookEnabled = !inventoryHandler.inventoryOpen;
        RaycastHit hit;
        RaycastHit descPanelHit;
        if (Physics.Raycast(raycastPoint.position, raycastPoint.TransformDirection(Vector3.forward), out hit, raycastLength, raycastLayerMask))
        {
            promptText.gameObject.SetActive(true);
            if (Input.GetKeyDown(inventoryHandler.globalVars.useKey))
            {
                inventoryHandler.PickupItem(hit.transform);
            }
        }
        else
        {
            promptText.gameObject.SetActive(false);
            
        }
        if (Physics.Raycast(raycastPoint.position, raycastPoint.TransformDirection(Vector3.forward), out descPanelHit, descriptionRaycastLength, raycastLayerMask) && showDescriptionOnRaycast)
            DrawDescriptionPanel(descPanelHit.transform);
        else descriptionPanel.gameObject.SetActive(false);
    }

    public void DrawDescriptionPanel(Transform itemObj)
    {
        descriptionPanel.gameObject.SetActive(true);

        SSI2_ItemScript item = itemObj.GetComponent<SSI2_ItemScript>();

        Vector2 viewport = Camera.main.WorldToViewportPoint(itemObj.position);
        descriptionPanel.anchoredPosition = new Vector2(viewport.x * canv.rect.width, Mathf.Clamp(viewport.y * canv.rect.height, descriptionPanel.rect.height, canv.rect.height));

        //Now for the visuals :)
        descPanelHeader.text = $"{item.itemName}" + (item.maxStack > 1 ? ($" (x{item.quantity})") : (""));
        descPanelDescription.text = item.itemDescription + $"\nWidth: {item.width}\nHeight: {item.height}\nType: {item.itemTag}";
        descPanelImage.sprite = item.texture;

        RectTransform imageRect = descPanelImage.GetComponent<RectTransform>();
        imageRect.sizeDelta = new Vector2(50 * item.width, 50 * item.height);
        if (imageRect.sizeDelta.x > descPanelBounds.rect.width)
            imageRect.sizeDelta *= (descPanelBounds.rect.width / imageRect.sizeDelta.x);
        if (imageRect.sizeDelta.y > descPanelBounds.rect.height)
            imageRect.sizeDelta *= (descPanelBounds.rect.height / imageRect.sizeDelta.y);
    }
}
