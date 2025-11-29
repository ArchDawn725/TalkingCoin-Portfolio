using UnityEngine;
using System.Collections.Generic;
using static RootMotion.Demos.Turret;
using UnityEngine.TextCore.Text;

public class CharacterManager
{
    private Dictionary<CharacterSO, string> characterSummaries = new Dictionary<CharacterSO, string>();
    private Dictionary<CharacterSO, int> characterAttitudes = new Dictionary<CharacterSO, int>();

    private readonly Controller con;
    private readonly GameObject characterPrefab;

    private readonly GameObject cartPrefab;

    /*

    private readonly Transform spawnLocations;
    private readonly Transform talkLocations;
    private readonly CharacterSO[] characters;
    private Cart cart;
    private readonly Transform cartMoveTarget;
    private CharacterBodyController newCharacterBody;
    public string crime;
     */

    public CharacterManager(Controller controller, GameObject prefab, GameObject cartPrefab)
    {
        this.con = controller;
        this.characterPrefab = prefab;

        this.cartPrefab = cartPrefab;
    }

    public GameObject SpawnCharacter(SpawnData data)
    {
        GameObject spawnedCharacter = Object.Instantiate(characterPrefab, data.spawnTransform.position, Quaternion.identity);
        SetUpCharacter(spawnedCharacter, data);
        return spawnedCharacter;
    }
    public void SpawnCart()
    {
        GameObject spawnedCharacter = Object.Instantiate(cartPrefab, con.spawnLocations.GetChild(Random.Range(0, con.spawnLocations.childCount)).position, Quaternion.identity);
        con.cart = spawnedCharacter.GetComponent<Cart>();
        SetUpCartCharacter(spawnedCharacter);
    }
    private void SetUpCharacter(GameObject spawnedObject, SpawnData spawnData)
    {
        string newSummery = "";
        if (characterSummaries.ContainsKey(spawnData.character)) { newSummery = characterSummaries[spawnData.character]; }

        LLMController characterGPT = spawnedObject.GetComponent<LLMController>();
        CharacterBodyController newCharacterBody = spawnedObject.GetComponent<CharacterBodyController>();

        GPTData data = GPTData(
            LLMController.GPTType.Trader,
            spawnData.character,
            newSummery,
            false,
            "",//trading items
             Controller.Instance.inventoryController.GetItems(Controller.Instance.inventoryController.MyItems, false),
             spawnData.setting
            );

        characterGPT.StartUp(data, spawnData.gptType);

        spawnedObject.GetComponent<TTSCon>().StartUp(spawnData.character);

        int characterIndex = con.characters.IndexOf(spawnData.character);
        Transform characterMesh = spawnedObject.transform.GetChild(0).GetChild(characterIndex + 1);
        if (characterMesh != null)
        {
            characterMesh.gameObject.SetActive(true);
        }

        newCharacterBody.StartUp(spawnData.destinationTransform, con.transform, spawnData.character, characterIndex, spawnData.homeDestination);
    }
    public void SetUpCartCharacter(GameObject spawnedObject)
    {
        string newSummery = "";
        if (characterSummaries.ContainsKey(con.characters[9])) { newSummery = characterSummaries[con.characters[9]]; }

        LLMController characterGPT = spawnedObject.GetComponent<Cart>().character.GetComponent<LLMController>();

        GPTData data = GPTData(
            LLMController.GPTType.Trader,
            con.characters[9],
            newSummery,
            true,
            Controller.Instance.inventoryController.GetItems(Controller.Instance.inventoryController.CartItems, true),
            Controller.Instance.inventoryController.GetItems(Controller.Instance.inventoryController.MyItems, false),
            ""
            );

        characterGPT.StartUp(data, LLMController.GPTType.Trader);

        characterGPT.GetComponent<TTSCon>().StartUp(Controller.Instance.characters[9]);
    }

    public void SetSummery(string summary, CharacterSO character, int attitude)
    {
        if (characterSummaries.ContainsKey(character))
        {
            // If the character already exists in the dictionary, override the summary
            characterSummaries[character] = summary;
            characterAttitudes[character] = attitude;
        }
        else
        {
            // If the character doesn't exist, add it to the dictionary
            characterSummaries.Add(character, summary);
            characterAttitudes.Add(character, attitude);
        }
    }
    public int GetAttitude(CharacterSO character)
    {
        if (characterAttitudes.ContainsKey(character))
        {
            return characterAttitudes[character];
        }
        else
        {
            return UnityEngine.Random.Range(25, 75);
        }
    }
    private GPTData GPTData(LLMController.GPTType type, CharacterSO character, string summery, bool seller, string sellerItems, string playerItems, string setting)
    {
        return new GPTData
        {
            Type = type,
            character = character,
            summery = summery,
            isSeller = seller,
            Selleritems = sellerItems,
            playerItems = playerItems,
            attitude = GetAttitude(character),

            crime = crime,
            setting = setting,
        };
    }
    public string crime;
    /*

    public void LeaveInteraction(bool hasCart, bool callRandom)
    {

    }
    */
}

public struct SpawnData
{
    public Transform spawnTransform;
    public Transform destinationTransform;
    public Transform homeDestination;

    public CharacterSO character;
    public LLMController.GPTType gptType;
    public string setting;
}
