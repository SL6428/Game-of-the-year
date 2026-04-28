using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossArenaTriggerForwarder : MonoBehaviour
{
    [SerializeField] private BossArena arena;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        arena?.OnPlayerEntered(other);
    }
}
