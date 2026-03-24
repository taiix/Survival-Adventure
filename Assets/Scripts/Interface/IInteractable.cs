public interface IInteractable
{
    string DisplayName { get; }
    bool CanInteract { get; }
    void OnInteract();
}
