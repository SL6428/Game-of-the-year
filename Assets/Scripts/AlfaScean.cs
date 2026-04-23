using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class AlfaScean : MonoBehaviour
{
    public GameObject AlfaPanel;

    public void QuitCancelClicked()
    {
        AlfaPanel.SetActive(false);
    }
}
