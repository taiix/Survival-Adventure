using UnityEngine;

public static class ServiceLocator
{
    private static IPlayerStatsService playerStatsService;
    private static IUpgradeService upgradeService;
    private static IInputService inputService;

    public static void RegisterPlayerStatsService(IPlayerStatsService service)
    {
        if (playerStatsService != null)
        {
            Debug.LogWarning("ServiceLocator: PlayerStatsService already registered!");
            return;
        }
        playerStatsService = service;
    }

    public static IPlayerStatsService GetPlayerStatsService()
    {
        if (playerStatsService == null)
        {
            Debug.LogError("ServiceLocator: PlayerStatsService not registered!\n" +
                "Make sure PlayerStats is in the scene and initialized.");
        }
        return playerStatsService;
    }

    public static bool IsPlayerStatsServiceAvailable() => playerStatsService != null;

    public static void RegisterUpgradeService(IUpgradeService service)
    {
        if (upgradeService != null)
        {
            Debug.LogWarning("ServiceLocator: UpgradeService already registered!");
            return;
        }
        upgradeService = service;
    }

    public static IUpgradeService GetUpgradeService()
    {
        if (upgradeService == null)
        {
            Debug.LogError("ServiceLocator: UpgradeService not registered!\n" +
                "Make sure UpgradeManager is in the scene and initialized.");
        }
        return upgradeService;
    }

    public static bool IsUpgradeServiceAvailable() => upgradeService != null;

    public static void RegisterInputService(IInputService service)
    {
        if (inputService != null)
        {
            Debug.LogWarning("ServiceLocator: InputService already registered!");
            return;
        }
        inputService = service;
    }

    public static IInputService GetInputService()
    {
        if (inputService == null)
        {
            Debug.LogError("ServiceLocator: InputService not registered!\n" +
                "Make sure InputService is in the scene and initialized.");
        }
        return inputService;
    }

    public static bool IsInputServiceAvailable() => inputService != null;

    public static void UnregisterAll()
    {
        playerStatsService = null;
        upgradeService = null;
        inputService = null;
    }

    public static void UnregisterInputService()
    {
        inputService = null;
    }
}