using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelUpShrine : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string promptMessage = "Нажмите E для молитвы";

    [Header("Level Up Panel (assign your own Panel here)")]
    [SerializeField] private GameObject levelUpPanel;

    [Header("Prompt (optional — assign existing UI panel)")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    private Transform player;
    private bool isPlayerInRange;
    private TextMeshProUGUI autoPrompt;
    private LevelUpMenu menu;

    void Start()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
            if (promptText != null)
                promptText.text = promptMessage;
        }
    }

    void Update()
    {
        if (LevelUpMenu.IsOpen)
        {
            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape))
                CloseMenu();
            return;
        }

        if (player == null)
        {
            GameObject po = GameObject.FindGameObjectWithTag("Player");
            if (po != null) player = po.transform;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        bool was = isPlayerInRange;
        isPlayerInRange = dist <= interactionRadius;

        if (isPlayerInRange && !was)
            ShowPrompt();

        if (!isPlayerInRange && was)
            HidePrompt();

        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            HidePrompt();
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);

            menu = levelUpPanel.GetComponent<LevelUpMenu>();
            if (menu == null)
                menu = levelUpPanel.GetComponentInChildren<LevelUpMenu>(true);
            if (menu == null)
            {
                CanvasGroup cg = levelUpPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = levelUpPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                menu = levelUpPanel.AddComponent<LevelUpMenu>();
            }

            EnsureEventSystem();
            menu.Open();
            return;
        }

        // Auto-create if no panel assigned
        if (menu == null)
            menu = CreateAutoMenu();

        EnsureEventSystem();
        menu.Open();
    }

    private void CloseMenu()
    {
        if (menu != null)
            menu.Hide();

        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    private LevelUpMenu CreateAutoMenu()
    {
        GameObject canvasObj = new GameObject("LevelUpCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("LevelUpPanel");
        panel.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(520, 650);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        CanvasGroup cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        return panel.AddComponent<LevelUpMenu>();
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    private void ShowPrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            return;
        }

        if (autoPrompt != null)
        {
            autoPrompt.gameObject.SetActive(true);
            return;
        }

        GameObject c = new GameObject("ShrinePrompt");
        Canvas cv = c.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 60;
        c.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject t = new GameObject("Txt");
        t.transform.SetParent(c.transform, false);
        RectTransform rt = t.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 60);
        rt.sizeDelta = new Vector2(220, 22);

        autoPrompt = t.AddComponent<TextMeshProUGUI>();
        autoPrompt.text = promptMessage;
        autoPrompt.fontSize = 14;
        autoPrompt.color = new Color(1f, 0.9f, 0.5f);
        autoPrompt.alignment = TextAlignmentOptions.Center;
        autoPrompt.raycastTarget = false;
        autoPrompt.outlineColor = Color.black;
        autoPrompt.outlineWidth = 0.25f;
    }

    private void HidePrompt()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);
        else if (autoPrompt != null)
            autoPrompt.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}