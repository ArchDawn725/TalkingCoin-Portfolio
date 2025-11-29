using UnityEngine;
using UnityEngine.AI;

public class Wheel : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    public float RotationTarget = -125f;
    [SerializeField] private NavMeshAgent agent;

    private void Update()
    {
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
        rotationSpeed = Mathf.Lerp(rotationSpeed, GetRealRotationSpeed(), Time.deltaTime);
    }
    private float GetRealRotationSpeed()
    {
        if (agent != null)
        {
            if (RotationTarget > 0) { return RotationTarget; }
            float value = Mathf.Clamp(agent.velocity.magnitude, 0.25f, 1);
            return RotationTarget * value;
        }
        else { return 0f; }
    }
}
