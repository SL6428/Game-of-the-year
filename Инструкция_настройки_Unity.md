# Инструкция по настройке Unity-проекта

## 1. Теги (Tags)

Edit → Project Settings → Tags and Layers → Tags:

| Тег | Назначение |
|-----|-----------|
| `Player` | Объект игрока. Обязателен для PlayerStats, LevelUpShrine, Weapon, BossArena, GameManager |
| `Enemy` | Объекты врагов. Обязателен для Weapon (определение цели) |

---

## 2. Сцены (Build Settings)

File → Build Settings — порядок сцен:

| Индекс | Имя сцены | Назначение |
|--------|-----------|-----------|
| 0 | `Sinematic` | Главное меню / кат-сцена |
| 1 | `mp_First lvl` | Игровой уровень |

В `MainMenuManager.StartNewGame()` вызывается `PlayerPrefs.DeleteAll()` и загрузка `mp_First lvl`. Убедитесь что имена сцен совпадают.

---

## 3. Игрок (GameObject с тегом Player)

Объект игрока должен содержать **все** компоненты ниже на одном GameObject:

| Компонент | Настройки |
|-----------|----------|
| `CharacterController` | Skin Width: 0.01, Center: (0, 0.9, 0), Height: 1.8, Radius: 0.3 |
| `PlayerController` | Walk Speed: 3, Run Speed: 6, Rotation Speed: 10, Gravity: -9.81, Jump Height: 1.2 |
| `Health` | Base Max Health: 100, Max Health: 100, Current Health: -1 |
| `Stamina` | Max Stamina: 100, Regen Rate: 30, Regen Delay: 0.5, Run Drain: 15, Attack: 20, Roll: 15, Jump: 10 |
| `PlayerRegeneration` | Настраивается отдельно (заряды лечения) |
| `Weapon` | Damage: 20, Enemy Layers: слой врагов, Weapon Collider: коллайдер оружия (Is Trigger = true) |
| `Animator` | Контроллер анимации с параметрами: Speed(float), VelocityX(float), VelocityZ(float), IsGrounded(bool), IsRunning(bool), триггеры: Jump, Roll, Attack, Heal |

**Важное**: Weapon Collider (на дочернем объекте оружия) обязан быть `Is Trigger = true` и по умолчанию `Enabled = false`. Hitbox управляется через Animation Events: `EnableHitbox` / `DisableHitbox`.

### Дочерние объекты игрока

- **CameraPivot** — объект с компонентом `CameraPivot`, дочерний к игроку или отдельный в сцене
- **LockOnSystem** — назначить в инспектор PlayerController
- **Оружие** — дочерний объект с Collider (Is Trigger), назначить в `Weapon.weaponCollider`

---

## 4. UI здоровья и стамины (в сцене уровня)

Создать Canvas (Screen Space — Overlay) и добавить:

### HealthUI
- Image (Type: Filled, Method: Horizontal, Origin: Left) → назначить в `hpFillImage`
- TextMeshPro — Text → назначить в `hpText`
- `playerHealth` — перетащить Health игрока (или автонайдётся)

### StaminaUI
- Image (Type: Filled, Method: Horizontal, Origin: Left) → назначить в `staminaFillImage`
- TextMeshPro — Text → назначить в `staminaText`
- Автоподключение к Stamina через `FindFirstObjectByType`

---

## 5. Счётчик душ (CurrencyUI)

**НЕ создавать вручную в сцене.** Скрипт `CurrencyUI` автоматически создаёт свой Canvas через `[RuntimeInitializeOnLoadMethod]` при запуске игры. Если добавить CurrencyUI в сцену — будет дубликат.

---

## 6. PlayerStats (синглтон)

**НЕ создавать вручную в игровой сцене.** PlayerStats создаётся в сцене `Sinematic` (главное меню) и переходит между сценами через `DontDestroyOnLoad`.

Если по какой-то причине нужно добавить — только в сцену `Sinematic`, на пустой GameObject без других компонентов.

### PlayerPrefs ключи (сохранение)

| Ключ | Тип | Значение |
|------|-----|----------|
| `PS_Currency` | Int | Количество душ |
| `PS_Stat_0` | Int | Уровень Силы |
| `PS_Stat_1` | Int | Уровень Ловкости |
| `PS_Stat_2` | Int | Уровень Здоровья |
| `PS_Stat_3` | Int | Уровень Сопротивления |
| `PS_Stat_4` | Int | Уровень Защиты |
| `PS_Stat_5` | Int | Уровень Удачи |
| `Settings_*` | Разные | Настройки из SettingsManager |

