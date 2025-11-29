using Newtonsoft.Json;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Unity.VisualScripting.Member;

public class TTSCon : MonoBehaviour
{
    private const string XI_API_KEY = "sk_3015c845885e6925db1359ba98528c4b5128149765e040e4";
    private string VOICE_ID = "EiNlNiXeDU1pqqOPrYMO";
    private const string OUTPUT_PATH = "output.mp3";
    private const int CHUNK_SIZE = 1024;

    public AudioSource AudioSource;
    private CharacterAnimationController animationController;
    private LLMController gpt;

    private void Start()
    {
        animationController = GetComponent<CharacterAnimationController>();
        gpt = GetComponent<LLMController>();
    }

    public void StartUp(CharacterSO character)
    {
        VOICE_ID = character.voiceID;
    }

    public void NewVoiceMessage(string message, string emotion)
    {
        Debug.Log("Emotion: " + emotion);
        StartCoroutine(ConvertTextToSpeechCoroutine(message, emotion));
    }

    private IEnumerator ConvertTextToSpeechCoroutine(string text, string emotion)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogError("Error: Text to speak cannot be empty.");
            yield break;
        }

        string ttsUrl = $"https://api.elevenlabs.io/v1/text-to-speech/{VOICE_ID}/stream";

        // Extracted method to get the settings
        var (stability, similarityBoost, style, useSpeakerBoost) = GetVoiceSettings(emotion);

        // Set up the data payload for the API request, including the text and voice settings
        var requestData = new
        {
            text = text,
            model_id = "eleven_turbo_v2", // Updated to ensure it matches expected model names
            voice_settings = new
            {
                stability = stability,
                similarity_boost = similarityBoost,
                style = style,
                use_speaker_boost = useSpeakerBoost
            }
        };

        // Set up headers for the API request, including the API key for authentication
        UnityWebRequest request = new UnityWebRequest(ttsUrl, "POST");
        request.SetRequestHeader("Accept", "audio/mpeg");
        request.SetRequestHeader("xi-api-key", XI_API_KEY);
        request.SetRequestHeader("Content-Type", "application/json");

        // Serialize the JSON data using Newtonsoft.Json
        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log($"JSON Data: {jsonData}"); // Debug the JSON string to ensure it's correct
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerAudioClip(ttsUrl, AudioType.MPEG);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            DownloadHandlerAudioClip downloadHandler = request.downloadHandler as DownloadHandlerAudioClip;
            if (downloadHandler != null && downloadHandler.audioClip != null)
            {
                AudioSource.clip = downloadHandler.audioClip;
                PlayAudio();
            }
            else
            {
                Debug.LogError("Failed to retrieve audio clip.");
            }
        }
    }

    private void PlayAudio()
    {
        AudioSource.Play();
        animationController.ChangeState(CharacterAnimationController.AnimationState.Speaking);
    }

    private Coroutine delayCoroutine;
    public bool started;
    public bool IsSpeaking;
    public bool activated;

    private void Update()
    {
        // Start the coroutine if the audio finishes playing
        if (AudioSource != null && !AudioSource.isPlaying && delayCoroutine == null && activated)
        {
            if (!gpt.leaving)
            {
                delayCoroutine = StartCoroutine(TriggerAfterAudioFinishes());
            }
        }
        // Cancel the coroutine if audio starts playing again
        else if (AudioSource != null && AudioSource.isPlaying && delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
            Debug.Log("Reset!!");
        }

        if (!AudioSource.isPlaying)
        {
            IsSpeaking = false;
        }
        else { IsSpeaking = true; }
    }

    private IEnumerator TriggerAfterAudioFinishes()
    {
        // Wait for an additional 1-minute delay (60 seconds)
        yield return new WaitForSeconds(60f);

        // Call your method here
        YourMethod();

        // Reset coroutine reference after completion
        delayCoroutine = null;
    }

    private void YourMethod()
    {
        if (gpt.AskIfSilent)
        {
            gpt.NewMessage("");
            Debug.Log("Triggered after 1 minute since the audio finished playing!");
        }
    }
    private (float stability, float similarityBoost, float style, bool useSpeakerBoost) GetVoiceSettings(string emotion)
    {
        float stability = 0.5f;
        float similarityBoost = 0.8f;
        bool useSpeakerBoost = true;
        float style = 0.0f;

        switch (emotion.ToLower())
        {
            case "angry":
                stability = 0.2f;
                similarityBoost = 0.9f;
                style = 0.7f;
                break;
            case "sad":
                stability = 0.4f;
                similarityBoost = 0.6f;
                style = 0.3f;
                useSpeakerBoost = false;
                break;
            case "happy":
                stability = 0.3f;
                similarityBoost = 0.9f;
                style = 0.8f;
                useSpeakerBoost = true;
                break;
            case "calm":
                stability = 0.7f;
                similarityBoost = 0.5f;
                style = 0.2f;
                useSpeakerBoost = false;
                break;
            case "nervous":
                stability = 0.1f;
                similarityBoost = 0.5f;
                style = 0.5f;
                useSpeakerBoost = false;
                break;
            case "excited":
                stability = 0.3f;
                similarityBoost = 0.9f;
                style = 0.9f;
                useSpeakerBoost = true;
                break;
            case "bored":
                stability = 0.8f;
                similarityBoost = 0.6f;
                style = 0.1f;
                useSpeakerBoost = false;
                break;
            case "fearful":
                stability = 0.2f;
                similarityBoost = 0.4f;
                style = 0.6f;
                useSpeakerBoost = false;
                break;
            case "curious":
                stability = 0.5f;
                similarityBoost = 0.7f;
                style = 0.5f;
                useSpeakerBoost = true;
                break;
            case "confident":
                stability = 0.7f;
                similarityBoost = 0.8f;
                style = 0.6f;
                useSpeakerBoost = true;
                break;
            case "disappointed":
                stability = 0.5f;
                similarityBoost = 0.5f;
                style = 0.3f;
                useSpeakerBoost = false;
                break;
            case "grateful":
                stability = 0.6f;
                similarityBoost = 0.7f;
                style = 0.6f;
                useSpeakerBoost = true;
                break;
            case "sarcastic":
                stability = 0.3f;
                similarityBoost = 0.6f;
                style = 0.4f;
                useSpeakerBoost = true;
                break;
            case "shocked":
                stability = 0.2f;
                similarityBoost = 0.5f;
                style = 0.8f;
                useSpeakerBoost = false;
                break;
            case "hopeful":
                stability = 0.6f;
                similarityBoost = 0.7f;
                style = 0.5f;
                useSpeakerBoost = true;
                break;
            case "relaxed":
                stability = 0.8f;
                similarityBoost = 0.6f;
                style = 0.2f;
                useSpeakerBoost = false;
                break;
            case "proud":
                stability = 0.7f;
                similarityBoost = 0.8f;
                style = 0.7f;
                useSpeakerBoost = true;
                break;
            case "guilty":
                stability = 0.4f;
                similarityBoost = 0.5f;
                style = 0.3f;
                useSpeakerBoost = false;
                break;
            case "playful":
                stability = 0.3f;
                similarityBoost = 0.8f;
                style = 0.9f;
                useSpeakerBoost = true;
                break;
            case "determined":
                stability = 0.5f;
                similarityBoost = 0.9f;
                style = 0.7f;
                useSpeakerBoost = true;
                break;
            case "jealous":
                stability = 0.4f;
                similarityBoost = 0.5f;
                style = 0.4f;
                useSpeakerBoost = false;
                break;
            case "mischievous":
                stability = 0.3f;
                similarityBoost = 0.7f;
                style = 0.8f;
                useSpeakerBoost = true;
                break;
            case "tired":
                stability = 0.9f;
                similarityBoost = 0.5f;
                style = 0.1f;
                useSpeakerBoost = false;
                break;
            case "annoyed":
                stability = 0.3f;
                similarityBoost = 0.6f;
                style = 0.5f;
                useSpeakerBoost = true;
                break;
            case "embarrassed":
                stability = 0.4f;
                similarityBoost = 0.4f;
                style = 0.3f;
                useSpeakerBoost = false;
                break;
            case "vengeful":
                stability = 0.2f;
                similarityBoost = 0.8f;
                style = 0.7f;
                useSpeakerBoost = true;
                break;
            default:
                Debug.LogWarning("Emotion not recognized, using default voice settings.");
                break;
        }

        return (stability, similarityBoost, style, useSpeakerBoost);
    }
}


