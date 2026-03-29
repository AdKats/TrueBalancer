# Changelog

## 0.5.3.0
- NEW: Added Gamemode Chainlink. Chainlink is treated just like Conquest.

## 0.5.2.0
- Fix: Balancing was not working for Carrier Assault. Carrier Assault will use Conquest settings.

## 0.5.1.1
- Fix: Commanders and Spectators are now excluded from balancing
- Fix: Persona ID could not be found for some players

## 0.5.1.0
- NEW: Automatic Update Check
- Fix: Domination settings block
- Fix: Confirmation msg when scrambling at roundend requested by admin via procon
- Fix: Settings for game mode Obliteration could not be altered

## 0.5.0.0
- NEW: BF4 compatibility

## 0.4.0.7
- Fix: Added Capture The Flag to plugin settings
- Fix: Scrambling for CTF

## 0.4.0.6
- ADD: Support for Air Superiority (Conquest settings apply)
- ADD: Support for Capture the Flag (Gunmaster settings apply)

## 0.4.0.2 - 0.4.0.5
- Restores tb-move and tb-fmove commands
- Reduces lag/delays due to stats fetching
- Reduces Procon panic errors
- Adds player name to StatsException messages
- Adds Maximum number of players to fetch setting

## 0.4.0.1
- ADD: Support for Scavenger (Conquest settings apply)

## 0.4.0.0
- ADD: Support for TankSuperiority (Conquest settings apply)

## 0.3.5.5
- Fix: Message spam on high ticket servers when ticket till end was reached
- Fix: Threshold adjusting for player difference

## 0.3.5.0
- HOTFIX: Workaround for ingame bug with 5 players in a squad

## 0.3.4.0
- CHANGE: Changed scrambling mechanism for 'keep no squads' and 'keep squads with two or more clanmates'

## 0.3.3.2
- Fix: Problems with negative Skills (set to 0)
- Fix: StatsReset feature calculation
- Fix: Rush/GM BalancingGuard wrong values

## 0.3.3.1
- ADDED: ClanTagWhitelist for SkillScrambler
- ADDED: TB-Value metric
- Fix: Squad assignment in scrambler

## 0.3.3
- ADDED: TrueBalancer's own move/fmove commands
- ADDED: Squad splitting options during scrambling

## 0.3.2
- CHANGE: Tickets to % in settings
- TWEAK: BalancingGuard automatic skill detection
- Fix: Dead player movement
- Fix: Manual round restart handling
- ADDED: Execute scrambleround via PRoCon

## 0.3.1
- Fix: Domination bug
- ADDED: Separating GunMaster from Rush
- Fix: Settings minimum values

## 0.3.0
- Adding support for CQC
- NEW: Choose scramble/sort criteria

## 0.2.8
- Tweaking Balancing Guard
- Fixing minor Rush bugs

## 0.2.7
- Fix: Balancing Guard player movement direction
- Fix: Good/bad player SPM definitions

## 0.2.6
- Manual scramble commands added

## 0.2.5
- Fix: Balancing Guard for Rush
- Fix: Critical stage at roundstart

## 0.2.4
- NEW: Balancing Guard feature

## 0.2.2
- Added Threshold for all gamemodes
- Separate settings for all GameModes

## 0.2.0
- Scrambling by SPM (Battlelog)
- Scrambling Message yelled at roundend
- Balance players only when dead

## 0.1.6
- RUSH: Scramble on every new map only

## 0.1.5
- Rush Mode Support

## 0.1.4
- Fix: Showing message at roundend when SkillScrambler deactivated

## 0.1.3
- Fix: Full server scramble handling
- All modes except SquadDeathMatch supported
- RUSH: No scramble when defenders win
- NEW: Message near roundend about upcoming scramble

## 0.1.2
- VIP-List whitelist inclusion
- Players scrambled before next round (no roundstart slay)

## 0.1.0
- SquadBug fixed
- Support for conquestsmall1
- Added scrambler for new maps

## 0.0.6
- Lone wolf option: lone wolfs moved first

## 0.0.5
- Move lone wolfs only option

## 0.0.4
- More ingame message customization

## 0.0.3
- Stop balancing when tickets till end
- Join time bug fix at roundstart

## 0.0.2
- Added Teamdeathmatch

## 0.0.1
- First Release
