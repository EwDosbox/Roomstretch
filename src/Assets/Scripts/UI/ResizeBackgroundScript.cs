using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResizeBackgroundScript : MonoBehaviour
{
    private RectTransform rectTransformBG;

    void Start()
    {
        rectTransformBG = gameObject.GetComponent<RectTransform>();
    }
    void Update()
    {
        // Size of the ViewPort Screen
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        // Adjust the background to the size of the Screen
        rectTransformBG.sizeDelta = screenSize;
    }

}
