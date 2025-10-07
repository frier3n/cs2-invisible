# CS2-invisible
Player model Invisibility toggle counterstrikesharp plugin

# Command
use: `!in playername, @me, @t or @ct`

examples:
| Command  | What does it do |
| ------------- |:-------------:|
| !in f0rest      |makes player f0rest invisible|
| !in @me      |makes YOU invisible     |
| !in @t      |makes entire T team invisible|
| !in @ct      |makes entire CT team invisible|

# Config & Translation
After loading the plugin on the server for the first time, it will create a "config" folder in the same directory as the DLL.
Inside the folder there will be "config.json" and "lang.json". The config and translation can be changed there.

## Config explanation

```
{
  "Persistent": true, => Persistence between rounds
  "RequiredFlag": "@css/root", => Admin flag to use the command
  "ChatPrefix": "[Invisible]" => Chat Prefix
}
```

## Example translations
EN
```
{
  "NoPermission": "You do not have permission to use this command.",
  "YouToggled": "Your invisibility has been toggled!",
  "TeamToggled": "Players from team {team} had their visibility toggled!",
  "PlayerNotFound": "Player not found.",
  "TargetToggled": "{player} had their visibility toggled.",
  "NowInvisible": "You are now invisible!",
  "NowVisible": "You are now visible again!"
}
```
RU
```
{
  "NoPermission": "У вас нет разрешения использовать эту команду.",
  "YouToggled": "Ваша невидимость переключена!",
  "TeamToggled": "Игроки из команды {team} переключили свою видимость!",
  "PlayerNotFound": "Игрок не найден.",
  "TargetToggled": "Игрок {player} переключил свою видимость.",
  "NowInvisible": "Теперь вы невидимы!",
  "NowVisible": "Теперь вы снова видимы!"
}
```
