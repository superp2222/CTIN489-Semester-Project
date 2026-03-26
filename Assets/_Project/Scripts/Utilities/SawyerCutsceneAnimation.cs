using UnityEngine;

public class SawyerCutsceneAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void PlayStandUp()
    {
        if (animator == null) return;

        animator.ResetTrigger("StandUp");
        animator.SetTrigger("StandUp");
    }

    public void SetSittingIdle()
    {
        if (animator == null) return;

        animator.Play("Sitting", 0, 0f);
    }

    public void SetNormalIdle()
    {
        if (animator == null) return;

        animator.Play("Idle", 0, 0f);
    }
}