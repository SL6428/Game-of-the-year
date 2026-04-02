using UnityEngine;
using UnityEditor;

/// <summary>
/// Инструмент для проверки настройки боевой системы.
/// Запустите через меню: Tools → Combat System Validator
/// </summary>
public class CombatSystemValidator : EditorWindow
{
    [MenuItem("Tools/Combat System Validator")]
    public static void ShowWindow()
    {
        GetWindow<CombatSystemValidator>("Combat Validator");
    }

    private Vector2 scrollPosition;

    void OnGUI()
    {
        GUILayout.Label("🎮 Combat System Validator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (GUILayout.Button("▶️ Run All Checks", GUILayout.Height(30)))
        {
            ValidateAll();
        }

        GUILayout.Space(20);
        GUILayout.Label("Checks:", EditorStyles.boldLabel);

        // Проверка тега Player
        CheckPlayerTag();

        GUILayout.Space(10);

        // Проверка слоя Enemy
        CheckEnemyLayer();

        GUILayout.Space(10);

        // Проверка сцены
        CheckSceneSetup();

        GUILayout.Space(10);

        // Проверка префабов
        CheckPrefabs();

        GUILayout.EndScrollView();
    }

    private void ValidateAll()
    {
        CheckPlayerTag();
        CheckEnemyLayer();
        CheckSceneSetup();
        CheckPrefabs();
    }

    private void CheckPlayerTag()
    {
        GUILayout.Label("1. Player Tag Check", EditorStyles.boldLabel);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            EditorGUILayout.HelpBox($"✓ Player found: {player.name}", MessageType.Info);

            // Проверка компонентов
            if (player.GetComponent<PlayerController>() != null)
                EditorGUILayout.HelpBox("  ✓ PlayerController present", MessageType.Info);
            else
                EditorGUILayout.HelpBox("  ✗ PlayerController MISSING", MessageType.Error);

            if (player.GetComponent<Health>() != null)
                EditorGUILayout.HelpBox("  ✓ Health present", MessageType.Info);
            else
                EditorGUILayout.HelpBox("  ✗ Health MISSING", MessageType.Error);

            if (player.GetComponent<PlayerRegeneration>() != null)
                EditorGUILayout.HelpBox("  ✓ PlayerRegeneration present", MessageType.Info);
            else
                EditorGUILayout.HelpBox("  ✗ PlayerRegeneration MISSING", MessageType.Error);

            if (player.GetComponent<Weapon>() != null)
                EditorGUILayout.HelpBox("  ✓ Weapon present", MessageType.Info);
            else
                EditorGUILayout.HelpBox("  ⚠ Weapon MISSING (may be on child)", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("✗ Player tag NOT found! Add 'Player' tag to player object.", MessageType.Error);
        }
    }

    private void CheckEnemyLayer()
    {
        GUILayout.Label("2. Enemy Layer & Tag Check", EditorStyles.boldLabel);

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
        {
            EditorGUILayout.HelpBox($"✓ Enemy layer exists (ID: {enemyLayer})", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("✗ Enemy layer NOT found! Add 'Enemy' layer in TagManager.", MessageType.Error);
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            EditorGUILayout.HelpBox($"✓ Found {enemies.Length} enemy object(s) with 'Enemy' tag", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ No enemies found with 'Enemy' tag in current scene", MessageType.Warning);
        }
    }

    private void CheckSceneSetup()
    {
        GUILayout.Label("3. Scene Setup Check", EditorStyles.boldLabel);

        // CameraPivot
        CameraPivot pivot = FindFirstObjectByType<CameraPivot>();
        if (pivot != null)
        {
            EditorGUILayout.HelpBox($"✓ CameraPivot found: {pivot.gameObject.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("✗ CameraPivot NOT found! Create object with CameraPivot component.", MessageType.Error);
        }

        // LockOnSystem
        LockOnSystem lockOn = FindFirstObjectByType<LockOnSystem>();
        if (lockOn != null)
        {
            EditorGUILayout.HelpBox($"✓ LockOnSystem found: {lockOn.gameObject.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ LockOnSystem NOT found (optional)", MessageType.Warning);
        }

        // GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            EditorGUILayout.HelpBox($"✓ GameManager found: {gm.gameObject.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ GameManager NOT found (optional)", MessageType.Warning);
        }
    }

    private void CheckPrefabs()
    {
        GUILayout.Label("4. Prefabs Check", EditorStyles.boldLabel);

        string prefabsPath = "Assets/Prefabs";
        if (System.IO.Directory.Exists(prefabsPath))
        {
            EditorGUILayout.HelpBox($"✓ Prefabs folder exists", MessageType.Info);

            string[] prefabs = System.IO.Directory.GetFiles(prefabsPath, "*.prefab");
            EditorGUILayout.HelpBox($"  Found {prefabs.Length} prefab(s)", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ Prefabs folder NOT found. Create 'Assets/Prefabs' for convenience.", MessageType.Warning);
        }
    }
}
