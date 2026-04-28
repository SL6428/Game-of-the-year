using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    // === НАСТРОЙКИ СМЕРТИ — меняйте прямо здесь в коде ===
    private string deathMessage = "\u0412\u044B \u043F\u043E\u0433\u0438\u0431\u043B\u0438";
    private float animWait = 1f;          // сколько ждать анимации смерти
    private float textFadeIn = 1.5f;      // появление текста (сек)
    private float textHold = 1f;          // задержка текста
    private float screenFade = 3f;      // затемнение экрана (сек)
    private float respawnHealthPercent = 0.35f; // HP после респавна (0.35 = 35%)
    private int respawnCharges = 2;       // зарядов фляги после смерти

    private CanvasGroup overlayGroup;
    private TextMeshProUGUI deathText;
    private Image blackScreen;
    private bool isRunning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BuildOverlay()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        overlayGroup = gameObject.GetComponent<CanvasGroup>();
        if (overlayGroup == null)
            overlayGroup = gameObject.AddComponent<CanvasGroup>();

        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;

        GameObject bg = new GameObject("BlackScreen");
        bg.transform.SetParent(transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        blackScreen = bg.AddComponent<Image>();
        blackScreen.color = Color.black;
        blackScreen.raycastTarget = false;

        GameObject txt = new GameObject("DeathText");
        txt.transform.SetParent(transform, false);
        RectTransform tRt = txt.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0.4f);
        tRt.anchorMax = new Vector2(1, 0.6f);
        tRt.offsetMin = Vector2.zero;
        tRt.offsetMax = Vector2.zero;

        deathText = txt.AddComponent<TextMeshProUGUI>();
        deathText.fontSize = 48;
        deathText.color = new Color(0.85f, 0.1f, 0.1f, 0f);
        deathText.alignment = TextAlignmentOptions.Center;
        deathText.raycastTarget = false;
        deathText.fontStyle = FontStyles.Bold;
        deathText.text = deathMessage;
    }

    public void ShowDeathScreen(Vector3 respawnPos, Health health, PlayerRegeneration regen, PlayerController pc)
    {
        if (isRunning) return;
        StartCoroutine(DeathSequence(respawnPos, health, regen, pc));
    }

    private IEnumerator DeathSequence(Vector3 respawnPos, Health health, PlayerRegeneration regen, PlayerController pc)
    {
        isRunning = true;

        overlayGroup.alpha = 1f;
        overlayGroup.blocksRaycasts = true;
        blackScreen.color = new Color(0, 0, 0, 0);
        deathText.color = new Color(0.85f, 0.1f, 0.1f, 0f);

        yield return new WaitForSeconds(animWait);

        float t = 0f;
        while (t < textFadeIn)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / textFadeIn);
            deathText.color = new Color(0.85f, 0.1f, 0.1f, a);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(textHold);

        t = 0f;
        while (t < screenFade)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / screenFade);
            blackScreen.color = new Color(0, 0, 0, a);
            deathText.color = new Color(0.85f, 0.1f, 0.1f, 1f - a);
            yield return null;
        }

        blackScreen.color = Color.black;
        deathText.color = new Color(0.85f, 0.1f, 0.1f, 0f);

        yield return new WaitForSecondsRealtime(0.5f);

        RespawnPlayer(respawnPos, health, regen, pc);

        t = 0f;
        while (t < screenFade)
        {
            t += Time.unscaledDeltaTime;
            float a = 1f - Mathf.Clamp01(t / screenFade);
            blackScreen.color = new Color(0, 0, 0, a);
            yield return null;
        }

        blackScreen.color = new Color(0, 0, 0, 0);
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;

        if (pc != null)
            pc.ChangeState(new LocomotionState(pc));

        isRunning = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        GameObject obj = new GameObject("DeathManagerCanvas");
        DontDestroyOnLoad(obj);
        obj.AddComponent<DeathManager>();
    }

    private void RespawnPlayer(Vector3 pos, Health health, PlayerRegeneration regen, PlayerController pc)
    {
        // Телепорт (контроллер остаётся ВЫКЛЮЧЕННЫМ — включится только после полного прояснения экрана)
        if (pc != null && pc.controller != null)
        {
            pc.controller.enabled = false;
            pc.transform.position = pos;
        }

        if (health != null)
        {
            health.Revive(health.MaxHealth * respawnHealthPercent);
        }

        if (regen != null && regen.HasFlask)
        {
            regen.SetChargesWithQueue(respawnCharges);
            regen.ApplyDeathPenalty();
        }

        var arena = Object.FindFirstObjectByType<BossArena>();
        arena?.ResetArenaAndHealBoss();
    }
}
