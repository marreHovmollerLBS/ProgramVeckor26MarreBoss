using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrades/Base Upgrade")
public class Upgrade : ScriptableObject
{
    public virtual void ApplyUpgrade(Character character)
    {

    }
}
