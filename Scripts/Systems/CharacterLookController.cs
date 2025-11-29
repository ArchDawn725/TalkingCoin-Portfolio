using UnityEngine;

public class CharacterLookController
{
    private enum LookState
    {
        Moving,
        CloseToDestination,
        Arrived
    }

    private Transform headLocation;
    private Transform lookAtReference;
    private MonoBehaviour parent;
    private Vector3 currentLookAtPoint;
    private const float LookAtLerpSpeed = 2.5f;
    [SerializeField] private Transform destination;
    [SerializeField] private LookState lookState = LookState.Moving;

    public CharacterLookController(MonoBehaviour controller, Transform headLocation, Transform lookAtReference)
    {
        this.headLocation = headLocation;
        this.lookAtReference = lookAtReference;
        parent = controller;
        currentLookAtPoint = GetRandomPointInCone();
    }

    public void SetDestination(Transform destination)
    {
        this.destination = destination;
        lookState = LookState.Moving;
    }

    public void UpdateLook()
    {
        if (destination != null)
        {
            switch (lookState)
            {
                case LookState.Arrived:
                    // Look at the player
                    LookAtPlayer();
                    break;
                case LookState.CloseToDestination:
                    // Look at the destination when close
                    LookAtDestination();
                    break;
                case LookState.Moving:
                    // Randomly look around when walking
                    HandleRandomLook();
                    CheckProximityToDestination();
                    break;
            }
        }
    }

    private void HandleRandomLook()
    {
        lookAtReference.position = Vector3.Lerp(lookAtReference.position, currentLookAtPoint, LookAtLerpSpeed * Time.deltaTime);

        if (Vector3.Distance(lookAtReference.position, currentLookAtPoint) < 0.5f)
        {
            currentLookAtPoint = GetRandomPointInCone();
        }
    }

    private void LookAtDestination()
    {
        lookAtReference.position = Vector3.Lerp(lookAtReference.position, destination.position, LookAtLerpSpeed * Time.deltaTime);
        if (Vector3.Distance(parent.transform.position, destination.position) < 0.5f)
        {
            lookState = LookState.Arrived;
        }
        if (destination != null && Vector3.Distance(parent.transform.position, destination.position) > 5f)
        {
            lookState = LookState.Moving;
        }
    }

    private void LookAtPlayer()
    {
        Transform playerTransform = Controller.Instance.transform;
        lookAtReference.position = Vector3.Lerp(lookAtReference.position, playerTransform.position, LookAtLerpSpeed * Time.deltaTime);

        if ( Vector3.Distance(parent.transform.position, destination.position) > 0.5f)
        {
            lookState = LookState.CloseToDestination;
        }
    }

    private void CheckProximityToDestination()
    {
        if (destination != null && Vector3.Distance(parent.transform.position, destination.position) < 5f)
        {
            lookState = LookState.CloseToDestination;
        }
    }

    private Vector3 GetRandomPointInCone()
    {
        Vector3 center = headLocation.position;
        float radius = 10f;
        float angle = 30f;
        Vector3 direction = parent.transform.forward.normalized;

        float angleRad = Mathf.Deg2Rad * angle;
        float u = Random.Range(0f, 1f);
        float cosPhi = Mathf.Lerp(1f, Mathf.Cos(angleRad), u);
        float phi = Mathf.Acos(cosPhi);
        float theta = Random.Range(0f, 2f * Mathf.PI);

        float x = Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = Mathf.Sin(phi) * Mathf.Sin(theta);
        float z = Mathf.Cos(phi);

        Vector3 localDirection = new Vector3(x, y, z);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
        return center + rotation * localDirection * radius;
    }
}
