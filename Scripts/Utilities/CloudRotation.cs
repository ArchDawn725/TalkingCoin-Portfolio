using UnityEngine;

public class CloudRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    private void Start()
    {
        Vector3 newRotation = new Vector3(transform.eulerAngles.x, Random.Range(0f, 360f), transform.eulerAngles.z);
        transform.eulerAngles = newRotation;
    }
    private void Update()
    {
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
