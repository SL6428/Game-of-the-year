using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum StatType
{
    Strength = 0,
    Dexterity = 1,
    Vitality = 2,
    Resistance = 3,
    Defense = 4,
    Luck = 5
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public const int STAT_COUNT = 6;

    [Header("Base Settings")]
    [SerializeField] private int baseStatLevel = 8;
    [SerializeField] private int baseCurrencyCost = 100;
    [SerializeField, Range(0.01f, 0.15f)]
    private float soulLevelCostRate = 0.08f;
    [SerializeField, Range(0.01f, 0.08f)]
    private float statLevelCostRate = 0.03f;

    [Header("Stat Bonuses Per Level Above Base")]
    [SerializeField] private int strengthDamageBonus = 2;
    [SerializeField] private int dexterityStaminaBonus = 10;
    [SerializeField] private int vitalityHealthBonus = 10;
    [SerializeField] private int defenseDamageReduction = 1;
    [SerializeField] private int luckSoulBonus = 10;

    [Header("Stamina")]
    [SerializeField] private int baseMaxStamina = 100;

    [Header("Currency")]
    [SerializeField] private int startingCurrency = 0;

    private int[] statLevels;
    private int currency;

    public event Action<int> OnCurrencyChanged;
    public event Action OnStatsChanged;

    public int Currency => currency;
    public int SoulLevel
    {
        get
        {
            int total = 0;
            for (int i = 0; i < STAT_COUNT; i++)
                total += statLevels[i];
            return total;
        }
    }
    public int StartSoulLevel => baseStatLevel * STAT_COUNT;
    public int BaseStatLevel => baseStatLevel;
    public int BaseCurrencyCost => baseCurrencyCost;
    public float SoulLevelCostRate => soulLevelCostRate;
    public float StatLevelCostRate => statLevelCostRate;

    public int DamageBonus =>
        (statLevels[(int)StatType.Strength] - baseStatLevel) * strengthDamageBonus;
    public int StaminaBonus =>
        (statLevels[(int)StatType.Dexterity] - baseStatLevel) * dexterityStaminaBonus;
    public int HealthBonus =>
        (statLevels[(int)StatType.Vitality] - baseStatLevel) * vitalityHealthBonus;
    public int DefenseBonus =>
        (statLevels[(int)StatType.Defense] - baseStatLevel) * defenseDamageReduction;
    public int LuckBonus =>
        (statLevels[(int)StatType.Luck] - baseStatLevel) * luckSoulBonus;
    public int MaxStamina => baseMaxStamina + StaminaBonus;

    public static readonly string[] StatNames =
    {
        "Сила",
        "Ловкость",
        "Здоровье",
        "Сопротивление",
        "Защита",
        "Удача"
    };

    public static readonly string[] StatDescriptions =
    {
        "+2 к урону оружия",
        "+10 к макс. стамине",
        "+10 к макс. здоровью",
        "Пока не влияет",
        "-1 к получаемому урону",
        "+10 к получаемым душам"
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        statLevels = new int[STAT_COUNT];
        LoadStats();
    }

    void Start()
    {
        EnsureCurrencyUI();
        ApplyStatBonuses();
        SubscribePlayerDefense();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureCurrencyUI();
        ApplyStatBonuses();
        SubscribePlayerDefense();
    }

    private void SubscribePlayerDefense()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            health.OnModifyIncomingDamage -= ApplyDefense;
            health.OnModifyIncomingDamage += ApplyDefense;
        }
    }

    private void EnsureCurrencyUI()
    {
        CurrencyUI.EnsureExists();
    }

    public int GetStatLevel(StatType type)
    {
        return statLevels[(int)type];
    }

    public void SetStatLevel(StatType type, int level)
    {
        statLevels[(int)type] = Mathf.Max(baseStatLevel, level);
    }

    public int GetLevelUpCost(StatType type)
    {
        int currentLevel = statLevels[(int)type];
        int levelsAboveBase = currentLevel - baseStatLevel;
        int soulLevelsAboveStart = SoulLevel - StartSoulLevel;

        float cost = baseCurrencyCost
            * (1f + soulLevelCostRate * soulLevelsAboveStart)
            * (1f + statLevelCostRate * levelsAboveBase);

        return Mathf.Max(1, Mathf.RoundToInt(cost));
    }

    public bool CanAffordLevelUp(StatType type)
    {
        return currency >= GetLevelUpCost(type);
    }

    public bool LevelUpStat(StatType type)
    {
        int cost = GetLevelUpCost(type);
        if (currency < cost) return false;

        currency -= cost;
        statLevels[(int)type]++;
        SaveStats();
        ApplyStatBonuses();
        OnStatsChanged?.Invoke();
        OnCurrencyChanged?.Invoke(currency);
        return true;
    }

    public void AddCurrency(int amount)
    {
        int modified = amount + LuckBonus;
        currency += modified;
        OnCurrencyChanged?.Invoke(currency);
    }

    public float ModifyIncomingDamage(float damage)
    {
        return Mathf.Max(1f, damage - DefenseBonus);
    }

    private float ApplyDefense(float damage)
    {
        return Mathf.Max(1f, damage - DefenseBonus);
    }

    public void ApplyStatBonuses()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            float newMax = health.BaseMaxHealth + HealthBonus;
            health.SetMaxHealth(newMax);
        }

        Stamina stamina = player.GetComponent<Stamina>();
        if (stamina != null)
        {
            float newMaxStam = baseMaxStamina + StaminaBonus;
            stamina.SetMaxStamina(newMaxStam);
        }
    }

    public void SaveStats()
    {
        PlayerPrefs.SetInt("PS_Currency", currency);
        for (int i = 0; i < STAT_COUNT; i++)
            PlayerPrefs.SetInt($"PS_Stat_{i}", statLevels[i]);
        PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        currency = PlayerPrefs.GetInt("PS_Currency", startingCurrency);

        bool hasSaved = PlayerPrefs.HasKey("PS_Stat_0");
        for (int i = 0; i < STAT_COUNT; i++)
        {
            if (hasSaved)
                statLevels[i] = PlayerPrefs.GetInt($"PS_Stat_{i}", baseStatLevel);
            else
                statLevels[i] = baseStatLevel;
        }

        OnCurrencyChanged?.Invoke(currency);
    }
}
