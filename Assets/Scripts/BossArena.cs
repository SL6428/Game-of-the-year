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
        DisableWallColliders();
        CacheWallColliders();
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
                col.isTrigger = false;
                Log($"Wall '{wall.name}' collider DISABLED");
            }

            Renderer rend = wall.GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false;
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
                col.isTrigger = false;
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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (bossWasDefeated) return;

        Log($"Player '{other.name}' entered arena trigger!");
        ActivateArena();
    }

    private bool IsBossDead()
    {
        if (boss == null) return true;
        if (!boss.activeInHierarchy) return true;
        if (bossHealth != null) return bossHealth.IsDead;
        return false;
    }

    private void Log(string message)
    {
        if (debugMode)
            Debug.Log($"[BossArena] {message}");
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
