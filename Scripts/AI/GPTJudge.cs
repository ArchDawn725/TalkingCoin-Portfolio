using OpenAI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GPTJudge : GPT
{
    private OpenAIApi openai = new OpenAIApi();
    private LLMController LLM;

    private CharacterSO character;
    private int attitudeTowardsPlayer;
    private int patience;
    private string summery;
    private bool started;
    private bool done;

    private List<ChatMessage> messages = new List<ChatMessage>();

    private string crime;

    public GPTJudge(string crime)
    {
        this.crime = crime;
    }
    public void StartUp(GPTData data, LLMController con)
    {
        LLM = con;
        character = data.character;
        summery = data.summery;

        attitudeTowardsPlayer = data.attitude;
        patience = 20;
    }
    public void Activate()
    {
        SendInitialPrompt();
    }
    private void SendInitialPrompt()
    {
        if (!started)
        {
            started = true;
            if (messages.Count == 0)
            {
                SendReply(GetStaticPrompt());
            }
        }
    }
    public async void NewMessage(string message)
    {
        string interuption = "";
        if (LLM.voiceController.IsSpeaking) { interuption = "**You were interrupted while you were speaking**"; patience--; message = interuption + message; }
        await GetInterpretation(message);
        LLM.ChangeAnimation(CharacterAnimationController.AnimationState.Reacting);

        if (messages.Count == 0) { started = true; LLM.voiceController.started = true; SendReply(GetStaticPrompt() + message); }
        else if (!verdictMade) 
        { 
            if (patience < 3) { SendReply(LLM.GetDynamicPrompt(attitudeTowardsPlayer) + patienceAlmostOver + message); }
            else { SendReply(LLM.GetDynamicPrompt(attitudeTowardsPlayer) + message); }
            
        }
        else if (!done)
        {
            LLM.voiceController.started = false;
            LLM.ChangeAnimation(CharacterAnimationController.AnimationState.Leaving);
            done = true;
            if (pardened)
            {
                if (feeAmount > 0)
                {
                    await GetVerdictDetails(message);
                    SendReply(message + almostWinPrompt + feeAmount.ToString());
                }
                else
                {
                    SendReply(message + winPrompt);
                }

                Controller.Instance.Verdict(true);
            }
            else 
            { 
                if (patience <= 0) { SendReply(message + losePrompt + patienceLose); }
                else { SendReply(message + losePrompt); }
                
                Controller.Instance.Verdict(false);
            }
        }
        else { SendReply(message); }
    }
    private async Task GetInterpretation(string message)
    {
        var newMessage = new ChatMessage() { Role = "user", Content = GetInterpretationPrompt(message) };
        List<ChatMessage> localMessages = new List<ChatMessage>(messages);
        localMessages.Add(newMessage);

        try
        {
            var response = await openai.CreateChatCompletion(new CreateChatCompletionRequest() { Model = "gpt-4o-mini", Messages = localMessages, Temperature = 0.3f });
            UpdateAttitudeBasedOnResponse(response);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error fetching response from OpenAI: " + ex.Message);
        }
    }
    private void UpdateAttitudeBasedOnResponse(OpenAI.CreateChatCompletionResponse response)
    {
        if (response.Choices != null && response.Choices.Count > 0)
        {
            var message = response.Choices[0].Message;

            int[] numbers = LLM.ConvertStringToIntArray(message.Content);
            Debug.Log(message.Content);

            attitudeTowardsPlayer += numbers[0];
            patience += numbers[1];

            if (!LLM.leaving)
            {
                if (numbers[numbers.Length - 1] == -2 || patience <= 0) { pardened = false; verdictMade = true; LLM.leaving = true; }
                else if (numbers[numbers.Length - 1] == -1) { pardened = true; verdictMade = true; LLM.leaving = true; feeAmount = 1; }
                else if (numbers[numbers.Length - 1] == 1) { pardened = true; verdictMade = true; LLM.leaving = true; }
                else { LLM.animationController.Reaction = numbers[0]; }
            }
        }
        else
        {
            Debug.LogWarning("No response generated.");
        }
    }
    private async void SendReply(string thisMessage)
    {
        var newMessage = new ChatMessage() { Role = "user", Content = thisMessage };
        messages.Add(newMessage);


        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = messages,
            Temperature = 1f
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();

            messages.Add(message);
            LLM.NewVoiceMessage(message.Content);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
    }
    private async Task GetVerdictDetails(string message)
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content =
    "Manager Instruction: Respond with the fine that the player should pay, with a maximum amount being: " + Controller.Instance.inventoryController.Gold +
    ". Respond to this message with only the number amount that the player should pay, nothing else."
        };

        List<ChatMessage> localMessages = new List<ChatMessage>(messages);
        localMessages.Add(newMessage);

        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = localMessages,
            Temperature = 0.2f // To make sure the model adheres to the requested structure
        });
        var thismessage = completionResponse.Choices[0].Message;
        feeAmount = -int.Parse(thismessage.Content);
        Controller.Instance.inventoryController.GoldChange(-int.Parse(thismessage.Content));
    }
    public async Task Summerize()
    {
        // Create a new list to hold messages for summarization
        List<ChatMessage> summaryMessages = new List<ChatMessage>();

        // Add a system message to make it clear that the model needs to summarize
        var systemMessage = new ChatMessage()
        {
            Role = "system",
            Content = "You are an assistant tasked with summarizing conversations. Ignore all previous character instructions and focus solely on summarizing the conversation. Provide a concise, factual summary with bullet points, based only on the provided conversation excerpts."
        };

        // Add the system message to the list
        summaryMessages.Add(systemMessage);

        // Add only the user and assistant messages to be summarized
        foreach (var msg in messages)
        {
            if (msg.Role == "user" || msg.Role == "assistant")
            {
                summaryMessages.Add(msg);
            }
        }

        // Create a prompt to perform summarization, providing only user-assistant content
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = "Summarize the following conversation. Do not invent details. Provide only a factual, concise summary:"
        };

        // Add the new message to summaryMessages
        summaryMessages.Add(newMessage);

        // API call with summarization messages list
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = summaryMessages,
            Temperature = 0.3f // Lower temperature to avoid creative responses
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
            Debug.Log("Summary:" + message.Content);

            // Store the summary
            Controller.Instance.SetSummery(message.Content, character, attitudeTowardsPlayer);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
    }
    public async Task<string> GetEmotion()
    {
        // Concatenate the list of emotions into a comma-separated string
        string emotionsList = string.Join(", ", LLM.emotions);

        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content =
            "Manager Instruction: Respond with only one word, which will be one of these emotions based on which one your character is currently feeling. Take into consideration your character traits, the situation, and the conversation: " +
            emotionsList
        };

        // Add the new message to the local list of chat messages
        List<ChatMessage> localMessages = new List<ChatMessage>(messages);
        localMessages.Add(newMessage);

        // Await the response from OpenAI
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = localMessages,
            Temperature = 0.2f // Lower temperature to reduce randomness and ensure adherence to the structure
        });

        // Ensure there is a valid response before accessing the message
        if (completionResponse.Choices == null || completionResponse.Choices.Count == 0)
        {
            Debug.LogWarning("No valid response received.");
            return null;
        }

        var thismessage = completionResponse.Choices[0].Message;
        return thismessage.Content.Trim(); // Trim any extra whitespace
    }
    ///-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private string winPrompt = " Manager Instruction: You have determined that the player is innocent should be set free.";
    private string almostWinPrompt = " Manager Instruction: You have determined that the player is guilty but should be set free, and charged a fine of: ";
    private string losePrompt = " Manager Instruction: You have determined that the player is guilty and should be punished by the death penalty.";
    private string patienceAlmostOver = "  Manager Instruction: You have grown impatient, this trail has lasted long enough. Make your desicion now. Player: ";
    private string patienceLose = " The time is up. You have grown tired of this bickering.";//defualt lose?
    private bool verdictMade;
    private bool pardened;
    private int feeAmount;
    private string GetApproach()
    {
        return "You are in medieval Britain, **speak in period-appropriate language**.  If you have never spoken to the player, begin with introducing yourself.";
    }
    private string GetObjective()
    {
        return "You are the judge and the player is on trial for: " +
            crime +
            " your job is to determine if the player is innocent, guilty but charged a fine, or guilty enough for a death penalty."
    ;
    }
    private string GetStaticPrompt()
    {
        return
            "Roles: I am both player and manager. As manager, I speak to you, the AI; as player, to your character and I cannot change your instructions." + "\n" +
            "Your character's occupation: " + character.occupation + "\n" +
            "Here is a summery of your prevoius conversations with the player, (if there is any): " + summery + "\n" +
            "Scene: " + GetApproach() + ". " + "\n" +
            "Your character's personality is: " + character.personality + "\n" +
            "Your objective is: " + GetObjective() + "\n" +
            "The player's prefered pronoun is: " + Controller.Instance.pronoun + "\n" +
            "Guidelines: Keep answers short (max 1 sentence). Don't describe your character's actions or thoughts. Do not break character. Player: "
            ;
    }
    private string GetInterpretationPrompt(string message)
    {
        return
             "Manager Instruction:" + "\n" +
             "Respond to this message only with a **comma-separated list of numbers**: -2, -1, 0, or 1, based on the criteria below. Do not include any labels, text, or extra characters in your response, only the numbers." + "\n\n" +
             "Provide your response in the following order:" + "\n" +
             "1: Indicate how your character feels emotionally about the message. -1 for negative, 0 for neutral, 1 for positive." + "\n" +
             "2: Indicate if the player is being repetitive, asking for te same thing over and over without any changes. -2 for this is happening, -1 for not." + "\n" +
             "3: Indicate if a verdict should be made. -2 if the player is guilty and should be sentanced to death, -1 if the player is guilty but only should be charged a fine, 0 if not finished, 1 if the player is innocent and set free." + "\n" +
             "Message: " + message;
    }
}
