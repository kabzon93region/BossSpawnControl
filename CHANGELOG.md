# Changelog — Boss Spawn Control

## 1.1.0 (2026-06-28)

- **F12 Configuration Manager:** полный список боссов — отдельный bool на каждого.
- **ModEnabled:** авто-спавн включённых боссов при старте рейда.
- **Кнопка отладки:** принудительный спавн включённых боссов в рейде (даже если ModEnabled=false или босс убит).
- **Детальное логирование** кнопки и авто-правил (`[BOSS_SPAWN]`).
- Fallback: `BotSpawner.method_2` если на карте нет boss-волны.
- Harmony priority 0 — патч после ABPS progressive boss.

## 1.0.0 (2026-06-28)

- Первый релиз (строковый конфиг ForceSpawn/ForceDisable) — заменён в 1.1.0.
