using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [Header("Available Characters")]
    public CharacterData[] availableCharacters;
    
    // UI Elements can be hooked up via UIManager or directly
    // This script should be attached to the CharacterSelectionPanel or an empty GameObject

    // Called from Unity UI Buttons in Character Selection Panel
    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= availableCharacters.Length)
        {
            Debug.LogError("Character index out of bounds!");
            return;
        }

        CharacterData selectedData = availableCharacters[index];
        Debug.Log("Selected Character: " + selectedData.characterName);

        // Tell UIManager to setup the HUD based on this character
        UIManager.Instance.SetupHUD(selectedData.characterIcon, selectedData.startingHealth);

        // Tell GameManager to Start Game with this prefab
        GameManager.Instance.StartGameWithCharacter(selectedData.characterPrefab);
    }
}
