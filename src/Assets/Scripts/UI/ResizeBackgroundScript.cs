using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResizeBackgroundScript : MonoBehaviour
{
    private GameObject Background;
    private RectTransform rectTransformBG;
    void Start()
    {
        Background = GameObject.Find("Background");
        rectTransformBG = Background.GetComponent<RectTransform>();
    }
    void Update()
    {
        // Size of the ViewPort Screen
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        // Adjust the Background to the size of the Screen
        rectTransformBG.sizeDelta = screenSize;
    }

}
