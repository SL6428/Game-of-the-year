using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Объект для подбора Estus Flask (фляги с хилом).
/// Подойдя и нажав E — игрок получает флягу навсегда.
/// </summary>
public class EstusPickup : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string promptMessage = "Press E to pick up Estus Flask";

    [Header("UI References (optional)")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    private Transform player;
    private bool isPlayerInRange;
    private TextMeshProUGUI autoPrompt;

    void Update()
    {
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
            Pickup();
        }
    }

    private void Pickup()
    {
        var regen = FindFirstObjectByType<PlayerRegeneration>();
        if (regen != null)
            regen.EnableFlask();
        else
            Debug.LogWarning("[EstusPickup] PlayerRegeneration not found!");

        HidePrompt();
        Destroy(gameObject);
    }

    private void ShowPrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            if (promptText != null) promptText.text = promptMessage;
            return;
        }

        if (autoPrompt != null)
        {
            autoPrompt.gameObject.SetActive(true);
            return;
        }

        GameObject c = new GameObject("EstusPrompt");
        Canvas cv = c.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 60;
        c.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject t = new GameObject("Txt");
        t.transform.SetParent(c.transform, false);
        RectTransform rt = t.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 60);
        rt.sizeDelta = new Vector2(260, 22);

        autoPrompt = t.AddComponent<TextMeshProUGUI>();
        autoPrompt.text = promptMessage;
        autoPrompt.fontSize = 14;
        autoPrompt.color = new Color(1f, 0.9f, 0.5f);
        autoPrompt.alignment = TextAlignmentOptions.Center;
        autoPrompt.raycastTarget = false;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
