# Publish to GitHub — Boss Spawn Control

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.1.0`  
**Deployment:** `(singleplayer,headless_host)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/BossSpawnControl/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/BossSpawnControl
git init
git add .
git commit -m "Source backup Boss Spawn Control v1.1.0"
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

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\BossSpawnControl_(singleplayer,headless_host)_v1.1.0_2026-06-28.zip`

```powershell
gh release create v1.1.0 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\BossSpawnControl_(singleplayer,headless_host)_v1.1.0_2026-06-28.zip" ^
  --title "Boss Spawn Control v1.1.0" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

Управление спавном боссов через F12: авто-spawn при старте рейда и кнопка принудительного spawn на host/headless.

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(singleplayer,headless_host)`.
