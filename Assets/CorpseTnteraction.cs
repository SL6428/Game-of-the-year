using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Взаимодействие с трупами врагов.
/// Подбор оружия с тел.
/// </summary>
public class CorpseInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject uiText;

    [Header("Loot")]
    [SerializeField] private string weaponName = "Rusty Dagger";

    private bool playerInRange = false;

    void Start()
    {
        if (uiText != null)
        {
            uiText.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Подобрано оружие: " + weaponName);
            
            if (uiText != null)
            {
                uiText.SetActive(false);
            }
            
            // Здесь будет логика подбора оружия
            // Например: добавить оружие в инвентарь игрока
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (uiText != null)
            {
                uiText.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (uiText != null)
            {
                uiText.SetActive(false);
            }
        }
    }
}
