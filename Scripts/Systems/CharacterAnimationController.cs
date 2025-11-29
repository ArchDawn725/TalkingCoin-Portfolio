using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimationController : MonoBehaviour
{
    public enum AnimationState
    {
        Approaching,
        Idle,
        Listening,
        Reacting,
        Speaking,
        Leaving
    }

    public AudioSource Source;
    public AnimationState State;

    public SpriteRenderer Mouth;
    [SerializeField] private Sprite[] mouthSprites;
    [SerializeField] private Animator animator;
    private float lastToggleTime;
    public int Reaction;
    public NavMeshAgent agent;

    private void Update()
    {
        if (State == AnimationState.Speaking)
        {
            CheckAudioStatus();
        }

        if (animator.GetBool("Walking"))
        {
            animator.speed = Mathf.Clamp(agent.velocity.magnitude, 0.25f, 1);
        }
    }

    public void UpdateMouth(int value)
    {
        if (value >= 0 && value < mouthSprites.Length)
        {
            Mouth.sprite = mouthSprites[value];
        }
    }

    public void ChangeState(AnimationState newState)
    {
        if (newState == AnimationState.Leaving)
        {
            SetState(AnimationState.Leaving);
        }
        else if (State != AnimationState.Leaving)
        {
            State = newState;
            animator.SetInteger("State", (int)State);
            animator.SetInteger("Reaction", Reaction);

            switch (State)
            {
                case AnimationState.Idle:
                    //SpeechController.Instance.Ready();
                    break;
                case AnimationState.Speaking:
                    lastToggleTime = Time.time;
                    break;
            }
        }
    }

    private void SetState(AnimationState newState)
    {
        State = newState;
        animator.SetInteger("State", (int)State);
    }
    public void SetWalking(bool value)
    {
        animator.SetBool("Walking", value);
        if (!value)
        {
            animator.speed = 1;
        }
    }

    private void CheckAudioStatus()
    {
        if (!Source.isPlaying && Source.time > 0)
        {
            Source.time = 0;
            ChangeState(AnimationState.Idle);
        }

        float timeSinceToggled = Time.time - lastToggleTime;

        if (timeSinceToggled > Source.clip.length * 1.5f)
        {
            Debug.LogWarning("Failsafe triggered: Returning to Idle state.");
            ChangeState(AnimationState.Idle);
        }
    }
}
