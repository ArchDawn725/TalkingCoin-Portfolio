using UnityEngine;
using UnityEngine.AI;
using static RootMotion.Demos.Turret;

public class CharacterBodyController : MonoBehaviour
{
    [SerializeField] private Transform headLocation;
    [SerializeField] private Transform lookAtReference;

    [SerializeField] private Transform mouth;
    [SerializeField] private Transform lips;

    public CharacterMovementController movementController;
    public CharacterIKController ikController {  get; private set; }
    private CharacterHighlightController highlightController;
    private CharacterLookController lookController;
    private LLMController GPTCon;
    public CharacterSO Character { get; private set; }
    public Transform PlayerTransform { get; private set; }

    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftHandTarget;
    [HideInInspector] public CharacterAnimationController AniCon { get; private set; }

    [SerializeField] private Transform items;
    [SerializeField] private Transform helms;

    public Transform follow;
    public Transform home;
    public Cart cart;
    private void Awake()
    {
        movementController = new CharacterMovementController(this, GetComponent<NavMeshAgent>());
        ikController = new CharacterIKController(this);
        highlightController = new CharacterHighlightController(this);
        lookController = new CharacterLookController(this, headLocation, lookAtReference);
        GPTCon = GetComponent<LLMController>();
        AniCon = GetComponent<CharacterAnimationController>();
    }
    public void StartUp(Transform targetLocation, Transform player, CharacterSO character, int characterIndex, Transform newHome)
    {
        home = newHome;
        gameObject.SetActive(true);
        //movementController.InitializeComponents();
        highlightController.skinnedMeshRenderer = transform.GetChild(0).GetChild(characterIndex + 1).GetComponent<SkinnedMeshRenderer>();
        SetIndependentTarget(targetLocation);

        mouth.localPosition = character.mouthPos;
        lips.localPosition = character.lipsPos;

        if (Random.Range(0, 100) > 50 && character.itemNumber != -1) { items.GetChild(character.itemNumber).gameObject.SetActive(true); ikController.hasItem = true; }
        if (Random.Range(0, 100) > 50 && character.helmNumber != -1) { helms.GetChild(character.helmNumber).gameObject.SetActive(true); }

        ikController.StopInteractions();
        if (Controller.Instance.SceneName != "Trading") { movementController.StartMoving(CharacterMovementController.Destination.none); }
        else { movementController.StartMoving(CharacterMovementController.Destination.player); }
        
        PlayerTransform = Controller.Instance.transform;
    }
    public void ChangeDestination(Transform target, CharacterMovementController.Destination destination)
    {
        ikController.StopInteractions();
        SetIndependentTarget(target);
        movementController.StartMoving(destination);
    }
    public void SetIndependentTarget(Transform target)
    {
        movementController.SetTarget(target);
        SetLookTargret(target);
    }
    public void SetLookTargret(Transform target)
    {
        lookController.SetDestination(target);
    }

    private void Update()
    {
        movementController.HandleMovement();
    }

    public void Highlight() => highlightController.Highlight();
    public void UnHighlight() => highlightController.UnHighlight();

    private void FixedUpdate()
    {
        lookController.UpdateLook();
    }
    public void DoneMoving(CharacterMovementController.Destination destination)
    {
        switch (destination)
        {
            case CharacterMovementController.Destination.player:
                if (Random.Range(0, 100) > 50) { ikController.StartPosing(leftHandTarget, rightHandTarget); }
                GPTCon.Activate(); 
                return;

            case CharacterMovementController.Destination.cart:
                //Controller.Instance.SendCartAway(); 
                cart.Activate(Controller.Instance.spawnLocations.GetChild(Random.Range(0, Controller.Instance.spawnLocations.childCount)), false);
                return;

            case CharacterMovementController.Destination.court:
                Controller.Instance.CalledToMove(Controller.Instance.CourtLocations.GetChild(1)); 
                ChangeDestination(Controller.Instance.CourtLocations.GetChild(2), CharacterMovementController.Destination.other);
                //Controller.Instance.TriggerJudge();
                return;

            case CharacterMovementController.Destination.home:
                GPTCon.Summerize(this); 
                return;
        }
    }
    public void GoHome(bool callRandom)
    {
        Debug.Log("Go Home");
        if (!cart)
        {
            CharacterBodyController bodyCon = Controller.Instance.ActiveInteraction.GetComponent<CharacterBodyController>();
            ChangeDestination(home, CharacterMovementController.Destination.home);
        }
        else
        {
            Debug.Log("Cart");
            cart.MoveCharacter();
        }

        if (callRandom) { Controller.Instance.CallNextCharacter(); }
    }
}
