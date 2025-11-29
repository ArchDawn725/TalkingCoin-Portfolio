using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Controller;

public class GPTGuard : GPT
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
    public GPTGuard(string crime)
    {
        this.crime = crime;
        Debug.Log(crime);
    }
    public void StartUp(GPTData data, LLMController con)
    {
        LLM = con;
        character = data.character;
        summery = data.summery;

        attitudeTowardsPlayer = data.attitude;
        patience = 15;
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

        if (messages.Count == 0) { started = true; LLM.voiceController.started = true; await SendReply(GetStaticPrompt() + message); }
        else if (!verdictMade) 
        { 
            if (patience < 3) { await SendReply(LLM.GetDynamicPrompt(attitudeTowardsPlayer) + patienceAlmostOver + message); }
            else { await SendReply(LLM.GetDynamicPrompt(attitudeTowardsPlayer) + message); }
        }
        else if (!done)
        {
            LLM.voiceController.started = false;
            LLM.ChangeAnimation(CharacterAnimationController.AnimationState.Leaving);
            done = true;
            if (pardened)
            {
                await SendReply(message + winPrompt);
                //await GetVerdictDetails(message);
                //send away? fine?
                LLM.bodyController.GoHome(false);
                Controller.Instance.ChangeScene(Scenes.Trading);
                //Controller.Instance.characterManager.LeaveInteraction(false, true);
            }
            else
            {
                await SendReply(message + losePrompt);
                //Controller.Instance.characterManager.LeaveInteraction(false); 
                //send to gallows? Fined?
                Controller.Instance.TakePlayer();
                LLM.bodyController.ChangeDestination(Controller.Instance.CourtLocations.GetChild(2), CharacterMovementController.Destination.court);
            }
        }
        else { await SendReply(message); }
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
    private async void UpdateAttitudeBasedOnResponse(OpenAI.CreateChatCompletionResponse response)
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
                if (!accusation)
                {
                    if (numbers[numbers.Length - 2] == -1 || patience <= 0) { pardened = false; verdictMade = true; LLM.leaving = true; }
                    else if (numbers[numbers.Length - 2] == 1) { pardened = true; verdictMade = true; LLM.leaving = true; }
                    else { LLM.animationController.Reaction = numbers[0]; }
                }
                else
                {
                    if (numbers[numbers.Length - 1] == 1)
                    {
                        LLM.leaving = true;
                        string newMessage = await SendReply(message + accusePrompt);
                        LLM.bodyController.GoHome(false);
                        string accused = await GetAccused(newMessage);
                        Controller.Instance.AccusationMade(accused);
                    }
                    else { LLM.animationController.Reaction = numbers[0]; }
                }
            }
        }
        else
        {
            Debug.LogWarning("No response generated.");
        }
    }
    private async Task<string> SendReply(string thisMessage)
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
            return message.Content;
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
            return "";
        }
    }
    private async Task<string> GetAccused(string message)
    {
        Debug.Log("Starting ccusation");

        // Store Controller instance in a local variable for convenience
        var controller = Controller.Instance;

        // Correct the typo in the occupations list and collect all occupations
        List<string> occupations = new List<string>();
        foreach (CharacterSO character in controller.characters)
        {
            occupations.Add(character.occupation);
        }

        // Create a system message to guide the model strictly
        var systemMessage = new ChatMessage()
        {
            Role = "system",
            Content = "You are a helpful assistant. You must strictly follow the user's instructions and respond exactly as requested."
        };

        // Create a new user message with a properly formatted occupations list
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = $"Manager Instruction: The player provided an accusation. Extract and match the occupation mentioned by the player from the given list of occupations.\n" +
                  $"List of possible occupations (use one of these exactly, cannot be the farmer): {string.Join(", ", occupations)}.\n" +
                  $"Player's message: \"{message}\"\n" +
                  "Only respond with the exact name of the occupation that matches the accusation. Do not respond with any extra words or sentences."
        };

        // Create the list of messages to send to OpenAI
        List<ChatMessage> localMessages = new List<ChatMessage>(messages) { systemMessage, newMessage };

        // Await the response from OpenAI
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = localMessages,
            Temperature = 0.2f // Lower temperature to reduce randomness and ensure adherence to the structure
        });

        // Ensure the response is valid
        if (completionResponse.Choices == null || completionResponse.Choices.Count == 0)
        {
            Debug.LogWarning("No valid response was received for the accusation request.");
            return null;
        }

        var responseMessage = completionResponse.Choices[0].Message.Content.Trim();

        // Post-processing to ensure that the response is a valid occupation
        if (occupations.Contains(responseMessage, StringComparer.OrdinalIgnoreCase))
        {
            return responseMessage;
        }

        Debug.LogWarning($"Received invalid occupation '{responseMessage}'.");
        return null;
    }
    private async Task GetVerdictDetails(string message)
    {
        /*
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content =
            "Manager Instruction: Respond with a list of items that were bought, their quantity, and their agreed price. " +
            "Return the response in the following JSON format ONLY (do not add extra text or explanations): \n" +
            "{ \"items\": [ { \"itemName\": \"<name>\", \"amount\": <quantity>, \"price\": <agreed_price> }, ... ] }\n" +
            "Message: " + message
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

        // Now parse the JSON response to extract item details
        try
        {
            PurchaseDetails purchaseDetails = JsonUtility.FromJson<PurchaseDetails>(thismessage.Content);
            foreach (var item in purchaseDetails.items)
            {
                if (!isSeller)
                {
                    Controller.Instance.RemoveItem(item.itemName, item.amount);
                    Controller.Instance.inventoryController.GoldChange(item.price);
                }
                else
                {
                    Controller.Instance.AddItem(item.itemName, item.amount);
                    Controller.Instance.inventoryController.GoldChange(-item.price);
                }
                for (int i = 0; i < item.amount; i++) { grabbingItems.Add(item.itemName); }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse the response: " + ex.Message);
        }
        */
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
    private string winPrompt = " Manager Instruction: You have not found evidance to convict the player and are now leaving.";
    private string losePrompt = " Manager Instruction: You are now going to take the player to the queen to face trial; order the player to follow you to the court.";
    private string accusePrompt = " Manager Instruction: You have got the evidance that you needed and will go arrest the accused and take them to court.";
    private string patienceAlmostOver = "  Manager Instruction: You have grown impatient, this has taken long enough. Make your desicion now. Player: ";
    private bool verdictMade;
    private bool pardened;
    private bool accusation;
    private string GetApproach()
    {
        return "You are in medieval Britain, **speak in period-appropriate language**. If you have never spoken to the player, begin with introducing yourself.";
    }
    private string GetObjective()
    {
        if (crime != "guard requested by player.")
        {
            return "You are a guard, called to the merchant's (Player) booth because he is being accused of this crime: " +
    crime +
    " your job is to determine if there is enough evidence for the player to be placed on trial. Take into account any past accusations, as too many of those can be evidence as well."
    ;
        }
        else
        {
            accusation = true;
            return "You are a guard, called to the merchant's (Player) booth from their request." +
" your job is to determine if the player is accusing someone of committing a crime, who they are accusing, and of what crime."//player abusing 911 system?
;
        }
    }
    //needs a call to gpt to ask which character SO is being accusded and what happens
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
             "3: Indicate if a verdict should be made. -1 if the player should face trial, 0 if not finished, 1 if no crime has been committed, the player is innocent, or there is not enough evidence to face trail." + "\n" +
             "4: Indicate if the player has accused someone of being the murderer. 0 if no job title of the accused has been stated yet, 1 if the job title of the accused has been clearly stated." + "\n" +
             "Message: " + message;
    }
}
