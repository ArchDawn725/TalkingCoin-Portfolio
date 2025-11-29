using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    public static Controller Instance;
    private void Awake() { Instance = this; }

    public LLMController ActiveInteraction;

    public List<CharacterSO> characters = new List<CharacterSO>();
    public Transform spawnLocations;
    public Transform talkLocations;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] public List<ItemSO> allItemsSO = new List<ItemSO>();
    [SerializeField] private ItemSO poison, artifact;
    [SerializeField] private Transform itemSpawnLocations;
    [SerializeField] private Transform myItemLocations;
    [SerializeField] private Transform spawnedItemsHolder;
    [SerializeField] private Transform myItemsHolder;
    [SerializeField] public Cart cart;
    [SerializeField] private GameObject cartPrefab;
    [SerializeField] private Transform cartMoveTarget;
    public InteractionObject EmptyCartItem;
    [SerializeField] private DayCycle dayCycle;
    public string pronoun;
    private NavMeshAgent agent;
    private CharacterBodyController followBody;
    private bool following;
    public Transform specialLocations;
    public Transform GallowsLocations;
    [SerializeField] private Transform playerStart;
    public Transform CourtLocations;
    public GameObject ActiveGuard;


    public InventoryCon inventoryController {  get; private set; }
    public CharacterManager characterManager {  get; private set; }

    private void Start()
    {
        characterManager = new CharacterManager(this, characterPrefab, cartPrefab);
        inventoryController = new InventoryCon(this, myItemLocations, myItemsHolder);
        agent = GetComponent<NavMeshAgent>();
    }
    public void StartUp()
    {
        inventoryController.GoldChange(100);
        //inventoryController.GoldChange(1000);//test
        ChangeScene(Scenes.Trading); //save/load game?
        //ChangeScene(Scenes.Court); //test
        dayCycle.NextTime();
    }
    
    public void ActivateCart()
    {
        characterManager.SpawnCart();
        spawnedItemsHolder = cart.spawnedItemHolder;
        itemSpawnLocations = cart.spawnedItemLocation;
        inventoryController.spawnedItemsHolder = spawnedItemsHolder;
        inventoryController.itemSpawnLocation = itemSpawnLocations;
        GenerateAIItems(6);
        cart.Activate(cartMoveTarget, true);
        characterManager.SetUpCartCharacter(cart.gameObject);
        dayCycle.NextDay();
    }
  
    public void CallNextCharacter()
    {
        dayCycle.NextTime();
        if (dayCycle.interactions > 5) { ActivateCart(); }
        else { scene.Activate(); }
    }

    public void AddItem(string name, int amount = 1)
    {
        inventoryController.AddItem(name, amount);
    }

    public void RemoveItem(string name, int amountToRemove, CharacterSO character)
    {
        inventoryController.RemoveItem(name, amountToRemove, character);
    }

    public List<InventoryCon.ItemData> GenerateAIItems(int amount)
    {
        return inventoryController.GenerateAIItems(amount);
    }

    public void GenerateMyItems(List<string> items)
    {
        inventoryController.GenerateMyItems(items);
    }

    public InteractionObject FindItem(string name)
    {
        return inventoryController.FindItem(name);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) { ReloadLevel(); }
        if (Input.GetKeyDown(KeyCode.B)) { BuyTest(); }
        if (Input.GetKeyDown(KeyCode.S)) { SellTest(); }
        if (Input.GetKeyDown(KeyCode.L)) { LeaveTest(); }

        if (following) 
        { 
            agent.SetDestination(followBody.follow.position);
        }
        else
        {
            if (moving && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                StopMoving();
            }
        }
    }

    private void BuyTest()
    {
        ActiveInteraction.BuyTest();
    }
    private void SellTest()
    {
        ActiveInteraction.SellTest();
    }
    private void LeaveTest()
    {
        ActiveInteraction.LeaveTest();
    }
    public void ReloadLevel()
    {
        SceneManager.LoadScene(0);
    }
    public void SetSummery(string summery, CharacterSO character, int attitude)
    {
        characterManager.SetSummery(summery, character, attitude);
    }
    public void CallGaurd(string reason, bool playerRequest)
    {
        Debug.Log("Gaurds!!! " + reason);
        characterManager.crime = reason;
        scene.ShutDown(false);
    }
    public void CalledToFollow()
    {
        followBody = ActiveInteraction.GetComponent<CharacterBodyController>();
        following = true;
        moving = true;
    }
    public void CalledToMove(Transform destination)
    {
        moving = true;
        following = false;
        agent.SetDestination(destination.position);
    }
    public void TakePlayer()
    {
        Debug.Log("TakingPlayer!!!!!!!!");
        CalledToFollow();
        ChangeScene(Scenes.Court);
    }
    public void Verdict(bool pardened)
    {
        if (pardened)
        {
            CalledToMove(playerStart);
        }
        else
        {
            ChangeScene(Scenes.Gallows);
        }
    }

    public enum Scenes
    {
        Trading,
        Court,
        Gallows,
    }
    public Scenes myScene;
    private Scene scene;
    public string SceneName;
    [SerializeField] private SceneSO[] scenes;

    public void ChangeScene(Scenes newScene)
    {
        Debug.Log("Change Scene: " + newScene);
        myScene = newScene;
        SceneName = newScene.ToString();

        if (scene != null) { scene.ShutDown(true); }
        switch (newScene)
        {
            case Scenes.Trading: scene = new TradingScene(scenes[0]); break;
            case Scenes.Court: scene = new CourtScene("", scenes[1]); break;
            case Scenes.Gallows: scene = new GallowsScene(scenes[2]); break;
        }
        scene.StartUp(this);
    }

    [SerializeField] private AudioSource[] testGallows;
    public void SpeakTest(int calls)
    {
        if (calls == 0) { testGallows[0].Play(); Invoke(nameof(Delay), 65); }
        if (calls == 1) { testGallows[1].Play(); Invoke(nameof(Delay), 10f); }
    }
    private void Delay()
    {
        scene.Activate();
    }
    private bool moving;
    private void StopMoving()
    {
        if (moving)
        {
            moving = false;
            scene.Activate();
        }
    }
    public CharacterSO murderer;
    public enum SideQuests
    {
        None,
        Murder_Mystery,
        Kill_The_King,
        The_Artifact,
    }
    public SideQuests sideQuest;
    private void SetUpSideQuest()
    {
        string disc = "";
        sideQuest = AssignRandomSideQuest();

        switch (sideQuest)
        {
            case SideQuests.Murder_Mystery:
                disc = MurderMystery();
                break;
            case SideQuests.Kill_The_King:
                disc = KillTheKing();
                break;
            case SideQuests.The_Artifact:
                disc = ObtainArtifact();
                break;
        }

        //update UI
        UIController.Instance.AssaignObjective(sideQuest.ToString() + "\n" + disc);
    }
    private string MurderMystery()
    {
        //choose a non-royal to be the murderer
        murderer = characters[UnityEngine.Random.Range(0, characters.Count)];
        if (murderer.royalty || murderer == null || murderer.isImportant) { MurderMystery(); }
        return "Find the murderer.";
    }
    private string KillTheKing()
    {
        //check to see if king is dead
        CharacterSO king = null;
        foreach(CharacterSO character in characters) { if (character.occupation == "King") { king = character; } }
        if (king == null) { SetUpSideQuest(); return null; }

        if (!allItemsSO.Contains(poison)) { allItemsSO.Add(poison); }
        return "Kill the tyrant king.";
    }
    private string ObtainArtifact()
    {
        if (!allItemsSO.Contains(artifact)) { allItemsSO.Add(artifact); }
        return "Obtain the artifact.";
    }
    public SideQuests AssignRandomSideQuest()
    {
        // Get all enum values
        Array values = Enum.GetValues(typeof(SideQuests));

        // Generate a random index
        System.Random random = new System.Random();
        int randomIndex = random.Next(values.Length);

        // Assign a random value from the enum
        return (SideQuests)values.GetValue(randomIndex);
    }

    public void AccusationMade(string accused)
    {
        Debug.Log("Accused: " + accused);

        if (sideQuest != SideQuests.Murder_Mystery)
        {
            ChangeScene(Scenes.Trading);
            return;
        }
        else
        {
            if (accused == murderer.occupation) //gain gold, kill murderer
            {
                characters.Remove(murderer);
                CompletedSideObjective();
                ChangeScene(Scenes.Trading); 
            }
            else { CallGaurd("The merchant (player) has accused someone of a crime that they did not commit!", false); }//accuse player of crime
        }

    }
    public void CompletedSideObjective()
    {
        inventoryController.GoldChange(100);
        UIController.Instance.AssaignObjective("Objective completed!");
        sideQuest = SideQuests.None;
    }
    public void NewDay()
    {
        if (sideQuest == SideQuests.None) { SetUpSideQuest(); }
    }

}

public interface Scene
{
    public void StartUp(Controller con);
    public void Activate();
    public void ShutDown(bool delete);
}
