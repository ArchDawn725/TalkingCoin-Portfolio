using UnityEngine;
[CreateAssetMenu(menuName = "Scenes/SceneData")]
public class SceneSO : ScriptableObject
{
    [SerializeField] private CharacterSO[] charactersToSpawn;
    [SerializeField] private LLMController.GPTType[] characterRoles;
}
