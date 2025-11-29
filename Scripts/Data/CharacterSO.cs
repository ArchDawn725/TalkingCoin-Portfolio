using UnityEngine;

[CreateAssetMenu(menuName = "Character/CharacterData")]
public class CharacterSO : ScriptableObject
{
    public string voiceID;
    public string occupation;
    public string personality;
    public int maxGold;
    public Vector3 mouthPos;
    public Vector3 lipsPos;
    public int itemNumber;
    public int helmNumber;
    public bool royalty;
    public bool isImportant;
}
