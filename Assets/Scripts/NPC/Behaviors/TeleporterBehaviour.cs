using UnityEngine;

public class TeleporterBehaviour : MonoBehaviour, INPCBehaviour
{
    [SerializeField] private GameObject teleporterUI;

    public GameObject GetUIPanel()
    {
        return teleporterUI;
    }

    public void OnInteract()
    {
        teleporterUI.SetActive(true);
    }

    public void OnInteractionEnd()
    {
        teleporterUI.SetActive(false);
    }

    public void OnInteractionUpdate()
    {
    }
}