/*
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class TTSCon : MonoBehaviour
{
    public AudioSource AudioSource;
    private CharacterAnimationController animationController;
    private const int ChunkSize = 1024;
    private const string ApiBaseUrl = "https://api.elevenlabs.io/v1/text-to-speech/";
    private string apiKey;
    [HideInInspector] public string VoiceId = "EiNlNiXeDU1pqqOPrYMO";

    private void Start()
    {
        animationController = GetComponent<CharacterAnimationController>();
        apiKey = GetApiKey(); // Use a method to securely retrieve the API key
    }

    public void StartUp(CharacterSO character)
    {
        VoiceId = character.voiceID;
        Debug.Log("SetUP");
    }

    public void NewVoiceMessage(string message)
    {
        StartCoroutine(GetTextToSpeech(message));
    }

    private IEnumerator GetTextToSpeech(string message)
    {
        string cleanedString = Regex.Replace(message, "[^a-zA-Z0-9\\s!?.,]", "");
        string url = ApiBaseUrl + VoiceId;

        // Create the request headers
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Accept", "audio/mpeg");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", apiKey);

        // Create the JSON data
        string jsonData = $"{{\"text\":\"{cleanedString}\",\"model_id\":\"eleven_turbo_v2_5\",\"voice_settings\":{{\"stability\":0.5,\"similarity_boost\":0.5}}}}";
       
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        // Use DownloadHandlerAudioClip to handle the audio data
        request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

        // Send the request
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError(cleanedString);
        }
        else
        {
            DownloadHandlerAudioClip downloadHandler = request.downloadHandler as DownloadHandlerAudioClip;
            if (downloadHandler != null && downloadHandler.audioClip != null)
            {
                AudioSource.clip = downloadHandler.audioClip;
                PlayAudio();
            }
            else
            {
                Debug.LogError("Failed to retrieve audio clip.");
            }
        }
    }

    private void PlayAudio()
    {
        AudioSource.Play();
        animationController.ChangeState(CharacterAnimationController.AnimationState.Speaking);
    }

    private string GetApiKey()
    {
        // This method should retrieve the API key securely from an encrypted file or environment variable
        return "sk_3015c845885e6925db1359ba98528c4b5128149765e040e4";
    }
}
*/