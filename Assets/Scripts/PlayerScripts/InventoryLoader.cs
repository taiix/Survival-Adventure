using UnityEngine;
using UnityEngine.UIElements;

public class InventoryLoader : MonoBehaviour
{
    public UIDocument inventoryUIDocument;
    public Texture2D itemTexture;
    
    void OnEnable()
    {
        if (inventoryUIDocument == null)
        {
            Debug.LogError("inventoryUIDocument is null");
            return;
        }

        var root = inventoryUIDocument.rootVisualElement;
        
        // Try to find Item directly anywhere in the tree
        var item = root.Q<Image>("Item");
        if (item != null)
        {
            Debug.Log("Item found directly in tree - setting texture");
            item.image = itemTexture;
        }
        else
        {
            Debug.LogError("Item not found anywhere in the tree");
            
            // Debug: Print entire hierarchy
            PrintHierarchy(root, 0);
        }
    }

    private void PrintHierarchy(VisualElement element, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}{element.name} ({element.GetType().Name})");
        
        foreach (var child in element.Children())
        {
            PrintHierarchy(child, depth + 1);
        }
    }

    void Update()
    {
        
    }
}
