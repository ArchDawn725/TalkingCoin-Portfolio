using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName;
    [SerializeField] private float holdWeight;
    [SerializeField] private FullBodyBipedIK ik;
    private OffsetPose holdPose;
    [SerializeField] private bool isEmpty;

    private void Start()
    {
        holdPose = GetComponent<OffsetPose>();
    }

    private IEnumerator OnPickUp()
    {
        while (holdWeight < 1f)
        {
            holdWeight += Time.deltaTime;
            yield return null;
        }
        DeleteItem();
    }

    private void DeleteItem()
    {
        if (!isEmpty)
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (ik == null && holdPose == null) return;
        holdPose.Apply(ik.solver, holdWeight, ik.transform.rotation);
    }
}
