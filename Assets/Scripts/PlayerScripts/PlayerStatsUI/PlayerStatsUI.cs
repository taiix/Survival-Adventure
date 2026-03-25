using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private VisualElement staminaFill;
    private PlayerController playerController;

    void Start()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument not found on PlayerStatsUI GameObject");
            return;
        }

        var root = uiDocument.rootVisualElement;
        staminaFill = root.Q<VisualElement>("StaminaFill");

        if (staminaFill == null)
        {
            Debug.LogError("StaminaFill element not found in UI");
            return;
        }

        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in scene");
            return;
        }
    }

    void Update()
    {
        if (playerController == null || staminaFill == null)
        {
            return;
        }

        float staminaPercent = playerController.GetStaminaController().CurrentStamina / 
                               playerController.GetStaminaController().MaxStamina;
        
        staminaFill.style.height = new Length(staminaPercent * 100f, LengthUnit.Percent);
    }
}
