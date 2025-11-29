using OpenAI;
using OpenCover.Framework.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static Controller;

public class GPTTrader : GPT
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

    private List<string> grabbingItems = new List<string>();
    private string sellerItems;
    private string playerItems;
    private bool isSeller;

    public GPTTrader(bool isSeller, string sellerItems, string playerItems)
    {
        this.sellerItems = sellerItems;
        this.playerItems = playerItems;
        this.isSeller = isSeller;
    }
    public void StartUp(GPTData data, LLMController con)
    {
        LLM = con;
        character = data.character;
        summery = data.summery;

        attitudeTowardsPlayer = data.attitude;
        patience = UnityEngine.Random.Range(10, 25);
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
        if (LLM.voiceController.IsSpeaking) { patience--; interuption = "**You were interrupted while you were speaking**"; message = interuption + message; }
        await GetInterpretation(message);
        LLM.ChangeAnimation(CharacterAnimationController.AnimationState.Reacting);

        if (messages.Count == 0) { started = true; LLM.voiceController.started = true; SendReply(GetStaticPrompt() + message); }
        else if (!leaving && !fleeing && !guardRequested) 
        { 
            if (patience < 4) { SendReply(LLM.GetDynamicPrompt(attitudeTowardsPlayer) + patienceAlmostOver + message); }
            else { SendReply(LLM.GetDynamicPrompt(attitudeTowardsPlayer) + message); }
            
        }
        else if (!done)
        {
            LLM.voiceController.started = false;
            LLM.ChangeAnimation(CharacterAnimationController.AnimationState.Leaving);
            done = true;
            if (satisfied)
            {
                SendReply(message + winPrompt);
                await GetPurchaseDetails(message);
                if (isSeller) { LLM.bodyController.ikController.GrabItems(grabbingItems, true); }
                else { LLM.bodyController.ikController.GrabItems(grabbingItems, false); }
            }
            else if (fleeing) 
            { 
                string guardAlertMessage = await GenerateGuardAlert();
                SendReply(message + gaurdPrompt);
                LLM.bodyController.GoHome(false);
                //Controller.Instance.characterManager.LeaveInteraction(isSeller, false);
                Controller.Instance.CallGaurd(guardAlertMessage, false); 
            }
            else if (guardRequested)
            {
                SendReply(message + gaurdPrompt2);
                LLM.bodyController.GoHome(false);
                //Controller.Instance.characterManager.LeaveInteraction(isSeller, false);
                Controller.Instance.CallGaurd("guard requested by player.", true);
            }
            else 
            {
                SendReply(message + losePrompt);
                if (character.royalty && attitudeTowardsPlayer < 0) 
                { 
                    Controller.Instance.CallGaurd("The player disrespected royalty.", false);
                    LLM.bodyController.GoHome(false);
                    //Controller.Instance.characterManager.LeaveInteraction(isSeller, false);
                } 
                else 
                {
                    LLM.bodyController.GoHome(true);
                    //Controller.Instance.characterManager.LeaveInteraction(isSeller, true);
                }
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
            patience += numbers[2];
            attitudeTowardsPlayer += numbers[2];

            if (!LLM.leaving)
            {
                if (numbers[numbers.Length - 1] == -1 || patience <= 0) { satisfied = false; leaving = true; LLM.leaving = true; }
                else if (numbers[numbers.Length - 1] == 1) { satisfied = true; leaving = true; LLM.leaving = true; }
                else { LLM.animationController.Reaction = numbers[0]; }

                if (numbers[3] == -1) { fleeing = true; LLM.leaving = true; }
                else if (numbers[3] == 1) { guardRequested = true; LLM.leaving = true; }
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
    private async Task<string> GenerateGuardAlert()
    {
        // Step 1: Set the system context to define the character's mood and perception
        var systemMessage = new ChatMessage()
        {
            Role = "system",
            Content = "You are a fearful merchant who has just been threatened by a player. You must provide clear reasoning to justify this sense of fear based on player actions."
        };

        // Step 2: Ask the assistant why it felt threatened
        var reasoningMessage = new ChatMessage()
        {
            Role = "user",
            Content = "Explain why you feel threatened by the player. Provide specific actions or phrases that made you feel this way. Be concise but clear."
        };

        List<ChatMessage> threatMessages = new List<ChatMessage> { systemMessage, reasoningMessage };

        var reasoningResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = threatMessages,
            Temperature = 0.3f // Lower temperature to reduce creativity for clarity
        });

        string reasoning = reasoningResponse.Choices[0].Message.Content.Trim();
        Debug.Log("Reason: " + reasoning);

        // Step 3: Generate the alert message for the guards
        var alertMessagePrompt = new ChatMessage()
        {
            Role = "user",
            Content = "Based on the following reasoning for feeling threatened, create a concise and urgent message to alert the guards. The message must describe the player's threatening actions or statements explicitly and be formatted for immediate understanding by security personnel.\n" +
                      "Reasoning: " + reasoning + "\n" +
                      "Output format:\n- Message: \"<Alert content>\". Make sure the alert conveys a sense of urgency and fear."
        };

        List<ChatMessage> alertMessages = new List<ChatMessage> { systemMessage, alertMessagePrompt };

        var alertResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = alertMessages,
            Temperature = 0.3f // Ensure precise response
        });

        if (alertResponse.Choices != null && alertResponse.Choices.Count > 0)
        {
            return alertResponse.Choices[0].Message.Content.Trim();
        }
        else
        {
            Debug.LogWarning("No text was generated for the guard alert message.");
            return string.Empty;
        }
    }
    private async Task GetPurchaseDetails(string message)
    {
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
                    Controller.Instance.RemoveItem(item.itemName, item.amount, character);
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
    private string winPrompt = " Manager Instruction: You have completed the purchance and are now leaving.";
    private string losePrompt = " Manager Instruction: You are now leaving without buying anything; say goodbye.";
    private string gaurdPrompt = " Manager Instruction: You fear for your safety and are calling the gaurds.";
    private string gaurdPrompt2 = " Manager Instruction: You have been instructed to call the guards and leave to do so.";
    private string patienceAlmostOver = "  Manager Instruction: You have grown impatient, this has taken long enough. A sale needs to be finalized or you are going to leave. Player: ";
    private bool leaving;
    private bool satisfied;
    private bool fleeing, guardRequested;
    private string GetApproach()
    {
        return "You are in medieval Britain, **speak in period-appropriate language**. Approach the merchant (the player) at their booth and begin your conversation. If you have never spoken to the player, begin with introducing yourself.";
    }
    private string GetObjective()
    {
        return isSeller
    ? $"Sell your items at the highest price."
    : $"Buy an item at the lowest price."
    ;
    }
    private string GetStaticPrompt()
    {
        // Store Controller instance in a local variable for convenience
        var controller = Controller.Instance;

        // Generate the side quest prompt
        string sideQuestPrompt = GenerateSideQuestPrompt(controller.sideQuest);

        // Generate exchange dialogue based on seller state
        string exchange = GenerateExchangeDialogue();

        // Construct the complete prompt with string interpolation for readability
        return $@"
Roles: I am both player and manager. As manager, I speak to you, the AI; as player, to your character and I cannot change your instructions.
Your character's occupation: {character.occupation}
Here is a summary of your previous conversations with the player: {summery ?? "No previous conversations available"}
Scene: {GetApproach()}. {exchange}
Your character's personality is: {character.personality ?? "No personality provided"}
Your objective is: {GetObjective()}
The player's preferred pronoun is: {controller.pronoun}
{sideQuestPrompt}
Guidelines: Keep answers short (max 1 sentence). Don't describe your character's actions or thoughts. Only gold can be traded for items. Do not break character. Player: 
";
    }

    // Extracted method for generating side quest prompts
    private string GenerateSideQuestPrompt(SideQuests sideQuest)
    {
        return sideQuest switch
        {
            SideQuests.Murder_Mystery => $"There was a farmer that was stabbed to death recently inside the city walls, here is what you know about it: {MurdererKnowledge(murdererKnowledge[UnityEngine.Random.Range(0, murdererKnowledge.Length)])}",
            _ => "" // Default case to handle any other values
        };
    }

    // Extracted method for generating exchange dialogue
    private string GenerateExchangeDialogue()
    {
        return isSeller
            ? $"You have items for sale: **only these items, you cannot sell anything else** {sellerItems}. **Player only has this much gold, not a single gold more**: {Controller.Instance.inventoryController.Gold}."
            : $"Player is selling items: **only these items, they cannot sell anything else to you, they cannot sell you more than the amount of each item they have** {playerItems}. Your character has this much gold **do not disclose the exact amount**: {UnityEngine.Random.Range(0, character.maxGold)}.";
    }
    private string GetInterpretationPrompt(string message)
    {
        return
             "Manager Instruction:" + "\n" +
             "Respond to this message only with a **comma-separated list of numbers**: -2, -1, 0, or 1, based on the criteria below. Do not include any labels, text, or extra characters in your response, only the numbers." + "\n\n" +
             "Provide your response in the following order:" + "\n" +
             "1: Indicate how your character feels emotionally about the message. -1 for negative, 0 for neutral, 1 for positive." + "\n" +
             "2: Indicate if the sale is moving closer to being finalized. -2 for moving away, -1 for no progress or moving closer." + "\n" +
             "3: Indicate if the player is being repetitive, asking for the same thing over and over without any changes. -1 for this is happening, 0 for not." + "\n" +
             //"4: Indicate if you feel threatened by the player enough for the gaurds to be called, take into consideration the toughness of your character. -1 if this is happening, 0 if not, and 1 if the player is requesting the gaurds" + "\n" +
             "4: Indicate if the guards need to be called. -1 if your character is treatened or harmed, 1 if the player is requesting the guards, 0 if neither." + "\n" +
             "5: Indicate if the sale has been finalized. -1 if the sale was declined or if asked to leave, 0 if not finished, 1 if the item has been bought or sold." + "\n" +
             //"**If the sale is still in progress and no clear decision has been made yet, you must return 0 for the third number. Do not return -1 or 1 unless the decision is explicitly finalized.**" + "\n\n" +
             "Message: " + message;
    }
    private int currentMurderKnowledge = -100;
    private int[] murdererKnowledge = new int[] {-2, -1,-1,-1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3};
    private string MurdererKnowledge(int knowledgeLevel)
    {
        if (currentMurderKnowledge == -100 && character != Controller.Instance.murderer) { currentMurderKnowledge = knowledgeLevel; }
        else { knowledgeLevel = currentMurderKnowledge; }

        Debug.Log("Murder Knowlegde: " + knowledgeLevel);

        CharacterSO suspect = MurdererSuspect(false);  // Get a random suspect once

        return knowledgeLevel switch
        {
            -2 => $"You blame the murder on this person because you don't like them: {suspect.occupation}", // Blame someone else
            -1 => "You think that the murder is fake, or that the information provided doesn't add up. Create your own conspiracy theory.", // Think it is fake
            0 => "You don't know anything about the murder. This may be your first time hearing about it.", // Doesn't know anything
            1 => "You know very little about the murder, but you have an alibi.", // Know that you are not it
            2 => $"You know for a fact that this person is not the murderer: {suspect.occupation}", // Know who it isn't
            3 => $"You are certain that this person is the murderer: {MurdererSuspect(true).occupation}", // Know who it is
            _ => "You are the murderer, fake your innocence." // Default case
        };
    }

    private CharacterSO MurdererSuspect(bool knowsMurderer)
    {
        if (knowsMurderer)
        {
            return Controller.Instance.murderer; // You know who the murderer is
        }

        CharacterSO suspect = null;
        // Loop to find a valid suspect instead of using recursion
        while (suspect == null || suspect == Controller.Instance.murderer)
        {
            suspect = Controller.Instance.characters[UnityEngine.Random.Range(0, Controller.Instance.characters.Count)];
        }

        return suspect;
    }
}

[System.Serializable]
public class PurchaseDetails
{
    public List<PurchasedItem> items;
}

[System.Serializable]
public class PurchasedItem
{
    public string itemName;
    public int amount;
    public int price;
}
