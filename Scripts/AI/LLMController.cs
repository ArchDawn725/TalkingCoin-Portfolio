using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static LLMController;

public class LLMController : MonoBehaviour
{
    public enum GPTType
    {
        Trader,
        Bystander,
        Judge,
        Guard
    }
    public GPTType Type;

    private GPT gpt;
    [SerializeField] private TextMeshPro responseText;
    public TTSCon voiceController {  get; private set; }
    public CharacterAnimationController animationController { get; private set; }
    public CharacterBodyController bodyController { get; private set; }

    public bool leaving;
    public bool AskIfSilent;
    public string ActionString;

    public void StartUp(GPTData data, GPTType gptType)
    {
        voiceController = GetComponent<TTSCon>();
        animationController = GetComponent<CharacterAnimationController>();
        bodyController = GetComponent<CharacterBodyController>();

        switch (gptType)
        {
            case GPTType.Trader: gpt = new GPTTrader(data.isSeller, data.Selleritems, data.playerItems); AskIfSilent = true; break;
            case GPTType.Bystander: gpt = new GPTBystander(); AskIfSilent = false; break;
            case GPTType.Judge: gpt = new GPTJudge(data.crime); AskIfSilent = true; break;
            case GPTType.Guard: gpt = new GPTGuard(data.crime); AskIfSilent = true; break;
        }

        gpt.StartUp(data, this);
    }
    public void Activate() { gpt.Activate(); voiceController.activated = true; }
    public void NewMessage(string message) { gpt.NewMessage(message); }
    public async void Summerize(MonoBehaviour caller) { await gpt.Summerize(); Destroy(caller.gameObject); }




    public async void NewVoiceMessage(string message) { responseText.text = message; voiceController.NewVoiceMessage(message, await gpt.GetEmotion()); }
    public void UpdateMouth(int value) { animationController.UpdateMouth(value); }
    public void ChangeAnimation(CharacterAnimationController.AnimationState state) { animationController.ChangeState(state); }

    public void BuyTest() { }
    public void SellTest() { }
    public void LeaveTest() { }

    public string GetDynamicPrompt(int attitudeValue)
    {
        string attitude = "indifferent";
        if (attitudeValue <= 0) { attitude = "hatred"; UpdateMouth(0); }
        if (attitudeValue <= 25f && attitudeValue > 0) { attitude = "dislike"; UpdateMouth(1); }
        if (attitudeValue <= 50f && attitudeValue > 25f) { attitude = "indifferent"; UpdateMouth(2); }

        if (attitudeValue < 75f && attitudeValue > 50f) { attitude = "like"; UpdateMouth(2); }
        if (attitudeValue < 100 && attitudeValue >= 75f) { attitude = "friendly"; UpdateMouth(3); }
        if (attitudeValue >= 100) { attitude = "love"; UpdateMouth(4); }

        string returnString =
            ActionString + "\n" +
            "Your character' feelings towards the player is: " + attitude + "\n" +
            "player: "
            ;
        ActionString = "";
        return returnString;

    }
    public int[] ConvertStringToIntArray(string str)
    {
        // Step 1: Split the string by commas
        string[] stringNumbers = str.Split(',');

        // Step 2: Create a list to hold the integers
        List<int> intList = new List<int>();

        // Step 3: Loop through the substrings
        foreach (string s in stringNumbers)
        {
            // Trim whitespace and parse the integer
            if (int.TryParse(s.Trim(), out int number))
            {
                intList.Add(number);
            }
            else
            {
                Debug.LogWarning($"Unable to parse '{s}' as an integer.");
            }
        }

        // Step 4: Convert the list to an array
        return intList.ToArray();
    }
    public List<string> emotions = new List<string>
{
    "angry",
    "sad",
    "happy",
    "calm",
    "nervous",
    "excited",
    "bored",
    "fearful",
    "curious",
    "confident",
    "disappointed",
    "grateful",
    "sarcastic",
    "shocked",
    "hopeful",
    "relaxed",
    "proud",
    "guilty",
    "playful",
    "determined",
    "jealous",
    "mischievous",
    "tired",
    "annoyed",
    "embarrassed",
    "vengeful"
};
}
public interface GPT
{
    void StartUp(GPTData data, LLMController controller); //starting up
    void Activate(); //initial call
    void NewMessage(string message);
    Task<string> GetEmotion();
    Task Summerize();
}
public struct GPTData
{
    public GPTType Type;
    public CharacterSO character;
    public string summery;
    public int attitude;

    public bool isSeller;
    public string Selleritems;
    public string playerItems;

    public string crime;
    public string setting;
}
