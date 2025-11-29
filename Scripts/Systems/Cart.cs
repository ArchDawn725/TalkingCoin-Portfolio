using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

public class Cart : MonoBehaviour
{
    public Transform character;
    private CharacterBodyController characterBodyCon;
    [SerializeField] private Wheel wheel;
    [SerializeField] private Transform right;
    [SerializeField] private Transform left;
    [SerializeField] private Transform startingTransform;
    [SerializeField] private Transform turnTransform;

    private NavMeshAgent agent;
    private bool toPlayer;
    private bool turning;

    public Transform spawnedItemHolder;
    public Transform spawnedItemLocation;
    public void Activate(Transform moveTarget, bool toPlayer)
    {
        gameObject.SetActive(true);
        agent = GetComponent<NavMeshAgent>();
        characterBodyCon = character.GetComponent<CharacterBodyController>();
        characterBodyCon.cart = this;

        characterBodyCon.GetComponent<NavMeshAgent>().enabled = false;
        this.toPlayer = toPlayer;
        AttachCharacterToCart();
        MoveToDestination(moveTarget);
        StartCoroutine(MovingCoroutine());
        Invoke(nameof(StartDelay), 0.1f);
    }
    private void StartDelay() { characterBodyCon.ikController.StartPosing(left, right); }
    private void AttachCharacterToCart() => character.SetParent(transform);
    private void DetachCharacterFromCart() => character.SetParent(null);

    private void MoveToDestination(Transform moveTarget)
    {
        characterBodyCon.AniCon.agent = agent;
        characterBodyCon.AniCon.SetWalking(true);
        agent.updateRotation = true;
        agent.SetDestination(moveTarget.position);
        characterBodyCon.SetLookTargret(moveTarget);

        wheel.RotationTarget = -125f;
    }
    private IEnumerator MovingCoroutine()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        AtDestination();
    }
    private void AtDestination()
    {
        agent.updateRotation = false;
        turning = true;
        wheel.RotationTarget = 50f;
    }
    private void Update()
    {
        if (turning)
        {
            RotateCart();
        }
    }

    private void RotateCart()
    {

        float rotationStep = 45f * Time.deltaTime;
        float newYRotation = transform.eulerAngles.y - rotationStep;

        if (newYRotation < 0f)
        {
            newYRotation += 360f;
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, newYRotation, transform.eulerAngles.z);

        if (newYRotation <= 1f || newYRotation > 350 || Mathf.Approximately(newYRotation, 0f))
        {
            turning = false;
            CompleteRotation();
        }
    }
    private void CompleteRotation()
    {
        wheel.RotationTarget = 0f;
        if (toPlayer)
        {
            characterBodyCon.AniCon.SetWalking(false);
            characterBodyCon.ikController.StopInteractions();
            DetachCharacterFromCart();

            // Obtain the necessary parameters from the CharacterManager
            Transform targetLocation = Controller.Instance.talkLocations.GetChild(1);
            Transform player = Controller.Instance.transform;
            CharacterSO selectedCharacter = Controller.Instance.characters[9];


            // Call StartUp with the correct parameters
            characterBodyCon.GetComponent<NavMeshAgent>().enabled = true;
            characterBodyCon.StartUp(targetLocation, player, selectedCharacter, 9, startingTransform);
        }
        else
        {
            character.GetComponent<LLMController>().Summerize(this);
        }
    }
    public void MoveCharacter()
    {
        characterBodyCon.AniCon.agent = characterBodyCon.GetComponent<NavMeshAgent>();
        characterBodyCon.ChangeDestination(startingTransform, CharacterMovementController.Destination.cart);
    }
}
