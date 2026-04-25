using UnityEngine;
using TMPro;

public class LevelUpShrine : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string promptMessage = "Нажмите E для молитвы";

    [Header("Prompt (optional)")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    private Transform player;
    private bool isPlayerInRange;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (promptPanel != null)
            promptPanel.SetActive(false);
        if (promptText != null)
            promptText.text = promptMessage;
    }

    void Update()
    {
        if (player == null) return;

        if (LevelUpMenu.IsOpen)
        {
            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape))
                LevelUpMenu.Hide();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRadius;

        if (isPlayerInRange && !wasInRange && promptPanel != null)
            promptPanel.SetActive(true);

        if (!isPlayerInRange && wasInRange && promptPanel != null)
            promptPanel.SetActive(false);

        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            LevelUpMenu.Show();
            if (promptPanel != null) promptPanel.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}