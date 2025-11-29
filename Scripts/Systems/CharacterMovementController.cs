using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovementController
{
    private readonly MonoBehaviour parent;
    private readonly NavMeshAgent agent;
    private Transform target;
    private bool moving;

    private bool turning;
    private CharacterBodyController con;

    public enum  Destination
    {
        none,
        player,
        cart,
        home,
        court,
        other
    }
    public Destination destination;

    public CharacterMovementController(MonoBehaviour parent, NavMeshAgent navMeshAgent)
    {
        this.parent = parent;
        con = parent.GetComponent<CharacterBodyController>();
        this.agent = navMeshAgent;
    }
    /*
    public void InitializeComponents()
    {
        // Add any additional movement setup here if needed
    }
    */
    public void SetTarget(Transform targetLocation)
    {
        target = targetLocation;
    }

    public void StartMoving(Destination newDestination)
    {
        destination = newDestination;
        if (target != null)
        {
            moving = true;
            agent.SetDestination(target.position);
            agent.isStopped = false;
            con.AniCon.SetWalking(true);
        }
        else
        {
            Debug.LogWarning("Target location is not set. Cannot start moving.");
        }
    }

    public void HandleMovement()
    {
        if (moving && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            StopMoving();
        }
        else if (turning) { Turner(); }
    }

    public bool IsMoving() => moving;

    private void StopMoving()
    {
        moving = false;
        agent.isStopped = true;
        turning = true;
        con.AniCon.SetWalking(false);
    }
    private void Turner()
    {
        // Get the target rotation from the target transform
        Quaternion targetRotation = target.rotation;

        // Rotate only around the Y-axis (ignore x and z axis rotation)
        targetRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

        // Calculate the rotation to move towards the target rotation smoothly
        if (Quaternion.Angle(agent.transform.rotation, targetRotation) > 0.5f) // Threshold angle to start and stop rotation
        {
            Quaternion newRotation = Quaternion.RotateTowards(agent.transform.rotation, targetRotation, Time.deltaTime * 100f);

            // Apply the new rotation to the agent
            agent.transform.rotation = newRotation;
        }
        else
        {
            // Consider rotation complete if the difference is small
            DoneTurning();
        }
    }
    private void DoneTurning()
    {
        turning = false;
        con.DoneMoving(destination);
    }
}
