using UnityEngine;
using UnityEngine.Events;

public class NPC_Interaction : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName;

    private INPCBehaviour npcBehavior;
    private GameObject uiPanel;
    private bool isInteracting;

    public string DisplayName => npcName;
    public bool CanInteract => enabled && gameObject.activeSelf;

    public event UnityAction OnInteractionStarted;
    public event UnityAction OnInteractionEnded;

    private void OnEnable()
    {
        if (npcBehavior == null && GetComponent<INPCBehaviour>() == null)
        {
            Debug.LogWarning($"NPC_Interaction ({npcName}): No INPCBehaviour component found on {gameObject.name}!");
        }
    }

    private void Start()
    {
        InitializeBehavior();
    }

    private void InitializeBehavior()
    {
        npcBehavior = GetComponent<INPCBehaviour>();

        if (npcBehavior == null)
        {
            Debug.LogError($"NPC_Interaction ({npcName}): Failed to initialize behavior!");
            Debug.LogError($"  GameObject: {gameObject.name}");
            Debug.LogError($"  Path: {GetGameObjectPath()}");
            Debug.LogError($"  → Add a component that implements INPCBehaviour (e.g., BlacksmithBehavior)");
            return;
        }

        uiPanel = npcBehavior.GetUIPanel();

        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"NPC_Interaction ({npcName}): No UI panel provided by behavior");
        }
    }

    private void Update()
    {
        if (isInteracting)
        {
            npcBehavior?.OnInteractionUpdate();
        }
    }

    public void OnInteract()
    {
        if (npcBehavior == null)
        {
            Debug.LogWarning($"NPC_Interaction ({npcName}): Cannot interact - behavior is null!");
            return;
        }

        isInteracting = true;
        npcBehavior.OnInteract();
        ShowUI();

        OnInteractionStarted?.Invoke();
    }

    public void EndInteraction()
    {
        if (!isInteracting)
            return;

        isInteracting = false;
        npcBehavior?.OnInteractionEnd();
        HideUI();

        OnInteractionEnded?.Invoke();
    }

    private void ShowUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }
    }

    private void HideUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    public GameObject GetAssociatedUI() => uiPanel;
    public INPCBehaviour GetBehavior() => npcBehavior;
    public bool IsInteracting() => isInteracting;

    private void OnDisable()
    {
        if (isInteracting)
        {
            EndInteraction();
        }
    }

    private string GetGameObjectPath()
    {
        string path = gameObject.name;
        Transform parent = gameObject.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
