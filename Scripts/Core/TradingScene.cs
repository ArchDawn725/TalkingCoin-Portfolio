using UnityEngine;

public class TradingScene : Scene
{
    private SceneSO thisScene;

    private Controller con;

    public TradingScene(SceneSO newScene)
    {
        //set unique variables
        thisScene = newScene;
    }
    public void StartUp(Controller newCon)
    {
        //set common variables
        con = newCon;

        //spawn characters
        SpawnRandomCharacter();
        if (newCon.ActiveGuard != null) { newCon.ActiveGuard.GetComponent<CharacterBodyController>().ChangeDestination(newCon.spawnLocations.GetChild(0), CharacterMovementController.Destination.home); }
    }
    private void SpawnRandomCharacter()
    {
        if (con.inventoryController.MyItems.Count == 0) { con.ActivateCart(); return; }

        con.characterManager.SpawnCharacter(RandomSpawnData());
    }
    private GameObject SpawnSpecificCharacter(CharacterSO newCharacter, LLMController.GPTType newGPT)
    {
        SpawnData newSpawnData = new SpawnData
        {
            spawnTransform = GetRandomSpawnLocation(),
            destinationTransform = GetRandomTalkLocation(),
            homeDestination = GetRandomSpawnLocation(),

            character = newCharacter,
            gptType = newGPT,
        };

        return con.characterManager.SpawnCharacter(newSpawnData);
    }
    public void Activate()
    {
        SpawnRandomCharacter();
    }
    public void ShutDown(bool delete)
    {
        if (!delete)
        {
            con.ActiveGuard = SpawnSpecificCharacter(con.characters[21], LLMController.GPTType.Guard);
        }
    }
    private Transform GetRandomSpawnLocation() { return con.spawnLocations.GetChild(Random.Range(0, con.spawnLocations.childCount)); }
    private Transform GetRandomTalkLocation() { return con.talkLocations.GetChild(Random.Range(0, con.talkLocations.childCount)); }
    private CharacterSO GetRandomCharacter() { return con.characters[Random.Range(0, con.characters.Count)]; }
    private SpawnData RandomSpawnData()
    {
        return new SpawnData
        {
            spawnTransform = GetRandomSpawnLocation(),
            destinationTransform = GetRandomTalkLocation(),
            homeDestination = GetRandomSpawnLocation(),

            character = GetRandomCharacter(),
            gptType = LLMController.GPTType.Trader,
        };
    }
}
