# Changelog — Boss Spawn Control

## 1.5.3 (2026-06-29) — **stable (population)**

**Подтверждено в тестах (listen-host / Fika):** спавн по ролям, поддержание лимитов по фракциям и общий cap, сброс AI-ботов с карты работают стабильно. Боссы при сбросе **не удаляются** (ожидаемо — кнопка «Удалить всех ботов» рассчитана на обычных ботов, не на boss-волны).

- **Fix спавна USEC/отступников в maintenance:** `BotSpawner.method_2(forced=false)` — игровой путь; PMC через BossSpawner, exUsec с `EPlayerSide.Savage`.
- **PMC/rogue zones:** для USEC/BEAR/отступников — `GetPmcZones()` вместо случайной зоны.
- **Fix ложного cap:** утечка `BotsLoading` после failed profile gen больше не блокирует maintenance.
- **Deployment:** `(singleplayer,headless_host,host_client)` — основной сценарий listen-host на ПК игрока-хоста.

### Известное ограничение (v1.5.3)

- **Pit Fire Team:** опция `ProtectPitFireCompanions` (по умолчанию вкл.) может не сработать для последователей отряда — см. расследование в следующих версиях.

## 1.5.2 (2026-06-29)

- **Deployment:** метка `(host_client)` — мод для **listen-host** (рейд на ПК игрока-хоста), не только headless.
- **Authority / UI:** сообщения F12 и логи явно указывают listen-host (`FikaBackendUtils.IsServer` / `EClientType.Host`) vs подключившийся client.

## 1.5.1 (2026-06-29)

- **Fix лимита USEC/BEAR:** `pmcUSEC`/`pmcBEAR`/`pmcBot` классифицируются как USEC/BEAR до проверки `IsBossOrFollower()` (в EFT они boss-flagged, из-за этого USEC считался 0 и лимит 4 не работал).
- **Fix спавна PMC:** maintenance использует `BotCreationDataClass` + `TryToSpawnInZoneAndDelay(forced=false)` вместо `method_2` boss-path с `IgnoreMaxBots`.
- **Учёт pending spawns:** `botsLoading` + очередь + `InSpawnProcess` входят в `effectiveTotal` для globalRoom.
- **Синхронизация MaxBots:** пока maintenance активен, `BotSpawner.SetMaxBots(LimitTotal)`; восстановление при остановке.
- **Trim excess:** удаление ботов сверх общего/фракционного лимита (vanilla/Fika queue могла поднимать счёт выше cap).
- **Authority headless:** `IsHeadless` / `IsHeadlessGame` / plugin `com.fika.headless` для сброса и maintenance на headless host.
- **Clear bots:** fallback `IBotGame.BotDespawn`, лог при нажатии кнопки без confirm/authority.

## 1.5.0 (2026-06-28)

- **Мгновенный сброс ботов:** `RemoveFromMap()` + `Kill()` вместо только `DoLeaveExternal()` (боты уходили на точку и оставались в BotOwners).
- **Poll после сброса:** повторные попытки до `ClearPollTimeoutSec`, лог `removeFromMap` / `kill` / `stillAlive`.
- **Блок автодоспавна** после сброса (`ClearSpawnBlockSec`, по умолчанию 45 сек).
- **Authority check:** сброс только на headless host / singleplayer (`FikaBackendUtils.IsServer` через reflection).
- **ProtectBtrDuringClear** — опция не трогать БТР.
- Модульная структура: `BotInstantRemover`, `BotRemovalPollRunner`, `PopulationMaintenanceSpawner`, `PopulationMaintenanceDeficits`.

## 1.4.2 (2026-06-28)

- **Защита Pit Fire Team:** компаньоны/последователи отряда не удаляются кнопкой сброса (`ProtectPitFireCompanions`, по умолчанию вкл.).
- Сброс идёт выборочно по BotOwners (без `LeaveAll`), в логе — `KEEP companion` / `protectedCompanions=N`.
- Опционально использует API `PitFireTeamFikaFix.CompanionGuard` или напрямую `BossPlayers.IsFollower`.

## 1.4.1 (2026-06-28)

- **Лимиты в штуках:** убраны слайдеры 0–100 (выглядели как %). Теперь поля ввода числа — «3 диких, 5 BEAR, 10 отступников, общий 15».
- Подписи лимитов: «(шт.)», в статусе F12 тоже «шт.»
- **Сброс ботов:** кнопка «Удалить всех ботов с карты» + галочка подтверждения; автостоп поддержания; лог до/после.

## 1.4.0 (2026-06-28)

- **Население карты:** режим поддержания численности ботов (F12 → «Население»).
- **Счётчик по фракциям:** отступники, дикие (assault/cursed/marksman), зомби, USEC, BEAR, боссы+свита.
- **Лимиты одновременного присутствия** по каждой фракции + общий лимит (0 = без лимита).
- **Приоритет фракций** — если общий лимит меньше суммы лимитов, доспавн идёт по приоритету.
- Сканирование каждые **3–8 сек** (настраивается); кнопки **Запустить/Остановить** и **Сканировать сейчас**.
- Статус счётчиков в F12 после последнего скана; подробные логи `[POPULATION]` / `[BOT_SPAWN]` / `[BOSS_SPAWN]`.
- Спавн через `method_2` с `await` — в логе видно успех/ошибку каждой попытки.
- Зомби-автоспавн выключен по умолчанию (`SpawnZombies=false`).

## 1.3.0 (2026-06-28)

- **PMC BEAR** и **PMC USEC** — отдельные роли (`pmcBEAR`, `pmcUSEC`) вместо одного общего PMC.
- **Зомби-дикие:** `infectedAssault`, `infectedCivil`, `infectedLaborant` (Halloween / infected scav roles).
- `pmcBot` переименован в «PMC случайный» — оставлен для совместимости конфига.

## 1.2.0 (2026-06-28)

- **Новая секция «Боты»** в F12 Configuration Manager.
- Поддержка спавна обычных ботов: **Дикий**, **Проклятый дикий**, **PMC**, **Бывший USEC**.
- Для каждой роли — отдельное поле **«Количество»** (0–20).
- Выбор **сложности** ботов (normal / hard / impossible) в настройках «Боты».
- Отдельная кнопка **«Заспавнить выбранных ботов»** с независимым логированием `[BOT_SPAWN]`.
- Боты спавнятся через `BotSpawner.method_2` с указанной стороной и сложностью.

## 1.1.0 (2026-06-28)

- **F12 Configuration Manager:** полный список боссов — отдельный bool на каждого.
- **ModEnabled:** авто-спавн включённых боссов при старте рейда.
- **Кнопка отладки:** принудительный спавн включённых боссов в рейде (даже если ModEnabled=false или босс убит).
- **Детальное логирование** кнопки и авто-правил (`[BOSS_SPAWN]`).
- Fallback: `BotSpawner.method_2` если на карте нет boss-волны.
- Harmony priority 0 — патч после ABPS progressive boss.

## 1.0.0 (2026-06-28)

- Первый релиз (строковый конфиг ForceSpawn/ForceDisable) — заменён в 1.1.0.
