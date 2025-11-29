using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CharacterIKController
{
    private readonly CharacterBodyController bodyController;
    private readonly InteractionSystem interactionSystem;
    private readonly FullBodyBipedIK ik;
    public bool hasItem;

    public CharacterIKController(CharacterBodyController controller)
    {
        bodyController = controller;
        interactionSystem = controller.transform.GetChild(0).GetComponent<InteractionSystem>();
        ik = controller.transform.GetChild(0).GetComponent<FullBodyBipedIK>();
    }
    public void GrabItems(List<string> items, bool fromCart)
    {
        ResetIK();
        bodyController.StartCoroutine(GrabItemsCoroutine(items, fromCart));
    }

    private IEnumerator GrabItemsCoroutine(List<string> items, bool fromCart)
    {
        StopInteractions();
        Debug.Log(items.Count);
        foreach (var item in items)
        {
            Debug.Log(item);
            InteractionObject pickingUp = Controller.Instance.FindItem(item);
            interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, pickingUp, true);
            yield return new WaitUntil(() => pickingUp == null);
        }

        if (fromCart)
        {
            interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, Controller.Instance.EmptyCartItem, true);
            Controller.Instance.GenerateMyItems(items);
            yield return new WaitForSeconds(1);
        }

        yield return new WaitForSeconds(1);
        bodyController.GoHome(true);
        //Controller.Instance.characterManager.LeaveInteraction(fromCart, true);
    }
    public void ResetIK()
    {
        ik.solver.leftHandEffector.target = null;
        ik.solver.leftHandEffector.positionWeight = 0;
        ik.solver.leftHandEffector.rotationWeight = 0;
        if (!hasItem)
        {
            ik.solver.rightHandEffector.target = null;
            ik.solver.rightHandEffector.positionWeight = 0;
            ik.solver.rightHandEffector.rotationWeight = 0;
        }
    }
    public void StopInteractions()
    {
        interactionSystem.ResumeInteraction(FullBodyBipedEffector.LeftHand);

        if (!hasItem)
        {
            interactionSystem.ResumeInteraction(FullBodyBipedEffector.RightHand);
        }

        ResetIK();
    }
    public void StartPosing(Transform leftHandTarget, Transform rightHandTarget)
    {
        interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, leftHandTarget.GetComponent<InteractionObject>(), true);

        if (!hasItem)
        {
            interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, rightHandTarget.GetComponent<InteractionObject>(), true);
        }

        SetIKTargets(leftHandTarget, rightHandTarget);
    }
    private void SetIKTargets(Transform leftHandTarget, Transform rightHandTarget)
    {
        ik.solver.leftHandEffector.target = leftHandTarget;

        if (!hasItem)
        {
            ik.solver.rightHandEffector.target = rightHandTarget;
        }
    }
}
