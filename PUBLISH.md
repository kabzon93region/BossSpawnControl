# Publish to GitHub — Boss Spawn Control

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.5.4`  
**Deployment:** `(singleplayer,headless_host,host_client)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/BossSpawnControl/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/BossSpawnControl
git init
git add .
git commit -m "Source backup Boss Spawn Control v1.5.4"
git branch -M main
git remote add origin https://github.com/kabzon93region/BossSpawnControl.git
git push -u origin main
```

Или автоматически:

```powershell
python CURSORAIMODING/tools/publish/publish_github_release.py BossSpawnControl --create-repo
```

## 3. GitHub Release

Прикрепить zip (только игровые файлы, без INSTALL.md):

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\BossSpawnControl_(singleplayer,headless_host,host_client)_v1.5.4_2026-06-29.zip`

```powershell
gh release create v1.5.4 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\BossSpawnControl_(singleplayer,headless_host,host_client)_v1.5.4_2026-06-29.zip" ^
  --title "Boss Spawn Control v1.5.4" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

Управление спавном боссов и населением карты через F12 на хосте рейда (listen-host или headless). v1.5.4: population stable + PitFireTeam companion protection.

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(singleplayer,headless_host,host_client)`.
