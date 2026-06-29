# Boss Spawn Control

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Release](https://img.shields.io/badge/release-v1.5.3-blue)](https://github.com/kabzon93region/BossSpawnControl/releases/tag/v1.5.3)
[![EFT](https://img.shields.io/badge/EFT-16%2E9-orange)](https://www.escapefromtarkov.com/)
[![SPT](https://img.shields.io/badge/SPT-4.0.13-blue)](https://sp-tarkov.com/)
[![Fika](https://img.shields.io/badge/Fika-2%2E3%2Ex-purple)](https://github.com/project-fika/Fika-Plugin)
[![BepInEx](https://img.shields.io/badge/BepInEx-5%2E4%2Ex-yellow)](https://github.com/BepInEx/BepInEx)
![Deployment](https://img.shields.io/badge/deployment-singleplayer%2Cheadless_host%2Chost_client-lightgrey)

Клиентский мод для SPT 4 / Fika: управление спавном боссов, ботов и поддержание численности на карте (F12).

| | |
|---|---|
| **Разработчик** | [kabzon93region](https://github.com/kabzon93region) |
| **Версия** | 1.5.3 |
| **GitHub** | [BossSpawnControl](https://github.com/kabzon93region/BossSpawnControl) |
| **Deployment** | `(singleplayer,headless_host,host_client)` |
| **Тип** | client |

## Статус v1.5.3 (подтверждено тестами)

| Функция | Статус |
|---------|--------|
| Спавн ботов по ролям (F12 → Боты) | ✅ |
| Поддержание населения (лимиты по фракциям + общий cap) | ✅ |
| Сброс AI-ботов с карты | ✅ (почти все; боссы не затрагиваются) |
| Спавн/форс боссов | ✅ (отдельная секция «Боссы») |

**Где работает:** ПК **хоста рейда** — listen-host (основной сценарий), headless или одиночный SPT.

## Куда ставить

| Режим | Куда |
|-------|------|
| Одиночный SPT | ПК игрока |
| Fika **listen-host** (рейд на ПК того, кто создал игру) | **ПК хоста** — основной сценарий |
| Fika **headless** (отдельная машина-хост) | headless PC |
| Fika client (подключился к чужому рейду) | опционально для F12, **без** спавна/сброса ботов |

Спавн, поддержание населения и «Удалить всех ботов» работают только там, где Fika даёт authority хоста (`IsServer` / `EClientType.Host`).

## F12 — секции

### Боссы

- **ModEnabled** — при старте рейда включённым боссам ставится `BossChance=100` (если волна есть на карте).
- Отдельная галочка на каждого босса.
- Кнопка **«Заспавнить включённых боссов»** — принудительный спавн в рейде (даже если ModEnabled=false).

### Боты

- Спавн диких, PMC, rogues по ролям и количеству.

### Население

- Поддержание лимитов по фракциям и общий cap.
- Сброс всех ботов с карты (с подтверждением).
- **ProtectPitFireCompanions** — не удалять последователей Pit Fire Team (требует мод `pitFireTeam`; см. ограничения).

## Установка

Скопировать DLL в `BepInEx/plugins/BossSpawnControl.dll`.

## Тест (listen-host)

1. Создайте рейд Fika на **своём ПК** (вы — host).
2. F12 → Boss Spawn Control → Население → лимиты → «Запустить поддержание».
3. `BepInEx/LogOutput.log` на **этом же ПК**: `[POPULATION]`, строка `Authority: fika host (listen-host...`.

## Ограничения

- Сброс ботов **не удаляет боссов** (boss/follower roles из волн карты).
- Fallback `BotSpawner.method_2` может не сработать для всех боссов без boss-волны на карте.
- Конфликт с ABPS progressive boss — наш патч с priority 0 идёт **после** ABPS.
- **Pit Fire Team:** защита компаньонов при сбросе зависит от API `pitTeam.Modules.BossPlayers` — может не сработать для всех последователей (см. CHANGELOG).

## Поддержать проект

**[DonationAlerts → kabzon93region](https://www.donationalerts.com/r/kabzon93region)**
