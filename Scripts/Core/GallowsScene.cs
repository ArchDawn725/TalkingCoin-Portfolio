using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GallowsScene : Scene
{
    private SceneSO thisScene;

    private LLMController[] spawnedCharacters;

    private Controller con;
    private int activated;

    public GallowsScene(SceneSO newScene)
    {
        //set unique variables
        thisScene = newScene;
    }
    public void StartUp(Controller newCon)
    {
        //set common variables
        con = newCon;
        con.CalledToMove(con.GallowsLocations.GetChild(1));

        SpawnBystanders();
        if (newCon.ActiveGuard != null) { newCon.ActiveGuard.GetComponent<CharacterBodyController>().ChangeDestination(newCon.GallowsLocations.GetChild(7), CharacterMovementController.Destination.none); }
    }
    private void SpawnBystanders()
    {
        int spawnAmount = Random.Range(0, 4);
        List<CharacterSO> spawnableCharacters = new List<CharacterSO>();

        foreach (CharacterSO character in con.characters)
        {
            spawnableCharacters.Add(character);
        }

        //spawn characters
        for (int i = 0; i < spawnAmount; i++)
        {
            //data

            SpawnData data = new SpawnData
            {
                spawnTransform = con.GallowsLocations.GetChild(4 + i),
                destinationTransform = con.GallowsLocations.GetChild(4 + i),
                homeDestination = con.GallowsLocations.GetChild(4 + i),

                character = spawnableCharacters[Random.Range(0, spawnableCharacters.Count)],
                gptType = LLMController.GPTType.Bystander,
                setting = "You see the merchant (the player) at the gallows, sent to death, for a terrible crime. This is your last chance to say your last words to them.",
            };

            con.characterManager.SpawnCharacter(data);
        }
    }
    public void Activate()
    {
        //start talking
        if (activated == 0) { con.SpeakTest(0); }//preist
        if (activated == 1) { con.SpeakTest(1); }//headsman
        if( activated == 2) { ShutDown(true); }
        activated++;
    }
    public void ShutDown(bool delete)
    {
        //destroy all characters
        SceneManager.LoadScene(0);
    }
}
