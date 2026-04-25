using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaUI : MonoBehaviour
{
    [Header("UI References - ASSIGN THESE IN INSPECTOR")]
    [SerializeField] private Image staminaFillImage;
    [SerializeField] private TextMeshProUGUI staminaText;

    [Header("Colors")]
    [SerializeField] private Color fullColor = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color lowColor = new Color(0.9f, 0.7f, 0.1f);
    [SerializeField] private float lowThreshold = 0.3f;

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 8f;

    private Stamina stamina;
    private float currentFill;
    private float targetFill;
    private bool subscribed;

    void Start()
    {
        TryConnect();
    }

    void OnEnable()
    {
        TryConnect();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    void Update()
    {
        if (stamina == null)
        {
            TryConnect();
            return;
        }

        if (Mathf.Abs(currentFill - targetFill) > 0.001f)
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFill, Time.unscaledDeltaTime * smoothSpeed);
            if (staminaFillImage != null)
                staminaFillImage.fillAmount = currentFill;
            UpdateColor(currentFill);
        }
    }

    private void TryConnect()
    {
        if (stamina != null) return;

        stamina = FindFirstObjectByType<Stamina>();
        if (stamina == null) return;

        if (!subscribed)
        {
            stamina.OnStaminaChanged += OnStaminaChanged;
            subscribed = true;
        }

        currentFill = stamina.CurrentStamina / stamina.MaxStamina;
        targetFill = currentFill;
        UpdateVisual(currentFill);

        EnsureFillImageSetup();
    }

    private void EnsureFillImageSetup()
    {
        if (staminaFillImage == null) return;

        staminaFillImage.type = Image.Type.Filled;
        staminaFillImage.fillMethod = Image.FillMethod.Horizontal;
        staminaFillImage.fillOrigin = 0;
        staminaFillImage.fillAmount = currentFill;
        UpdateColor(currentFill);
    }

    private void Unsubscribe()
    {
        if (stamina != null && subscribed)
        {
            stamina.OnStaminaChanged -= OnStaminaChanged;
            subscribed = false;
        }
    }

    private void OnStaminaChanged(float current, float max)
    {
        targetFill = current / max;
        if (staminaText != null)
            staminaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateVisual(float fill)
    {
        if (staminaFillImage != null)
            staminaFillImage.fillAmount = fill;
        UpdateColor(fill);
    }

    private void UpdateColor(float fill)
    {
        if (staminaFillImage == null) return;
        staminaFillImage.color = fill <= lowThreshold ? lowColor : fullColor;
    }
}
