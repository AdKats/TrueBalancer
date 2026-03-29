# TrueBalancer

A team balancing plugin for Procon (BF3/BF4 game server administration).

## Features

- **Player Number Balancer** -- keeps teams even by player count with configurable thresholds per game mode
- **Skill Scrambler** -- scrambles teams at round end based on skill metrics (SPM, K/D, Rank, Skill, TB-Value)
- **Balancing Guard** -- prevents unfair team switching and sorts joining players to maintain balance

## Supported Game Modes

- Conquest (all variants including Chainlink, Carrier Assault, Tank Superiority, Scavenger, Air Superiority)
- Rush
- Team Deathmatch
- Domination
- Obliteration
- Gun Master / Capture the Flag
- Defuse

## In-Game Commands

| Command | Description |
|---------|-------------|
| `@scramblenow` | Scramble teams immediately |
| `@scrambleround` | Scramble teams at next round end |
| `@tb-move <player>` | Move player to other team on death |
| `@tb-fmove <player>` | Force move player to other team immediately |

## Installation

Copy the `.cs` files from `src/` into your Procon plugins directory.

## License

GPLv3 -- see [LICENSE](LICENSE) for details.

## Authors

- **Panther** -- original author (versions < 0.5)
- **onegrizzlybeer** -- current maintainer (v0.5+)
- Contributors from the Procon community
