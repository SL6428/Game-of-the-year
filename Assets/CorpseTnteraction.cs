using UnityEngine;
using UnityEngine.UI;

public class CorpseInteraction : MonoBehaviour
{
    public GameObject uiText;
    public string weaponName = "Rusty Dagger";
    private bool playerInRange = false;

    void Start()
    {
        uiText.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Получено оружие: " + weaponName);
            uiText.SetActive(false);
            // Здесь можно добавить выдачу оружия игроку
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            uiText.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            uiText.SetActive(false);
        }
    }
}