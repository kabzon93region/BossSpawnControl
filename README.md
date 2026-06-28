# Boss Spawn Control

**GitHub:** [kabzon93region](https://github.com/kabzon93region)

Клиентский мод для SPT 4 / Fika: управление спавном боссов через **F12 (Configuration Manager)**.

## v1.1.0 — F12 + кнопка отладки

### Общие

- **ModEnabled** — при старте рейда включённым боссам ставится `BossChance=100` (если волна есть на карте).
- **DebugLogging** — подробные логи `[BOSS_SPAWN]`.

### Боссы

Полный список боссов игры — **отдельная строка (bool)** на каждого. Включите нужных боссов (например `Killa (bossKilla)`).

### Отладка

Кнопка **«Заспавнить включённых боссов»**:

- работает **в рейде**;
- спавнит всех **включённых** боссов на текущей карте;
- **принудительно**, даже если босс уже был и убит;
- работает **даже если ModEnabled = false**;
- пишет **детальный лог** каждого шага в `LogOutput.log`.

## Почему v1.0.0 не работал

1. Конфиг был через строки `ForceSpawn=` — если пусто, мод ничего не делал (в логах не было `[BOSS_SPAWN]`).
2. В Fika спавн идёт на **host/headless** — мод на клиенте игрока без authority не спавнит ботов.

## Установка

Скопировать DLL в `BepInEx/plugins/BossSpawnControl.dll`.

| Режим | Куда |
|-------|------|
| Одиночный SPT | клиент |
| Fika | **headless host** (обязательно) + можно на клиенте для F12 |

## Тест

1. F12 → Boss Spawn Control → Боссы → включить `bossKilla` (на Interchange).
2. F12 → Отладка → **Заспавнить включённых боссов** (в рейде).
3. Смотреть `BepInEx/LogOutput.log` — блок `===== DEBUG SPAWN BUTTON =====`.

## Ограничения

- Без волны на карте используется fallback `BotSpawner.method_2` (синтетический спавн) — может не сработать для всех боссов.
- Конфликт с ABPS progressive boss — наш патч с priority 0 идёт **после** ABPS.

## Поддержать проект

Разовый донат картой РФ, СБП, ЮMoney, VK Pay:  
**[DonationAlerts → kabzon93region](https://www.donationalerts.com/r/kabzon93region)**
