using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineSensitivity : MonoBehaviour
{
    [System.Serializable]
    public class AxisGain
    {
        public string axisName;
        public float baseGain = 1f;
        public float baseLegacyGain = 1f;
    }

    [Header("Base Input Gains (auto-captured on Start)")]
    [SerializeField] private List<AxisGain> axisGains = new List<AxisGain>();

    private CinemachineInputAxisController inputController;
    private bool initialized;

    void Start()
    {
        inputController = GetComponent<CinemachineInputAxisController>();
        if (inputController == null)
            inputController = GetComponentInChildren<CinemachineInputAxisController>();

        if (inputController != null)
        {
            CaptureBaseGains();
            initialized = true;
        }
        else
        {
            Debug.LogWarning(
                $"CinemachineSensitivity: CinemachineInputAxisController не найден на {gameObject.name}");
        }

        if (SettingsManager.Instance != null)
            ApplySensitivity(SettingsManager.Instance.CameraSensitivity);
    }

    private void CaptureBaseGains()
    {
        axisGains.Clear();

        foreach (var controller in inputController.Controllers)
        {
            if (controller.Input is CinemachineInputAxisController.Reader reader)
            {
                axisGains.Add(new AxisGain
                {
                    axisName = controller.Name,
                    baseGain = reader.Gain,
                    baseLegacyGain = reader.LegacyGain
                });
            }
        }
    }

    public void ApplySensitivity(float multiplier)
    {
        if (!initialized || inputController == null) return;

        for (int i = 0; i < axisGains.Count && i < inputController.Controllers.Count; i++)
        {
            var controller = inputController.Controllers[i];
            if (controller.Input is CinemachineInputAxisController.Reader reader)
            {
                reader.Gain = axisGains[i].baseGain * multiplier;
                reader.LegacyGain = axisGains[i].baseLegacyGain * multiplier;
            }
        }
    }
}
