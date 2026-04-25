using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelUpMenu : MonoBehaviour
{
    private static LevelUpMenu _instance;
    public static bool IsOpen => _instance != null && _instance.isOpen;

    private bool isOpen;

    private const int STAT_COUNT = 6;

    private TextMeshProUGUI soulLevelText;
    private TextMeshProUGUI currencyText;
    private TextMeshProUGUI totalCostText;

    private TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[STAT_COUNT];
    private TextMeshProUGUI[] levelTexts = new TextMeshProUGUI[STAT_COUNT];
    private TextMeshProUGUI[] costTexts = new TextMeshProUGUI[STAT_COUNT];
    private TextMeshProUGUI[] bonusTexts = new TextMeshProUGUI[STAT_COUNT];
    private Button[] plusButtons = new Button[STAT_COUNT];
    private Button[] minusButtons = new Button[STAT_COUNT];

    private Button confirmButton;
    private Button resetButton;
    private Button closeButton;

    private int[] plannedLevels = new int[STAT_COUNT];
    private int[] savedLevels = new int[STAT_COUNT];

    public static void Show()
    {
        if (_instance == null)
            CreateInstance();
        _instance.Open();
    }

    public static void Hide()
    {
        if (_instance != null)
            _instance.Close();
    }

    private static void CreateInstance()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        GameObject obj = new GameObject("LevelUpMenu");

        Canvas canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        obj.AddComponent<GraphicRaycaster>();

        _instance = obj.AddComponent<LevelUpMenu>();
        _instance.BuildUI();
        obj.SetActive(false);
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void Open()
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return;

        for (int i = 0; i < STAT_COUNT; i++)
        {
            savedLevels[i] = stats.GetStatLevel((StatType)i);
            plannedLevels[i] = savedLevels[i];
        }

        gameObject.SetActive(true);
        isOpen = true;

        Time.timeScale = 0f;

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshDisplay();
    }

    private void Close()
    {
        isOpen = false;
        gameObject.SetActive(false);

        Time.timeScale = 1f;

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void BuildUI()
    {
        GameObject panel = new GameObject("MenuPanel");
        panel.transform.SetParent(transform, false);

        RectTransform panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = new Vector2(500, 620);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        float y = -25f;

        soulLevelText = MakeText("SoulLevel", panel.transform, new Vector2(0, y), new Vector2(-40, 25));
        soulLevelText.fontSize = 22;
        soulLevelText.color = Color.white;
        soulLevelText.alignment = TextAlignmentOptions.Center;
        y -= 35f;

        currencyText = MakeText("Currency", panel.transform, new Vector2(0, y), new Vector2(-40, 25));
        currencyText.fontSize = 20;
        currencyText.color = new Color(1f, 0.85f, 0.3f);
        currencyText.alignment = TextAlignmentOptions.Center;
        y -= 45f;

        for (int i = 0; i < STAT_COUNT; i++)
        {
            BuildStatRow(i, panel.transform, y);
            y -= 70f;
        }

        totalCostText = MakeText("TotalCost", panel.transform, new Vector2(0, y), new Vector2(-40, 25));
        totalCostText.fontSize = 20;
        totalCostText.color = new Color(1f, 0.9f, 0.5f);
        totalCostText.alignment = TextAlignmentOptions.Center;
        y -= 40f;

        GameObject btnRow = new GameObject("Buttons");
        btnRow.transform.SetParent(panel.transform, false);
        RectTransform btnRt = btnRow.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0, 1);
        btnRt.anchorMax = new Vector2(1, 1);
        btnRt.pivot = new Vector2(0.5f, 1);
        btnRt.anchoredPosition = new Vector2(0, y);
        btnRt.sizeDelta = new Vector2(-40f, 40f);

        confirmButton = MakeButton(btnRow.transform, "ConfirmBtn", "Подтвердить", new Vector2(-170f, 0), new Vector2(100f, 35f));
        confirmButton.onClick.AddListener(OnConfirmClicked);

        resetButton = MakeButton(btnRow.transform, "ResetBtn", "Сброс", new Vector2(0f, 0), new Vector2(80f, 35f));
        resetButton.onClick.AddListener(OnResetClicked);

        closeButton = MakeButton(btnRow.transform, "CloseBtn", "Закрыть", new Vector2(170f, 0), new Vector2(80f, 35f));
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void BuildStatRow(int index, Transform parent, float yPos)
    {
        GameObject row = new GameObject($"StatRow_{index}");
        row.transform.SetParent(parent, false);

        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0, 1);
        rowRt.anchorMax = new Vector2(1, 1);
        rowRt.pivot = new Vector2(0, 1);
        rowRt.anchoredPosition = new Vector2(20f, yPos);
        rowRt.sizeDelta = new Vector2(-40f, 55f);

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(row.transform, false);
        RectTransform nameRt = nameObj.AddComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 0.6f);
        nameRt.anchorMax = new Vector2(0.5f, 1f);
        nameRt.offsetMin = Vector2.zero;
        nameRt.offsetMax = Vector2.zero;
        nameTexts[index] = nameObj.AddComponent<TextMeshProUGUI>();
        nameTexts[index].fontSize = 18;
        nameTexts[index].color = Color.white;
        nameTexts[index].alignment = TextAlignmentOptions.Left;
        nameTexts[index].raycastTarget = false;

        GameObject bonusObj = new GameObject("Bonus");
        bonusObj.transform.SetParent(row.transform, false);
        RectTransform bonusRt = bonusObj.AddComponent<RectTransform>();
        bonusRt.anchorMin = new Vector2(0, 0f);
        bonusRt.anchorMax = new Vector2(0.5f, 0.55f);
        bonusRt.offsetMin = Vector2.zero;
        bonusRt.offsetMax = Vector2.zero;
        bonusTexts[index] = bonusObj.AddComponent<TextMeshProUGUI>();
        bonusTexts[index].fontSize = 12;
        bonusTexts[index].color = new Color(0.7f, 0.7f, 0.7f);
        bonusTexts[index].alignment = TextAlignmentOptions.Left;
        bonusTexts[index].raycastTarget = false;

        GameObject levelObj = new GameObject("Level");
        levelObj.transform.SetParent(row.transform, false);
        RectTransform levelRt = levelObj.AddComponent<RectTransform>();
        levelRt.anchorMin = new Vector2(0.5f, 0.6f);
        levelRt.anchorMax = new Vector2(0.7f, 1f);
        levelRt.offsetMin = Vector2.zero;
        levelRt.offsetMax = Vector2.zero;
        levelTexts[index] = levelObj.AddComponent<TextMeshProUGUI>();
        levelTexts[index].fontSize = 18;
        levelTexts[index].color = Color.white;
        levelTexts[index].alignment = TextAlignmentOptions.Center;
        levelTexts[index].raycastTarget = false;

        minusButtons[index] = MakeButton(row.transform, "MinusBtn", "<", new Vector2(-60f, -20f), new Vector2(30f, 30f));
        int minusIdx = index;
        minusButtons[index].onClick.AddListener(() => OnMinusClicked(minusIdx));

        plusButtons[index] = MakeButton(row.transform, "PlusBtn", ">", new Vector2(60f, -20f), new Vector2(30f, 30f));
        int plusIdx = index;
        plusButtons[index].onClick.AddListener(() => OnPlusClicked(plusIdx));

        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(row.transform, false);
        RectTransform costRt = costObj.AddComponent<RectTransform>();
        costRt.anchorMin = new Vector2(0.72f, 0f);
        costRt.anchorMax = new Vector2(1f, 1f);
        costRt.offsetMin = Vector2.zero;
        costRt.offsetMax = Vector2.zero;
        costTexts[index] = costObj.AddComponent<TextMeshProUGUI>();
        costTexts[index].fontSize = 15;
        costTexts[index].color = new Color(1f, 0.85f, 0.3f);
        costTexts[index].alignment = TextAlignmentOptions.Right;
        costTexts[index].raycastTarget = false;
    }

    private TextMeshProUGUI MakeText(string name, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return obj.AddComponent<TextMeshProUGUI>();
    }

    private Button MakeButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.6f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.3f);
        btn.colors = colors;

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return btn;
    }

    private void OnPlusClicked(int index)
    {
        plannedLevels[index]++;
        RefreshDisplay();
    }

    private void OnMinusClicked(int index)
    {
        if (plannedLevels[index] > savedLevels[index])
        {
            plannedLevels[index]--;
            RefreshDisplay();
        }
    }

    private void OnConfirmClicked()
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return;

        int totalCost = CalculateTotalCost();
        if (stats.Currency < totalCost) return;

        for (int i = 0; i < STAT_COUNT; i++)
        {
            StatType type = (StatType)i;
            int current = stats.GetStatLevel(type);
            while (current < plannedLevels[i])
            {
                if (!stats.LevelUpStat(type)) break;
                current++;
            }
        }

        for (int i = 0; i < STAT_COUNT; i++)
            savedLevels[i] = stats.GetStatLevel((StatType)i);

        RefreshDisplay();
    }

    private void OnResetClicked()
    {
        for (int i = 0; i < STAT_COUNT; i++)
            plannedLevels[i] = savedLevels[i];
        RefreshDisplay();
    }

    private void OnCloseClicked()
    {
        Close();
    }

    private void RefreshDisplay()
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return;

        if (soulLevelText != null)
            soulLevelText.text = $"Уровень души: {stats.SoulLevel}";
        if (currencyText != null)
            currencyText.text = $"Души: {stats.Currency:N0}";

        int totalCost = 0;

        for (int i = 0; i < STAT_COUNT; i++)
        {
            int currentLevel = stats.GetStatLevel((StatType)i);
            int planned = plannedLevels[i];
            int diff = planned - currentLevel;

            if (nameTexts[i] != null)
                nameTexts[i].text = PlayerStats.StatNames[i];

            if (bonusTexts[i] != null)
                bonusTexts[i].text = PlayerStats.StatDescriptions[i];

            if (levelTexts[i] != null)
            {
                if (diff > 0)
                    levelTexts[i].text = $"{currentLevel} → <color=#FFD700>{planned}</color>";
                else
                    levelTexts[i].text = currentLevel.ToString();
            }

            int cost = CalculateStatCost(i);
            if (diff > 0) totalCost += cost;

            if (costTexts[i] != null)
                costTexts[i].text = diff > 0 ? cost.ToString() : "";

            if (minusButtons[i] != null)
                minusButtons[i].interactable = diff > 0;
        }

        if (totalCostText != null)
            totalCostText.text = totalCost > 0 ? $"Итого: {totalCost:N0} душ" : "";

        if (confirmButton != null)
            confirmButton.interactable = totalCost > 0 && stats.Currency >= totalCost;

        if (resetButton != null)
            resetButton.interactable = totalCost > 0;
    }

    private int CalculateTotalCost()
    {
        int total = 0;
        for (int i = 0; i < STAT_COUNT; i++)
            total += CalculateStatCost(i);
        return total;
    }

    private int CalculateStatCost(int statIndex)
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return 0;

        int current = stats.GetStatLevel((StatType)statIndex);
        int planned = plannedLevels[statIndex];
        if (planned <= current) return 0;

        int total = 0;
        int tempSoul = stats.SoulLevel;
        int tempStat = current;

        for (int j = current; j < planned; j++)
        {
            int soulAboveStart = tempSoul - stats.StartSoulLevel;
            int statAboveBase = tempStat - stats.BaseStatLevel;

            float cost = stats.BaseCurrencyCost
                * (1f + stats.SoulLevelCostRate * soulAboveStart)
                * (1f + stats.StatLevelCostRate * statAboveBase);

            total += Mathf.Max(1, Mathf.RoundToInt(cost));
            tempSoul++;
            tempStat++;
        }

        return total;
    }
}