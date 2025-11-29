using System.Collections.Generic;
using UnityEngine;

public class CourtScene : Scene
{
    private SceneSO thisScene;

    private List<LLMController> spawnedCharacters = new List<LLMController>();

    private Controller con;

    private string crime;
    private bool activated;
    public CourtScene(string newCrime, SceneSO newScene)
    {
        //set unique variables
        crime = newCrime;
        thisScene = newScene;
    }
    public void StartUp(Controller newCon)
    {
        Debug.Log("Court start");
        //set common variables
        con = newCon;
        con.CalledToMove(con.CourtLocations.GetChild(1));

        SpawnData data = new SpawnData
        {
            spawnTransform = con.CourtLocations.GetChild(0),
            destinationTransform = con.CourtLocations.GetChild(0),
            homeDestination = con.CourtLocations.GetChild(6),

            character = con.characters[18],
            gptType = LLMController.GPTType.Judge,
        };

        GameObject spawned = con.characterManager.SpawnCharacter(data);
        spawnedCharacters.Add(spawned.GetComponent<LLMController>());
        SpawnBystanders();
    }
    private void SpawnBystanders()
    {
        int spawnAmount = Random.Range(0, 4);
        List<CharacterSO> spawnableCharacters = new List<CharacterSO>();

        foreach (CharacterSO character in con.characters)
        {
            if (!character.royalty) { spawnableCharacters.Add(character); }
        }

        //spawn characters
        for (int i = 0; i < spawnAmount; i++)
        {
            //data

            SpawnData data = new SpawnData
            {
                spawnTransform = con.CourtLocations.GetChild(3 + i),
                destinationTransform = con.CourtLocations.GetChild(3 + i),
                homeDestination = con.CourtLocations.GetChild(6),

                character = spawnableCharacters[Random.Range(0, spawnableCharacters.Count)],
                gptType = LLMController.GPTType.Bystander,
                setting = "You see the merchant (the player) on trail for a terrible crime, do not intervene or interrupt as the Queen holds court for their crime.",
            };

            con.characterManager.SpawnCharacter(data);
        }
    }
    public void Activate()
    {
        Debug.Log("Court Activated");
        if (!activated)
        {
            spawnedCharacters[0].Activate();
        }
        else
        {
            Controller.Instance.ChangeScene(Controller.Scenes.Trading);
        }
        activated = true;
    }
    public void ShutDown(bool delete)
    {
        if (delete)
        {
            //destroy all
            for (int i = spawnedCharacters.Count - 1; i <= 0; i--)
            {
                spawnedCharacters[i].Summerize(spawnedCharacters[i]);
            }
        }
        else
        {
            //make player leave
        }
    }
}

/*
 
    public GameObject judge;
    public void SpawnJudgeTest(CharacterSO selectedCharacter, LLMController.GPTType gpt)
    {
        // Check if the character is already spawned
        if (spawnedCharacters.ContainsKey(selectedCharacter))
        {
            SetUpCharacter(spawnedCharacters[selectedCharacter], selectedCharacter, gpt);
            return;
        }

        // Spawn a new character if it does not exist already
        GameObject spawnedCharacter = Object.Instantiate(characterPrefab, Controller.Instance.specialLocations.GetChild(0).position, Quaternion.identity);
        judge = spawnedCharacter;

        // Store the spawned character in the dictionary
        spawnedCharacters.Add(selectedCharacter, spawnedCharacter);

        LLMController characterGPT = spawnedCharacter.GetComponent<LLMController>();
        newCharacterBody = spawnedCharacter.GetComponent<CharacterBodyController>();

        // Set up the character
        string newSummery = "";
        if (characterSummaries.ContainsKey(selectedCharacter)) { newSummery = characterSummaries[selectedCharacter]; }

        GPTData data = GPTData(
            LLMController.GPTType.Trader,
            selectedCharacter,
            newSummery,
            false,
            "",//trading items
            Controller.Instance.inventoryController.GetItems(Controller.Instance.inventoryController.MyItems, false)
            );

        characterGPT.StartUp(data, gpt);
        //characterGPT.SetUp(character, approach, Controller.Instance.inventoryController.GetItems(Controller.Instance.inventoryController.MyItems, false), "Buy an item at the lowest price", "", newSummery, this);

        spawnedCharacter.GetComponent<TTSCon>().StartUp(selectedCharacter);

        int characterIndex = System.Array.IndexOf(characters, selectedCharacter);
        Transform characterMesh = spawnedCharacter.transform.GetChild(0).GetChild(characterIndex + 1);
        if (characterMesh != null)
        {
            characterMesh.gameObject.SetActive(true);
        }

        newCharacterBody.StartUp(Controller.Instance.specialLocations.GetChild(0), con.transform, selectedCharacter, characterIndex);
    }
 */
