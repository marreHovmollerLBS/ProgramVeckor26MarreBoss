using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrades/Base Upgrade")]
public class Upgrade : ScriptableObject
{
    [Header("UI Display")]
    public Sprite icon;
    public string title;
    [TextArea(3, 5)]
    public string description;

    public virtual void ApplyUpgrade(Character character)
    {   
    }
}
