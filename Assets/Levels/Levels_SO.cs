using UnityEngine;
public enum Bioms
{
    None,
    Forest,
    Desert,
    Snow,
    Swamp,
    Mountain
}

[CreateAssetMenu(fileName = "Levels_SO", menuName = "Scriptable Objects/Levels")]
public class Levels_SO : ScriptableObject
{
    public Bioms biome;
    public bool isLocked;
    public int level;
}
