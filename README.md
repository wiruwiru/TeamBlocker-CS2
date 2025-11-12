# TeamBlocker
Restricts how many players can join a team. Once the limit is reached, no additional players are allowed to join.
---

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master).
2. Download [TeamBlocker.zip](https://github.com/wiruwiru/TeamBlocker-CS2/releases/latest) from the releases section.
3. Unzip the archive and upload it to your CS2 game server.
4. Start the server. The plugin will be loaded and ready to use.

---

## Configuration
The `TeamBlocker.json` configuration file will be automatically generated when the plugin is first loaded. Below are the available configuration options:

### Team Settings
| Parameter                    | Description                                                                 | Default |
|------------------------------|-----------------------------------------------------------------------------|---------|
| `MaxCounterTerrorists`       | Maximum number of players allowed on the Counter-Terrorist team            | `5`     |
| `MaxTerrorists`              | Maximum number of players allowed on the Terrorist team                     | `5`     |
| `MoveToSpectatorOnConnect`   | If `true`, automatically moves all connecting players to spectator mode     | `true`  |

### Sound Settings
| Parameter                    | Description                                                                 | Default |
|------------------------------|-----------------------------------------------------------------------------|---------|
| `SoundFilePath`              | Path to the sound event file to precache                                   | `"soundevents/game_sounds_ui.vsndevts"` |
| `SoundEvent`                 | Sound event name to play when team join is blocked                         | `"Vote.Failed"` |

### General Settings
| Parameter                    | Description                                                                 | Default |
|------------------------------|-----------------------------------------------------------------------------|---------|
| `EnableDebug`                | If `true`, displays debug messages in the server console                    | `false` |

### Example Configuration
```json
{
  "TeamSettings": {
    "MaxCounterTerrorists": 5,
    "MaxTerrorists": 5,
    "MoveToSpectatorOnConnect": true
  },
  "SoundSettings": {
    "SoundFilePath": "soundevents/game_sounds_ui.vsndevts",
    "SoundEvent": "UI.PlayerPingUrgent"
  },
  "EnableDebug": false,
  "ConfigVersion": 1
}
```
---