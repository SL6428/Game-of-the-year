using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelUpMenu : MonoBehaviour
{
    public static LevelUpMenu Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance._isOpen;
    public static bool JustClosed { get; private set; }

    private CanvasGroup cg;
    private bool _isOpen;
    private bool built;
    private bool buttonsBound;

    private TextMeshProUGUI soulLevelText;
    private TextMeshProUGUI currencyText;
    private TextMeshProUGUI totalCostText;

    private TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[6];
    private TextMeshProUGUI[] bonusTexts = new TextMeshProUGUI[6];
    private TextMeshProUGUI[] levelTexts = new TextMeshProUGUI[6];
    private TextMeshProUGUI[] costTexts = new TextMeshProUGUI[6];
    private Button[] plusButtons = new Button[6];
    private Button[] minusButtons = new Button[6];
    private Button confirmButton;
    private Button resetButton;
    private Button closeButton;

    private int[] planned = new int[6];
    private int[] saved = new int[6];
    private const int MAX_LVL = 99;

    void Awake()
    {
        Instance = this;
        cg = GetComponent<CanvasGroup>();
        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        _isOpen = false;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_isOpen) RestoreGame();
    }

    void Update()
    {
        if (JustClosed)
        {
            JustClosed = false;
            return;
        }
        if (!_isOpen) return;
        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

    public void Open()
    {
        var s = PlayerStats.Instance;
        if (s == null)
        {
            Debug.LogError("[LevelUpMenu] PlayerStats.Instance is NULL!");
            return;
        }

        if (!built)
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt == null) rt = GetComponentInChildren<RectTransform>(true);
            if (rt != null)
                Build(rt);
            else
            {
                Debug.LogError("[LevelUpMenu] No RectTransform found!");
                return;
            }
        }

        for (int i = 0; i < 6; i++)
        {
            saved[i] = s.GetStatLevel((StatType)i);
            planned[i] = saved[i];
        }

        if (!buttonsBound)
        {
            BindButtons();
            buttonsBound = true;
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        _isOpen = true;

        Time.timeScale = 0f;
        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Refresh();
    }

    public void Hide()
    {
        _isOpen = false;
        JustClosed = true;
        RestoreGame();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private void RestoreGame()
    {
        Time.timeScale = 1f;
        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Build(RectTransform panel)
    {
        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            Transform child = panel.GetChild(i);
            if (child.GetComponent<Image>() != null && child.GetComponent<Button>() == null)
                continue;
            Destroy(child.gameObject);
        }

        float y = -25f;

        soulLevelText = MakeText("SoulLevel", panel, new Vector2(0, y), new Vector2(-40, 25));
        soulLevelText.fontSize = 22;
        soulLevelText.color = Color.white;
        soulLevelText.alignment = TextAlignmentOptions.Center;
        y -= 35f;

        currencyText = MakeText("Currency", panel, new Vector2(0, y), new Vector2(-40, 25));
        currencyText.fontSize = 20;
        currencyText.color = new Color(1f, 0.85f, 0.3f);
        currencyText.alignment = TextAlignmentOptions.Center;
        y -= 45f;

        for (int i = 0; i < 6; i++)
        {
            BuildRow(i, panel, y);
            y -= 70f;
        }

        totalCostText = MakeText("TotalCost", panel, new Vector2(0, y), new Vector2(-40, 25));
        totalCostText.fontSize = 20;
        totalCostText.color = new Color(1f, 0.9f, 0.5f);
        totalCostText.alignment = TextAlignmentOptions.Center;
        y -= 40f;

        GameObject btnRow = new GameObject("Buttons");
        btnRow.transform.SetParent(panel, false);
        RectTransform btnRt = btnRow.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0, 1);
        btnRt.anchorMax = new Vector2(1, 1);
        btnRt.pivot = new Vector2(0.5f, 1);
        btnRt.anchoredPosition = new Vector2(0, y);
        btnRt.sizeDelta = new Vector2(-40f, 40f);

        confirmButton = MakeButton(btnRow.transform, "ConfirmBtn", "Confirm", new Vector2(-150f, 0), new Vector2(110f, 35f));
        resetButton = MakeButton(btnRow.transform, "ResetBtn", "Reset", new Vector2(0f, 0), new Vector2(80f, 35f));
        closeButton = MakeButton(btnRow.transform, "CloseBtn", "Close", new Vector2(150f, 0), new Vector2(80f, 35f));

        built = true;
    }

    private void BuildRow(int idx, Transform parent, float yPos)
    {
        GameObject row = new GameObject($"StatRow_{idx}");
        row.transform.SetParent(parent, false);

        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20f, yPos);
        rt.sizeDelta = new Vector2(-40f, 55f);

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(row.transform, false);
        RectTransform nRt = nameObj.AddComponent<RectTransform>();
        nRt.anchorMin = new Vector2(0, 0.6f);
        nRt.anchorMax = new Vector2(0.5f, 1f);
        nRt.offsetMin = Vector2.zero;
        nRt.offsetMax = Vector2.zero;
        nameTexts[idx] = nameObj.AddComponent<TextMeshProUGUI>();
        nameTexts[idx].fontSize = 18;
        nameTexts[idx].color = Color.white;
        nameTexts[idx].alignment = TextAlignmentOptions.Left;
        nameTexts[idx].raycastTarget = false;

        GameObject bonusObj = new GameObject("Bonus");
        bonusObj.transform.SetParent(row.transform, false);
        RectTransform bRt = bonusObj.AddComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 0f);
        bRt.anchorMax = new Vector2(0.5f, 0.55f);
        bRt.offsetMin = Vector2.zero;
        bRt.offsetMax = Vector2.zero;
        bonusTexts[idx] = bonusObj.AddComponent<TextMeshProUGUI>();
        bonusTexts[idx].fontSize = 12;
        bonusTexts[idx].color = new Color(0.7f, 0.7f, 0.7f);
        bonusTexts[idx].alignment = TextAlignmentOptions.Left;
        bonusTexts[idx].raycastTarget = false;

        GameObject lvlObj = new GameObject("Level");
        lvlObj.transform.SetParent(row.transform, false);
        RectTransform lRt = lvlObj.AddComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0.5f, 0.6f);
        lRt.anchorMax = new Vector2(0.7f, 1f);
        lRt.offsetMin = Vector2.zero;
        lRt.offsetMax = Vector2.zero;
        levelTexts[idx] = lvlObj.AddComponent<TextMeshProUGUI>();
        levelTexts[idx].fontSize = 18;
        levelTexts[idx].color = Color.white;
        levelTexts[idx].alignment = TextAlignmentOptions.Center;
        levelTexts[idx].raycastTarget = false;

        minusButtons[idx] = MakeButton(row.transform, "MinusBtn", "<", new Vector2(-60f, -20f), new Vector2(30f, 30f));
        plusButtons[idx] = MakeButton(row.transform, "PlusBtn", ">", new Vector2(60f, -20f), new Vector2(30f, 30f));

        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(row.transform, false);
        RectTransform cRt = costObj.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.72f, 0f);
        cRt.anchorMax = new Vector2(1f, 1f);
        cRt.offsetMin = Vector2.zero;
        cRt.offsetMax = Vector2.zero;
        costTexts[idx] = costObj.AddComponent<TextMeshProUGUI>();
        costTexts[idx].fontSize = 15;
        costTexts[idx].color = new Color(1f, 0.85f, 0.3f);
        costTexts[idx].alignment = TextAlignmentOptions.Right;
        costTexts[idx].raycastTarget = false;
    }

    private TextMeshProUGUI MakeText(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return obj.AddComponent<TextMeshProUGUI>();
    }

    private Button MakeButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.5f, 0.5f, 0.6f);
        cb.pressedColor = new Color(0.2f, 0.2f, 0.3f);
        btn.colors = cb;

        GameObject lbl = new GameObject("Label");
        lbl.transform.SetParent(obj.transform, false);
        RectTransform lRt = lbl.AddComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero;
        lRt.anchorMax = Vector2.one;
        lRt.offsetMin = Vector2.zero;
        lRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return btn;
    }

    private void BindButtons()
    {
        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            if (plusButtons[i] != null)
                plusButtons[i].onClick.AddListener(() => OnPlus(idx));
            if (minusButtons[i] != null)
                minusButtons[i].onClick.AddListener(() => OnMinus(idx));
        }
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (resetButton != null) resetButton.onClick.AddListener(OnReset);
        if (closeButton != null) closeButton.onClick.AddListener(OnClose);
    }

    private void OnPlus(int i) { if (planned[i] < MAX_LVL) { planned[i]++; Refresh(); } }
    private void OnMinus(int i) { if (planned[i] > saved[i]) { planned[i]--; Refresh(); } }

    private void OnConfirm()
    {
        var s = PlayerStats.Instance;
        if (s == null)
        {
            Debug.LogError("[LevelUpMenu] OnConfirm: PlayerStats is NULL!");
            return;
        }

        int total = CalcTotalCost();
        Debug.Log($"[LevelUpMenu] OnConfirm: currency={s.Currency}, totalCost={total}, planned=[{planned[0]},{planned[1]},{planned[2]},{planned[3]},{planned[4]},{planned[5]}]");

        if (s.Currency < total)
        {
            Debug.LogWarning($"[LevelUpMenu] Not enough currency! Have {s.Currency}, need {total}");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            StatType t = (StatType)i;
            int cur = s.GetStatLevel(t);
            while (cur < planned[i])
            {
                bool ok = s.LevelUpStat(t);
                Debug.Log($"[LevelUpMenu] LevelUpStat({t}): cur={cur}, planned={planned[i]}, success={ok}");
                if (!ok) break;
                cur++;
            }
        }
        for (int i = 0; i < 6; i++)
            saved[i] = s.GetStatLevel((StatType)i);
        Refresh();
    }

    private void OnReset()
    {
        for (int i = 0; i < 6; i++) planned[i] = saved[i];
        Refresh();
    }

    private void OnClose() { Hide(); }

    private void Refresh()
    {
        var s = PlayerStats.Instance;
        if (s == null) return;

        if (soulLevelText != null)
            soulLevelText.text = $"Soul Level: {s.SoulLevel}";
        if (currencyText != null)
            currencyText.text = $"Souls: {s.Currency:N0}";

        int totalCost = 0;

        for (int i = 0; i < 6; i++)
        {
            int cur = s.GetStatLevel((StatType)i);
            int pln = planned[i];
            int diff = pln - cur;

            if (nameTexts[i] != null)
                nameTexts[i].text = PlayerStats.StatNames[i];

            if (bonusTexts[i] != null)
                bonusTexts[i].text = PlayerStats.StatDescriptions[i];

            if (levelTexts[i] != null)
                levelTexts[i].text = diff > 0
                    ? $"{cur} -> <color=#FFD700>{pln}</color>"
                    : cur.ToString();

            int statCost = 0, ts = s.SoulLevel, tl = cur;
            for (int j = cur; j < pln; j++)
            {
                float c = s.BaseCurrencyCost
                    * (1f + s.SoulLevelCostRate * (ts - s.StartSoulLevel))
                    * (1f + s.StatLevelCostRate * (tl - s.BaseStatLevel));
                int r = Mathf.Max(1, Mathf.RoundToInt(c));
                statCost += r; totalCost += r; ts++; tl++;
            }

            if (costTexts[i] != null)
                costTexts[i].text = diff > 0 ? $"{statCost}" : "";

            if (plusButtons[i] != null)
                plusButtons[i].interactable = pln < MAX_LVL;
            if (minusButtons[i] != null)
                minusButtons[i].interactable = diff > 0;
        }

        if (totalCostText != null)
            totalCostText.text = totalCost > 0 ? $"Total: {totalCost} souls" : "";

        if (confirmButton != null)
            confirmButton.interactable = totalCost > 0 && s.Currency >= totalCost;

        if (resetButton != null)
            resetButton.interactable = totalCost > 0;
    }

    private int CalcTotalCost()
    {
        var s = PlayerStats.Instance;
        if (s == null) return 0;
        int total = 0, ts = s.SoulLevel;
        for (int i = 0; i < 6; i++)
        {
            int cur = s.GetStatLevel((StatType)i), tl = cur;
            for (int j = cur; j < planned[i]; j++)
            {
                float c = s.BaseCurrencyCost
                    * (1f + s.SoulLevelCostRate * (ts - s.StartSoulLevel))
                    * (1f + s.StatLevelCostRate * (tl - s.BaseStatLevel));
                total += Mathf.Max(1, Mathf.RoundToInt(c)); ts++; tl++;
            }
        }
        return total;
    }
}

