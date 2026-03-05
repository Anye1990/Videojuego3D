using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "ShooterGame/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public GameObject characterPrefab;
    public Sprite characterIcon;
    public int startingHealth = 100;
    // You can add more stats here later like movement speed, default weapon, etc.
}
