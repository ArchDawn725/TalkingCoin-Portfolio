using LeastSquares.Undertone;
using System.Diagnostics;
using System;
using TMPro;
using UnityEngine;

public class STTCon : MonoBehaviour
{
    public static STTCon Instance;
    private void Awake() {  Instance = this; }

    [SerializeField] private PushToTranscribe transcriber;
    [SerializeField] private TMP_InputField transcriptionText;
    [SerializeField] private TextMeshProUGUI placeholderText;

    private bool isRecording;
    private bool isTalkable = true;

    public async void OnClicked()
    {
        placeholderText.text = "WAIT.";
        if (!transcriber.Engine.Loaded) return;

        int model = GetModelFactor();
        transcriber.StartRecording();
        isRecording = true;
        if (Controller.Instance.ActiveInteraction != null)
        {
            Controller.Instance.ActiveInteraction.animationController.ChangeState(CharacterAnimationController.AnimationState.Listening);
        }
        transcriptionText.text = string.Empty;
        isTalkable = false;
        Invoke(nameof(Delay), 0.1f * model);
    }
    private int GetModelFactor()
    {
        return transcriber.Engine.SelectedModel switch
        {
            "whisper-tiny.en" => 1,
            "whisper-base.en" => 2,
            "whisper-small.en" => 4,
            _ => 0
        };
    }
    private void Delay()
    {
        placeholderText.text = "LISTENING";
    }
    public async void OnRelease()
    {
        if (!isRecording) return;

        isRecording = false;
        placeholderText.text = "TRANSCRIBING...";

        Stopwatch stopwatch = Stopwatch.StartNew();
        string transcription = await transcriber.StopRecording();
        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;

        transcriptionText.text = transcription;
        SendTranscription(transcription);
    }

    private void SendTranscription(string text)
    {
        if (Controller.Instance.ActiveInteraction != null)
        {
            Controller.Instance.ActiveInteraction.NewMessage(text);
        }
        Ready();
    }
    private void Ready()
    {
        isTalkable = true;
        //transcriptionText.text = string.Empty;
        //placeholderText.text = "PRESS AND HOLD SPACE BAR TO TALK";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isTalkable)
        {
            OnClicked();
        }

        if (Input.GetKeyUp(KeyCode.Space) && isRecording)
        {
            OnRelease();
        }
    }
}
