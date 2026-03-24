using UnityEngine;

public class NPC_Interaction : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName;
    [SerializeField] private GameObject associatedUI;

    public string DisplayName => npcName;

    public bool CanInteract => enabled && gameObject.activeSelf;

    private void Start()
    {
        if (associatedUI != null)
        {
            associatedUI.SetActive(false);
        }
    }

    public void OnInteract()
    {
        Debug.Log($"You interacted with {npcName}!");
        if (associatedUI != null)
        {
            associatedUI.SetActive(!associatedUI.activeSelf);
        }
    }

    public GameObject GetAssociatedUI() => associatedUI;
}
