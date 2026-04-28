using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private GameObject boss;
    [SerializeField] private Health bossHealth;

    [Header("Entrance Trigger")]
    [Tooltip("The trigger collider on this object. Must have isTrigger=true.")]
    [SerializeField] private Collider entranceTrigger;

    [Header("Walls (always active GameObjects, colliders start disabled)")]
    [SerializeField] private GameObject wallNorth;
    [SerializeField] private GameObject wallSouth;
    [SerializeField] private GameObject wallEast;
    [SerializeField] private GameObject wallWest;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    [Header("Gizmo Color")]
    [SerializeField] private Color gizmoTriggerColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color gizmoWallColor = new Color(0f, 1f, 0f, 0.5f);

    private bool isArenaActive;
    private bool bossWasDefeated;
    private Collider[] wallColliders;

    void Start()
    {
        FindBossHealth();
        DetachWallsFromParent();
        DisableWallColliders();
        CacheWallColliders();
        ValidateTrigger();
    }

    private void DetachWallsFromParent()
    {
        GameObject[] walls = { wallNorth, wallSouth, wallEast, wallWest };
        foreach (var wall in walls)
        {
            if (wall == null) continue;
            if (wall.transform.parent != null)
            {
                Vector3 worldPos = wall.transform.position;
                Quaternion worldRot = wall.transform.rotation;
                wall.transform.SetParent(null, true);
                wall.transform.position = worldPos;
                wall.transform.rotation = worldRot;
                Log($"Wall '{wall.name}' detached from parent '{wall.transform.parent?.name}'");
            }
        }
    }

    private void ValidateTrigger()
    {
        if (entranceTrigger == null)
        {
            LogError("entranceTrigger is NOT assigned in Inspector!");
            return;
        }

        if (!entranceTrigger.isTrigger)
        {
            LogError($"entranceTrigger '{entranceTrigger.name}' must have isTrigger=true!");
            entranceTrigger.isTrigger = true;
        }

        if (entranceTrigger.gameObject != gameObject)
        {
            LogWarning($"entranceTrigger '{entranceTrigger.name}' is on a different GameObject than BossArena script. " +
                       "OnTriggerEnter may NOT fire! Move the trigger Collider to the same object as this script, " +
                       "or ensure the object with the Collider also has a script forwarding OnTriggerEnter.");
        }

        if (!entranceTrigger.enabled)
        {
            LogWarning("entranceTrigger is disabled — arena cannot be entered!");
        }
    }

    void Update()
    {
        if (isArenaActive && !bossWasDefeated && IsBossDead())
        {
            bossWasDefeated = true;
            DeactivateArena();
        }
    }

    private void FindBossHealth()
    {
        if (boss == null)
        {
            Log("Boss not assigned!");
            return;
        }

        if (bossHealth == null)
            bossHealth = boss.GetComponent<Health>();

        if (bossHealth == null)
            Log($"Health not found on boss '{boss.name}'!");
    }

    private void CacheWallColliders()
    {
        GameObject[] walls = { wallNorth, wallSouth, wallEast, wallWest };
        System.Collections.Generic.List<Collider> colliders =
            new System.Collections.Generic.List<Collider>();

        foreach (var wall in walls)
        {
            if (wall != null)
            {
                Collider col = wall.GetComponent<Collider>();
                if (col != null)
                    colliders.Add(col);
            }
        }

        wallColliders = colliders.ToArray();
    }

    private void DisableWallColliders()
    {
        GameObject[] walls = { wallNorth, wallSouth, wallEast, wallWest };

        foreach (var wall in walls)
        {
            if (wall == null) continue;

            Collider col = wall.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
                Log($"Wall '{wall.name}' collider DISABLED");
            }
        }
    }

    private void EnableWallColliders()
    {
        GameObject[] walls = { wallNorth, wallSouth, wallEast, wallWest };

        foreach (var wall in walls)
        {
            if (wall == null) continue;

            Collider col = wall.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
                Log($"Wall '{wall.name}' collider ENABLED");
            }
        }
    }

    public void ActivateArena()
    {
        if (isArenaActive) return;

        EnableWallColliders();

        if (entranceTrigger != null)
            entranceTrigger.enabled = false;

        isArenaActive = true;
        Log("Arena ACTIVATED - walls are up!");
    }

    public void DeactivateArena()
    {
        if (!isArenaActive) return;

        DisableWallColliders();

        if (entranceTrigger != null)
            entranceTrigger.enabled = false;

        isArenaActive = false;
        Log("Arena DEACTIVATED - walls are down!");
    }

    public void ResetArena()
    {
        if (!isArenaActive) return;

        DisableWallColliders();

        if (entranceTrigger != null)
            entranceTrigger.enabled = true;

        isArenaActive = false;
        Log("Arena RESET after player death - walls down, trigger re-enabled!");
    }

    void OnTriggerEnter(Collider other)
    {
        OnPlayerEntered(other);
    }

    public void OnPlayerEntered(Collider other)
    {
        bool isPlayer = other.CompareTag("Player")
            || other.GetComponent<PlayerController>() != null
            || other.GetComponent<CharacterController>() != null;

        if (!isPlayer)
        {
            Log($"Trigger ignored: '{other.name}' is not player (tag={other.tag})");
            return;
        }

        if (bossWasDefeated)
        {
            LogWarning("Player entered but bossWasDefeated = true — arena blocked!");
            return;
        }

        Log($"Player '{other.name}' entered arena trigger!");
        ActivateArena();
    }

    public void ResetArenaAndHealBoss()
    {
        ResetArena();
        if (bossHealth != null)
        {
            bossHealth.ResetHealth();
            Log("Boss healed to full HP after player death!");
        }
    }

    private bool IsBossDead()
    {
        if (boss == null) return false; // босс не назначен ≠ побеждён
        if (!boss.activeInHierarchy) return true;
        if (bossHealth != null) return bossHealth.IsDead;
        return false;
    }

    private void Log(string message)
    {
        if (debugMode)
            Debug.Log($"[BossArena] {message}");
    }

    private void LogWarning(string message)
    {
        if (debugMode)
            Debug.LogWarning($"[BossArena] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[BossArena] {message}");
    }

    void OnDrawGizmos()
    {
        if (entranceTrigger != null)
        {
            Gizmos.color = gizmoTriggerColor;

            if (entranceTrigger is BoxCollider box)
            {
                Gizmos.matrix = entranceTrigger.transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (entranceTrigger is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(
                    entranceTrigger.transform.position + sphere.center,
                    sphere.radius);
            }
        }

        Gizmos.color = gizmoWallColor;
        GameObject[] walls = { wallNorth, wallSouth, wallEast, wallWest };

        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] == null) continue;
            Collider col = walls[i].GetComponent<Collider>();

            if (col is BoxCollider wallBox)
            {
                Gizmos.matrix = walls[i].transform.localToWorldMatrix;
                Gizmos.DrawWireCube(wallBox.center, wallBox.size);
            }
            else if (col != null)
            {
                Gizmos.DrawWireSphere(walls[i].transform.position, 0.5f);
            }
        }

        if (wallNorth != null && wallSouth != null &&
            wallEast != null && wallWest != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wallNorth.transform.position, wallEast.transform.position);
            Gizmos.DrawLine(wallEast.transform.position, wallSouth.transform.position);
            Gizmos.DrawLine(wallSouth.transform.position, wallWest.transform.position);
            Gizmos.DrawLine(wallWest.transform.position, wallNorth.transform.position);
        }
    }
}
