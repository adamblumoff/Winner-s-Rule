using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SimpleTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Settings")]
    public string tooltipText;
    public GameObject tooltipPrefab;
    public Vector2 offset = new Vector2(10, 10);
    
    [Header("Auto-Create Tooltip")]
    public bool autoCreateTooltip = true;
    public Font tooltipFont;
    public int fontSize = 14;
    public Color backgroundColor = Color.black;
    public Color textColor = Color.white;
    
    private GameObject tooltipInstance;
    private Canvas parentCanvas;
    
    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindFirstObjectByType<Canvas>();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
    
    void ShowTooltip()
    {
        if (string.IsNullOrEmpty(tooltipText) || parentCanvas == null) return;
        
        // Create tooltip if it doesn't exist
        if (tooltipInstance == null)
        {
            if (tooltipPrefab != null)
            {
                tooltipInstance = Instantiate(tooltipPrefab, parentCanvas.transform);
            }
            else if (autoCreateTooltip)
            {
                CreateAutoTooltip();
            }
        }
        
        if (tooltipInstance != null)
        {
            // Set text
            TMP_Text tooltipTextComponent = tooltipInstance.GetComponentInChildren<TMP_Text>();
            if (tooltipTextComponent != null)
            {
                tooltipTextComponent.text = tooltipText;
            }
            
            // Position tooltip
            RectTransform tooltipRect = tooltipInstance.GetComponent<RectTransform>();
            if (tooltipRect != null)
            {
                Vector2 mousePos = Input.mousePosition;
                tooltipRect.position = mousePos + offset;
            }
            
            tooltipInstance.SetActive(true);
        }
    }
    
    void HideTooltip()
    {
        if (tooltipInstance != null)
        {
            tooltipInstance.SetActive(false);
        }
    }
    
    void CreateAutoTooltip()
    {
        // Create a simple tooltip GameObject
        tooltipInstance = new GameObject("Tooltip");
        tooltipInstance.transform.SetParent(parentCanvas.transform);
        
        // Add Image background
        Image bg = tooltipInstance.AddComponent<Image>();
        bg.color = backgroundColor;
        
        // Add Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(tooltipInstance.transform);
        
        TMP_Text text = textObj.AddComponent<TMP_Text>();
        text.text = tooltipText;
        text.color = textColor;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        
        // Setup RectTransforms
        RectTransform tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        
        tooltipRect.sizeDelta = new Vector2(200, 50);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add Content Size Fitter
        ContentSizeFitter fitter = tooltipInstance.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        tooltipInstance.SetActive(false);
    }
    
    void OnDestroy()
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
        }
    }
}