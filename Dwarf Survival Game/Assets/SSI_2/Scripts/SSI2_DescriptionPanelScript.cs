using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSI2_DescriptionPanelScript : MonoBehaviour
{
    public bool allowsItemDestruction = false;

    public RectTransform panel;
    public RectTransform equipButton;
    public Text equipButtonText;
    public RectTransform dropButton;
    public RectTransform destroyButton;

    public int buttonYOffset = 15;
    // Start is called before the first frame update
    void Start()
    {
        if (allowsItemDestruction)
        {
            destroyButton.gameObject.SetActive(true);
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, panel.sizeDelta.y + 75);
            dropButton.anchoredPosition = new Vector2(dropButton.anchoredPosition.x, dropButton.anchoredPosition.y + destroyButton.sizeDelta.y + buttonYOffset);
            equipButton.anchoredPosition = new Vector2(equipButton.anchoredPosition.x, equipButton.anchoredPosition.y + destroyButton.sizeDelta.y + buttonYOffset);
        }
        else
        {
            destroyButton.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
