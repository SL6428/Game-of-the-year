using UnityEngine;

/// <summary>
/// Связывает EnemyAI и Animator — передаёт скорость движения в анимации.
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Устанавливает параметр скорости для Animator (0 = idle, 0.5 = walk, 1 = run).
    /// </summary>
    public void SetSpeed(float speed)
    {
        if (animator != null && animator.enabled)
        {
            animator.SetFloat(SpeedHash, speed);
        }
    }

    /// <summary>
    /// Проигрывает анимацию смерти.
    /// </summary>
    public void PlayDeath()
    {
        if (animator != null && animator.enabled)
        {
            animator.SetTrigger(IsDeadHash);
        }
    }

    /// <summary>
    /// Проигрывает анимацию атаки.
    /// </summary>
    public void PlayAttack()
    {
        if (animator != null && animator.enabled)
        {
            animator.SetTrigger(AttackHash);
        }
    }
}