---

## 7. Алтарь прокачки (LevelUpShrine)

Разместить в сцене уровня на пустом GameObject:

| Параметр | Значение по умолчанию | Описание |
|----------|----------------------|----------|
| Interaction Radius | 3 | Радиус взаимодействия |
| Interact Key | E | Клавиша открытия |
| Prompt Message | "Нажмите E для молитвы" | Текст подсказки |
| Prompt Panel | (опционально) | Ссылка на панель-подсказку из сцены |
| Prompt Text | (опционально) | Ссылка на текст в панели-подсказке |

Если Prompt Panel не назначен — подсказка создаётся автоматически в нижней части экрана.

Меню прокачки (LevelUpMenu) **НЕ** нужно добавлять в сцену — оно создаётся программно при вызове `LevelUpMenu.Show()`.

---

## 8. Враги (GameObject с тегом Enemy)

Каждый враг должен содержать:

| Компонент | Назначение |
|-----------|-----------|
| `Health` | HP врага |
| `CurrencyDrop` | Soul Value: 50 (количество душ за убийство) |
| `EnemyAI` | ИИ врага |
| `EnemyAnimator` | Анимация |
| Collider | Для получения урона от оружия |
| Rigidbody | Если используется физика |

---

## 9. Босс-арена (BossArena)

На объекте-триггере арены:

- `BossArena` компонент
- Walls: массив ссылок на объекты-стены с Collider (не SetActive — управляется через `Collider.enabled`)
- Boss Health: ссылка на Health босса
- Trigger Collider: Is Trigger = true, для входа игрока

---

## 10. TextMesh Pro

Window → TextMeshPro → Import TMP Essentials:

- **Default Font Asset** — должен быть назначен в TMP Settings. Скрипты `CurrencyUI` и `LevelUpMenu` используют `TMP_Settings.defaultFontAsset` для программно-созданного текста
- Без этого программный UI будет невидимым

Проверить: Edit → Project Settings → TextMeshPro → Default Font Asset — не должен быть None.

---

## 11. Input System

Если установлен New Input System (пакет `com.unity.inputsystem`):

- Edit → Project Settings → Player → Active Input Handling = **Both** или **Input System Package (New)**
- `LevelUpMenu` автоматически создаст `InputSystemUIInputModule` если пакет доступен
- Если стоит **Both** — старый `StandaloneInputModule` тоже работает

---

## 12. DontDestroyOnLoad объекты

При запуске из сцены `Sinematic` в DontDestroyOnLoad переходят:

| Объект | Содержит |
|--------|----------|
| PlayerStats | Синглтон статов, валюты, бонусов |
| CurrencyCanvas | CurrencyUI (счётчик душ) |
| GameManager | Пауза, ссылка на игрока |
| SettingsManager | Настройки |
| SimpleInventory | Инвентарь |

**Порядок создания**: CurrencyUI создаётся раньше всех через `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`, PlayerStats — из сцены Sinematic.

---

## 13. Частые ошибки и проверки

| Симптом | Причина | Решение |
|---------|---------|---------|
| Текст душ не виден | TMP Default Font не назначен | Импортировать TMP Essentials, назначить шрифт |
| 3 счётчика душ | CurrencyUI добавлен в сцену вручную | Удалить из сцены, оставить автосоздание |
| Кнопки не нажимаются | Отсутствует EventSystem или конфликт Input System | Проверить EventSystem на сцене; переключить Active Input Handling |
| Прокачка не применяется | PlayerStats.Instance = null | Убедиться что PlayerStats есть в Sinematic сцене |
| Души не зачисляются | У врага нет CurrencyDrop или тег не Enemy | Добавить CurrencyDrop, проверить тег |
| Escape конфликтует | LevelUpMenu и GameMenu реагируют одновременно | Проверить что GameManager и GameMenu содержат `if (LevelUpMenu.IsOpen) return;` |
| Урон не проходит | Weapon Collider не Is Trigger или включён всегда | Is Trigger = true, Enabled = false по умолчанию |
| Защита не работает | Health.OnModifyIncomingDamage не подписан | PlayerStats.SubscribePlayerDefense() вызывается из Start/OnSceneLoaded |
