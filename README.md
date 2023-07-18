WackysDatabase

WackysDatabase is a mod for Valheim created by Wackymole & Rexabyte.

Version: 2.0
Features

    SetEffects: All of them!
    4x more item configurations for Primary and Secondary Attacks
    CustomIcons: Autogenerated or JPEG/PNG (64x64)
    Add or remove conversions on smelter pieces
    MaxCraftingStationLevel: YAML support
    Mainscreen cloned items
    Water Resistance is back
    Slow Reload!
    Visuals Modifier by Rexabit!
    Seteffect by Azu!
    Most components can be removed from YAML
    sizeMultiplier: Make a small or HUGE world
    Future Proof: Can add or remove components without much fuss

<!-- ![WackysDatabase](https://wackymole.com/hosts/lightblue%20Sword.webp) ![WackysDatabase](https://wackymole.com/hosts/1825-1648309710-715635595.png) ![WackysDatabase](https://wackymole.com/hosts/orangeish%20bow.jpg) -->
Description

WackysDatabase, or Wackydb, allows you to modify and customize items, pieces, recipes, and effects in Valheim without coding. With this mod, you can clone items, change their colors, create new items, and even translate language. It offers a wide range of possibilities for customization.

<details><summary>Installation</summary>
### Installation

    Download and extract the latest version of WackysDatabase into the BepInEx plugin folder (usually Valheim/BepInEx/plugins).
    Run Valheim and join a world.
    Go to Valheim/BepInEx/config/ and locate the "wackysDatabase" folder.
    Inside the "wackysDatabase" folder, you will find three subfolders: "Items," "Recipes," and "Pieces."
    Put the mod on the server to force server sync. The YAML files only need to be on the server. There's no need to share the YAML files.

For multiplayer, the mod has been locked down to prevent easy cheating. However, it is recommended to use the AzuAntiCheat and ServerCharacters mods for additional security.
    AzuAntiCheat
    ServerCharacters

</details>

<details><summary>FAQ</summary>

Frequently Asked Questions

    Q: Can I use custom icons for items?
        A: Yes, you can use autogenerated icons or provide JPEG/PNG icons (64x64) for customization.

    Q: Can I add new items with this mod?
        A: Yes, you can create new items and make them exclusive.

    Q: Can I remove components from YAML files?
        A: Yes, most components can be removed from the YAML files.

    Q: Is the mod future-proof?
        A: Yes, you can add or remove components without much hassle.
</details>

## Configuration

The configuration file for WackysDatabase is located at BepInEx/config/WackyMole.WackysDatabase.cfg. The available configurations and their default values are as follows:

- Force Server Config: true (forces server sync)
- Enable this mod: true
- IsDebug: true (enables debug information)
- StringisDebug: false (enables string debugging)
- IsAutoReload: false (enables auto-reload instead of using wackydb_reload command)
- NexusModID: 1825 (does not have much functionality)
- DedServer load Memory: false (dedicated server loads objects into the game like a client)
- ExtraSecurity on Servers: true (prevents loading into singleplayer and then loading into multiplayer)
- FileWatcher for YMLs: true (reloads the mod on any changes to the wackydatabase folder on the server)
- List of Extra Effects: "lightningAOE" (extra effects to look for from the base game or mods)


<details><summary> Console Commands</summary>



## Console Commands

You will can reference https://valheim-modding.github.io/Jotunn/data/objects/item-list.html for Prefab names. Thank you JVL team

To use the console commands, press F5 in the game to open the game console. Make sure to enable the console for Valheim by adding "-console" to the launch options.

- `wackydb_reload`: Reloads all the YML files in the wackysDatabase folder. : This now works for admins on server now. So you could turn off Filewatcher and just do a manual reload on your client and forces all clients to reload. 
- `wackydb_reload_fast`: Fast reload that may cause game stutter.
- `wackydb_save_recipe [ItemName]`: Saves a recipe YML in the wackysDatabase Recipe folder.
- `wackydb_save_piece [PieceName]`: Saves a piece YML in the wackysDatabase Piece folder.
- `wackydb_save_item [ItemName]`: Saves an item YML in the wackysDatabase Item folder.
- `wackydb_save_creature [CreatureName]`: Saves a Creature YML in the wackysDatabase Creature folder.
- `wackydb_save_material[MaterialName]`: Saves a Material clone YML in the wackysDatabase Material folder. Usually has a _mat at end end. 
- `wackydb_all_items`: Saves all items in the game into wackyDatabase-BulkYML.
- `wackydb_all_recipes`: Saves all recipes in the game into wackyDatabase-BulkYML.
- `wackydb_all_pieces [Hammer] [Optionally: Category]`: Saves all pieces in the game into wackyDatabase-BulkYML. Use 'Hammer' for default, or specify a different hammer name. Optionally, you can set a category to only get specific pieces in a cat.
- `wackydb_se_all`: Retrieves almost all status effects in the game (including modded effects) and saves them into the Effects folder.
- `wackydb_se [effectname]`: Retrieves a specific status effect and saves it into the Effect folder.
- `wackydb_se_create`: Creates a clone of SetEffect_FenringArmor in the Status folder. You can edit it as needed.
- `wackydb_help`: Shows a list of commands.
- `wackydb_clone [recipe/item/piece/creatures] [Prefab to clone] [Unique name for the clone]`: Clones an object and changes it differently than a base game object. For example: `wackydb_clone item SwordIron WackySword`.

--There is a optional 4th parameter for clone RECIPES ONLY [original item prefab to use for recipe](Optional 4th parameter for a cloned item's recipes ONLY)
--For example you can already have item WackySword loaded in game, but now want a recipe. WackySword Uses SwordIron  - wackydb_clone recipe WackySword RWackySword SwordIron - otherwise manually edit

- `wackydb_clone_recipeitem [Prefab to clone] [clone name]`: Clones an item and recipe at the same time. The recipe name will be Rname.
- `wackydb_vfx`: Saves a vfx.txt file with all vfx effects
- `wackydb_sfx`: Saves a sfx.txt file with all sfx effects
- `wackydb_fx`: Saves a fx.txt file with all fx effects
- `wackydb_help`: command list
- `wackydb_describe[ObjectName]`: Saves describe of an object, so you have an idea of the structure of the object for materials and customVisuals
- `wackydb_sendtheload` : Experiemental command that will send pngs and objs to clients utilizing ServerSync. I recommend the Network mod to uncap datarates. 
- `wackydb_get_piecehammers`: Saves all hammers, currently in your game to Hammer.txt file
- `wackydb_material` : Generate a text file of all <Material> Gameobjects in vanilla game. Saves to text


</details>

YML Knowledge

    YML is easier to edit and change without getting confused on the syntac. 

    You can use https://www.yamllint.com/ to validate any yml code

    Almost every component of items/pieces/recipes/effects/creatures can be deleted.

    Some components are multilined where you can actually add your own stuff the ymls.


<details><summary><b> Item components</b></summary>


## Item Config

![Glowing Red BronzeSword ](https://wackymole.com/hosts/redsword.png)

### Properties

- `name` (string, required): The name of the item.
- `m_weight` (float, required): The weight of the item.
- `m_name` (string):The in game name.
- `m_description` (string): The description of the item.
- `clonePrefabName` (string): The name of the prefab to clone.
- `mockName` (string): The name of the mock object.
- `customIcon` (string): The custom icon for the item. PNG 64x64, Icon needs to be in the Icon folder ( doesn't server sync)
- `material` (string): The material of the item. Images on nexus https://www.nexusmods.com/valheim/mods/1825 of the various changes you can make. </br>
Visit the Material and CustomVisual Section to understand this complex system. 
- `customVisual` (CustomVisual): The custom visual data of the item.
- `sizeMultiplier` (float): The size multiplier of the item. You can go from .01 to 1000.5 if you want. Have fun!
- `scale_weight_by_quality` (float): The scaling factor for weight based on quality.
### CustomVisual

- `base_mat` (string): The base material of the custom visual.
- `chest` (string): The chest visual.
- `legs` (string): The legs visual.
- `realtime` (string): The realtime visual.


`Primary_Attack` (AttackArm): The primary attack data.
### Primary Attack

- `AttackType` (Attack.AttackType): The type of attack.
- `Attack_Animation` (string): The animation for the attack.
- `Attack_Random_Animation` (int): The random animation for the attack.
- `Chain_Attacks` (int): The number of chain attacks.
- `Hit_Terrain` (bool): Indicates whether the attack can hit terrain.
- `Custom_AttackSpeed` (float): This mod keeps track of this adjustment 1.0 is normal speed. 2.0 is twice as fast. This will override Wackymmo speeds. Should be compatible with most other mods that increase speed.
- `m_attackStamina` (float): The stamina cost of the attack.
- `m_eitrCost` (float): The eitr cost of the attack.
- `AttackHealthCost` (float): The health cost of the attack.
- `m_attackHealthPercentage` (float): The health cost percentage of the attack.
- `SpeedFactor` (float): The speed factor of the attack.
- `DmgMultiplier` (float): The damage multiplier of the attack.
- `ForceMultiplier` (float): The force multiplier of the attack.
- `StaggerMultiplier` (float): The stagger multiplier of the attack.
- `RecoilMultiplier` (float): The recoil multiplier of the attack.
- `AttackRange` (float): The range of the attack.
- `AttackHeight` (float): The height of the attack.
- `Spawn_On_Trigger` (string): The spawn-on-trigger effect of the attack.
- `Requires_Reload` (bool): Indicates whether the attack requires reloading.
- `Reload_Animation` (string): The animation for reloading.
- `ReloadTime` (float): The time it takes to reload.
- `Reload_Stamina_Drain` (float): The stamina drain during reloading.
- `Bow_Draw` (bool): Indicates whether the bow is drawn for the attack.
- `Bow_Duration_Min` (float): The minimum duration of the bow.
- `Bow_Stamina_Drain` (float): The stamina drain during bow usage.
- `Bow_Animation_State` (string): The animation state for the bow.
- `Attack_Angle` (float): The angle of the attack.
- `Attack_Ray_Width` (float): The width of the attack ray.
- `Lower_Dmg_Per_Hit` (bool): Indicates whether the attack lowers damage per hit.
- `Hit_Through_Walls` (bool): Indicates whether the attack can hit through walls.
- `Multi_Hit` (bool): Indicates whether the attack can hit multiple times.
- `Pickaxe_Special` (bool): Indicates whether it is a special pickaxe attack.
- `Last_Chain_Dmg_Multiplier` (float): The damage multiplier for the last chain attack.
- `Attack_Projectile` (string): Indicates whether the attack has a projectile.
- `Projectile_Vel` (float): The velocity of the projectile.
- `Projectile_Accuraccy` (float): The accuracy of the projectile.
- `Projectiles` (int): The number of projectiles.
- `AEffects` (AEffects): The additional effects of the attack.

### Class: AEffects

- `Hit_Effects` (string[]): The hit effects of the attack.
- `Hit_Terrain_Effects` (string[]): The effects when hitting terrain.
- `Start_Effect` (string[]): The starting effect of the attack.
- `Trigger_Effect` (string[]): The triggered effect of the attack.
- `Trail_Effect` (string[]): The effect trail of the attack.
- `Burst_Effect` (string[]): The burst effect of the attack.

### Secondary Attack 
  Is the same as Primary just for the secondary attack. 


`Damage` (WDamages): The damage values of the item.
`Damage_Per_Level` (WDamages): The damage values per level of the item.
###  Damages

- `Blunt` (float): The blunt damage value.
- `Chop` (float): The chop damage value.
- `Damage` (float): The general damage value.
- `Fire` (float): The fire damage value.
- `Frost` (float): The frost damage value.
- `Lightning` (float): The lightning damage value.
- `Pickaxe` (float): The pickaxe damage value.
- `Pierce` (float): The pierce damage value.
- `Poison` (float): The poison damage value.
- `Slash` (float): The slash damage value.
- `Spirit` (float): The spirit damage value.

`Armor` (ArmorData): The armor data of the item.
### Armor
- `armor` (float): The armor value.
- `armorPerLevel` (float): The armor value per level. 

`FoodStats` (FoodData): The food-related statistics of the item.
### FoodData

- `m_foodHealth` (float): The health provided by the food.
- `m_foodStamina` (float): The stamina provided by the food.
- `m_foodRegen` (float): The regeneration rate of the food.
- `m_foodBurnTime` (float): The burn time of the food.
- `m_FoodEitr` (float): The eitr provided by the food.

`Moddifiers` (StatMods): The stat modifiers of the item.
### StatMods
- `m_movementModifier` (float): The movement modifier.
- `m_EitrRegen` (float): The eitr regeneration modifier.


`SE_Equip` (SE_Equip): The special effect data for equipping the item.</br> If you want an Item to have an Effect by itself, put the effect name here
</br>`SE_SET_Equip` (SE_SET_Equip): All of this should be the same accross all items that have this set
</br>You can delete an SE_Equip or SE_SET_Equip from item using EffectName : delete 
### SE_Equip 

- `EffectName` (string): The name of the effect for equipping.

### SE_SET_Equip

- `SetName` (string): The name of the set.
- `Size` (int): The size of the set.
- `EffectName` (string): The name of the effect for equipping the set.


`ShieldStats` (ShieldData): The shield statistics of the item.
- ### Shield

- `m_blockPower` (float): The block power of the shield.
- `m_blockPowerPerLevel` (float): The block power per level of the shield.
- `m_timedBlockBonus` (float): The timed block bonus of the shield.
- `m_deflectionForce` (float): The deflection force of the shield.
- `m_deflectionForcePerLevel` (float): The deflection force per level of the shield.

###  Properties Continued
- `m_maxStackSize` (int): The maximum stack size of the item.
- `m_canBeReparied` (bool): Indicates whether the item can be repaired.
- `m_destroyBroken` (bool): Indicates whether the item gets destroyed when broken.
- `m_dodgeable` (bool): Indicates whether the item can be dodged.
- `Attack_status_effect` (string): The attack status effect of the item.
- `spawn_on_hit` (string): The spawn-on-hit effect of the item.
- `spawn_on_terrain_hit` (string): The spawn-on-terrain-hit effect of the item.
- `m_questItem` (bool): Indicates whether the item is a quest item.
- `m_teleportable` (bool): Indicates whether the item is teleportable.
- `m_backstabbonus` (float): The backstab bonus of the item.
- `m_knockback` (float): The knockback value of the item.
- `m_useDurability` (bool): Indicates whether the item uses durability.
- `m_useDurabilityDrain` (float): The durability drain when the item is used.
- `m_durabilityDrain` (float): The durability drain of the item.
- `m_maxDurability` (float): The maximum durability of the item.
- `m_durabilityPerLevel` (float): The durability increase per level of the item.
- `m_equipDuration` (float): The equip duration of the item.
- `m_skillType` (Skills.SkillType): The skill type of the item.
- `m_animationState` (ItemDrop.ItemData.AnimationState): The animation state of the item.
- `m_itemType` (ItemDrop.ItemData.ItemType): The item type of the item.
- `m_toolTier` (int): The tool tier of the item.
- `m_maxQuality` (int): The maximum quality of the item.
- `m_value` (int): The value of the item. The value of the item. if value is >0. Then the object becomes salable at Trader. 
        The Object Description gets a yellow Valuable notice. Just like base game you don't know what object you are selling to Trader.
- `damageModifiers` (List<string>): The list of damage modifiers for the item.

### DamageModifiers
The first value is the damage type, the second value is the resistance level.</br>
Blunt Slash Pierce Chop Pickaxe Physical Fire Frost Lightning Elemental Poison Spirit Water 
 
    Normal - no change
    Resistant - increases status countdown speed by 100%
    Weak - Decreases status countdown speed by 1/3
    Immune - prevents status effect
    Ignore - prevents status effect
    VeryResistant - Prevent status effect application except when actively in, and increases status countdown speed by 100%
    VeryWeak - Decreases status countdown speed by 2/3



`GEffects` (GEffects): The additional game effects of the item.

### GEffects

- `Hit_Effects` (string[]): The hit effects.
- `Hit_Terrain_Effects` (string[]): The effects when hitting terrain.
- `Start_Effect` (string[]): The starting effect.
- `Hold_Start_Effects` (string[]): The hold starting effects.
- `Trigger_Effect` (string[]): The triggered effect.
- `Trail_Effect` (string[]): The effect trail.


</details>

<details><summary> Piece components</summary>

<img src="https://wackymole.com/hosts/red%20walls.png" width="450"/>


### Pieces

- `name` (string, required): The name of the piece.
- `piecehammer` (string, required): The piece hammer required to build the piece.
- `m_name` (string): The In Game Piece Name
- `sizeMultiplier` (float): The size multiplier of the piece. Any float range 1.0 is normal size, 2.0 is v^3
- `m_description` (string): The description of the piece.
- `customIcon` (string): The custom icon for the piece. PNG or JPEG, 64 x 64, Must be in Icon Folder (Icons do not Synced)
- `clonePrefabName` (string): The name of the prefab to clone.
- `material` (string):  Visit Material Section and Custom Visual Section
- `damagedMaterial` (string): The material of the piece when damaged.
- `craftingStation` (string): The crafting station required to craft the piece.
- `piecehammerCategory` (string): The category of the piece hammer.
- `minStationLevel` (int): The minimum level of the crafting station required.
- `amount` (int): The amount of the piece.
- `disabled` (bool): Indicates whether the piece is disabled.
- `adminonly` (bool): Indicates whether the piece is admin-only.
- `comfort` (ComfortData): The comfort data of the piece.
- `groundPiece` (bool): Indicates whether the piece is a ground piece.
- `ground` (bool): Indicates whether the piece can be placed on the ground.
- `waterPiece` (bool): Indicates whether the piece is a water piece.
- `noInWater` (bool): Indicates whether the piece cannot be placed in water.
- `notOnFloor` (bool): Indicates whether the piece cannot be placed on the floor.
- `onlyinTeleportArea` (bool): Indicates whether the piece can only be placed in teleport areas. Stone Groups making a Circle
- `allowedInDungeons` (bool): Indicates whether the piece is allowed in dungeons.
- `canBeRemoved` (bool): Indicates whether the piece can be removed.
- `wearNTearData` (WearNTearData): The wear and tear data of the piece.
- `craftingStationData` (CraftingStationData): The crafting station data of the piece.
- `cSExtensionData` (CSExtensionData): The crafting station extension data of the piece.
- `contData` (ContainerData): The container data of the piece.
- `smelterData` (SmelterData): The smelter data of the piece.
- `build` (List<string>): The list of requirement items for the piece.

build: requirements to build: Item:amount:amountPerLevel:refundable,

### ComfortData

- `comfort` (int): The comfort value of the piece.
- `comfortGroup` (Piece.ComfortGroup): The comfort group of the piece.
- `comfortObject` (GameObject): The comfort object of the piece.

### WearNTearData

- `health` (float): The health of the piece.
- `damageModifiers` (HitData.DamageModifiers): The damage modifiers for wear and tear.
- `noRoofWear` (bool): Indicates whether the piece does not wear when under a roof.
- `noSupportWear` (bool): Indicates whether the piece does not wear when supported.
- `supports` (bool): Indicates whether the piece provides support.
- `triggerPrivateArea` (bool): Indicates whether the piece triggers the private area.

### CraftingStationData

- `cStationCustomIcon` (string): The custom icon for the crafting station. May remove - Not really useful
- `discoveryRange` (float): The discovery range of the crafting station. Range that you discovery the piece for the first time
- `buildRange` (float): The build range of the crafting station.
- `craftRequiresRoof` (bool): Indicates whether crafting at the station requires a roof.
- `craftRequiresFire` (bool): Indicates whether crafting at the station requires a fire.
- `showBasicRecipes` (bool): Indicates whether to show basic recipes at the station.
- `useDistance` (float): The use distance of the station.
- `useAnimation` (int): The animation to use for station usage.

### CSExtensionData

- `MainCraftingStationName` (string): The name of the main crafting station.
- `maxStationDistance` (float): The maximum distance for the extension to connect to the main station.
- `continousConnection` (bool): Indicates whether the extension provides continuous connection.
- `stack` (bool): Indicates whether the extension can stack.

###  ContainerData

- `Width` (int): The width of the container.
- `Height` (int): The height of the container.
- `CheckWard` (bool): Indicates whether the container checks for ward placement.
- `AutoDestoryIfEmpty` (bool): Indicates whether the container auto-destroys if empty.

### SmelterData


- `smelterName` (string): The name of the smelter.
- `addOreTooltip` (string): The tooltip for adding ore to the smelter.
- `emptyOreTooltip` (string): The tooltip for emptying ore from the smelter.
- `fuelItem` (fuelItemData): The fuel item for the smelter.
- `maxOre` (int): The maximum ore capacity of the smelter.
- `maxFuel` (int): The maximum fuel capacity of the smelter.
- `fuelPerProduct` (int): The amount of fuel required per product.
- `secPerProduct` (float): The time taken per product.
- `spawnStack` (bool): Indicates whether the smelter spawns stacked products.
- `requiresRoof` (bool): Indicates whether the smelter requires a roof.
- `addOreAnimationLength` (float): The length of the animation for adding ore.
- `smelterConversion` (List<SmelterConversionList>): The list of smelter conversions.

### fuelItemData

- `name` (string): The name of the fuel item.

### SmelterConversionList

- `FromName` (string): The name of the item to convert from.
- `ToName` (string): The name of the item to convert to.

Delete all by using "-"

</details>

<details><summary> Recipe Components</summary>


<img src="https://wackymole.com/hosts/red%20forge.webp" width="700"/>


## Recipes

- `name` (string, required): The name of the recipe.
- `clonePrefabName` (string): The name of the prefab to clone.
- `craftingStation` (string): The crafting station required to craft the recipe. "" is hand crafted
- `minStationLevel` (int): The minimum level of the crafting station required.
- `maxStationLevelCap` (int): The maximum level cap of the crafting station.
- `repairStation` (string): The repair station required to repair the recipe.
- `amount` (int): The amount of the recipe.
- `disabled` (bool): Indicates whether the recipe is disabled.
- `requireOnlyOneIngredient` (bool): Indicates whether the recipe requires only one ingredient.
- `reqs` (List<string>, required): The list of required ingredients for the recipe.
- 
(Required) requirements to build: Item:amount:amountPerLevel:refundable:quality,

Quality is a requirement of what quality of item you need to be able to use this recipe.  Like a 2* Bronze Sword to upgrade to 3*
 </br>Arrows x50 will be put above Arrow x20


</details>

<details><summary> Creatures</summary>


## CreatureData

- `name` (string, required): The name of the creature.
- `mob_display_name` (string, required): The display name of the creature in game.
- `custom_material` (string): The custom material for the creature.
- `clone_creature` (string): The name of the creature to clone.
- `creature_replacer` (string): The creature replacer. This will replace this creature with the one named here. All spawns will be replaced with this creature. 


</details>

<details><summary> SE_Effects</summary>

### SE_Effects

- `Name` (string): The name of the status effect.
- `Status_m_name` (string): In Game Name
- `Category` (string): The category of the status effect.
- `IconName` (string): The name of the icon for the status effect.
- `CustomIcon` (string): The custom icon for the status effect. customIcon, jpeg or png. ie wacky.png 64 x 64
- `FlashIcon` (bool): Indicates whether the icon should flash.
- `CooldownIcon` (bool): Indicates whether the icon is for a cooldown.
- `Tooltip` (string): The tooltip for the status effect.
- `Attributes` (StatusEffect.StatusAttribute): The attributes of the status effect.
- `StartMessageLoc` (MessageHud.MessageType): The localization message type for the start message.
- `StartMessage` (string): The start message of the status effect.
- `StopMessageLoc` (MessageHud.MessageType): The localization message type for the stop message.
- `StopMessage` (string): The stop message of the status effect.
- `RepeatMessageLoc` (MessageHud.MessageType): The localization message type for the repeat message.
- `RepeatMessage` (string): The repeat message of the status effect.
- `RepeatInterval` (float): The repeat interval of the status effect.
- `TimeToLive` (float): The time to live of the status effect.
- `StartEffect` (List<string>): The list of start effects for the status effect.
- `StopEffect` (List<string>): The list of stop effects for the status effect.
- `Cooldown` (float): The cooldown of the status effect.
- `ActivationAnimation` (string): The animation for the activation of the status effect.
- `SeData` (SEdata): The additional data for the status effect.

### Class: SEdata

- `m_tickInterval` (float): The tick interval for the status effect.
- `m_healthPerTickMinHealthPercentage` (float): The minimum health percentage per tick.
- `m_healthPerTick` (float): The health per tick.
- `m_healthOverTime` (float): The health over time value.
- `m_healthOverTimeDuration` (float): The duration of health over time.
- `m_healthOverTimeInterval` (float): The time interval for health over time.
- `m_staminaOverTime` (float): The stamina over time value.
- `m_staminaOverTimeDuration` (float): The duration of stamina over time.
- `m_staminaDrainPerSec` (float): The stamina drain per second.
- `m_runStaminaDrainModifier` (float): The stamina drain modifier for running.
- `m_jumpStaminaUseModifier` (float): The stamina use modifier for jumping.
- `m_eitrOverTime` (float): The eitr over time value.
- `m_eitrOverTimeDuration` (float): The duration of eitr over time.
- `m_healthRegenMultiplier` (float): The health regeneration multiplier.
- `m_staminaRegenMultiplier` (float): The stamina regeneration multiplier.
- `m_eitrRegenMultiplier` (float): The eitr regeneration multiplier.
- `m_raiseSkill` (Skills.SkillType): The skill type to raise.
- `m_raiseSkillModifier` (float): The skill modifier to raise.
- `m_skillLevel` (Skills.SkillType): The skill type to modify the level.
- `m_skillLevelModifier` (float): The skill level modifier.
- `m_skillLevel2` (Skills.SkillType): Another skill type to modify the level.
- `m_skillLevelModifier2` (float): Another skill level modifier.
- `m_mods` (List<HitData.DamageModPair>): The list of damage modifiers for hit.
- `m_modifyAttackSkill` (Skills.SkillType): The skill type to modify the attack.
- `m_damageModifier` (float): The damage modifier for the attack.
- `m_noiseModifier` (float): The noise modifier for sneaking.
- `m_stealthModifier` (float): The stealth modifier for sneaking.
- `m_addMaxCarryWeight` (float): The additional maximum carry weight.
- `m_speedModifier` (float): The speed modifier.
- `m_maxMaxFallSpeed` (float): The maximum maximum fall speed.
- `m_fallDamageModifier` (float): The fall damage modifier.
- `m_tickTimer` (float): The tick timer.
- `m_healthOverTimeTimer` (float): The health over time timer.
- `m_healthOverTimeTicks` (float): The health over time ticks.
- `m_healthOverTimeTickHP` (float): The health over time tick health points.


You should be able to delete existing m_mods, by
</br>m_mods:
</br> -

Use wackydb_se_create as a "template" to create a new status effect


</details>

<details><summary> MOCK System </summary>

![Bike Model Import ](https://wackymole.com/hosts/bike.png)

All Credits to @KG for making this incredible system.

In Wackydatabase folder is the Object Folder

Objects can only be items that you can pickup/trade for the moment, but mocks are limitless in the possibilities of what you can add.

Pick an object like bike.obj and bike_albedo.png and put into Object folder.  It should load up the new Gameobject at the start. You can then wackydb_save_item to customize and/or make recipe. 

The object folder matches based on the preceding name so bike_ matches to bike

It looks for pngs   "_albedo" "_metallic" "_normal";



</details>


<details><summary> 1.xx ChangeLog</summary>
        
        Version 1.4.2
            Had to disable Piece snapshot because of hovering pieces stacking up on each other, hopefully someone fixes it someday.
            You might have to destroy the existing pieces at (0,0) with infinity hammer quite a lot depending on reloads and players joining.    
        Version 1.4.1
            Some items don't like snapshot icons - Added extra checks and only items with material changes get new icons
        Version 1.4.0
            Added DedServer load Memory config to allow people to see if loading Wackydb on DedServer helps or hinders. 
            extraSecurity - Allows people who don't want the extra cheat protection to disable it and not get 0.0.1 Error
            Big News! Added auto Icon Generation to cloned Items, and all Pieces with custom material(pieces angles are a little wonky or wacky if you will) - Thx Blaxx for code
        Version 1.3.6
            Added m_attackHealthPercentage and m_secAttackHealthPercentage- Warning any Items that uses this Must be recreated. s
            Otherwise default will go to 0. These items include the staffs that use a percentage of player health to power. 
        Version 1.3.5
            I have decided to add more parameters to Json file, so please do not use older version of Wackydb after upgrading. Wackydb 2.0 is not coming soon
            Existing Jsons are fine to use, you can regenerate them to get new values. 
            Added m_EitrCost, m_secEitrCost - These are attack costs for primary and secondary weapon attacks, no Eitr, no swing. 
            m_FoodEitr - Food Eitr amount, m_EitrRegen- Modifier to Eitr Regen - Very powerful on clothes, weapons, added more warnings. 
            Seperated out m_attackStamina and secondary m_attackStamina
        Version 1.3.2
            Mistlands Update: Removed extra Wet effect/restance since Mistlands adds its own. 
            Removed FoodColor, as it was removed from game and didn't really do anything.
        Version 1.3.0
            Hopefully fixed Co-Op hosting bug again..
        Version 1.2.9
            Updated ServerSync for 211.11
        Version 1.2.8
            Hopefully fixed issue with Co-op hosting. Added 0.0.1 Custom message back. 
        Version 1.2.7
            Updated ServerSync for crossplay - Custom Message for Ver 0.0.1 is not displayed. No Singleplayer before multiplayer without restart.
            Known issue of TrophyDraugr is not able to be set (targets TrophyDraugrFem) use Fem or clone TrophyDraugr. 
            Destroyer is spelled with "troy", also now a loginfo instead of warning
        Version 1.2.5
            Moved wackysDatabase to Config instead of Plugins folder to stop r2mod from deleting folder on updates.
            Warning 1.2.4 and Lower will delete wackysDatabase folder in Plugins on Update, please backup.
        Version 1.2.4 
            Expanded Recipe Compatibility to Recipe_ ( Modification only, no cloning),  Can now change any material's type,
            category, craftingstation instead of just clones. Cannot change piecehammer of non clones. You can now set the piece's 
            material at 50% health. If you only set 1 it sets to both "full health" and "half health" otherwise,
            "material1, material2" (full, half health). "same_mat" or "no_wear" sets pieces to have no wear material. 
            Updated ServerSync and PieceManager
        Version 1.1.9
            Bug Fixes. Cleaned up Logs
        Version 1.1.8
            Fixed two main bugs,
            Properly unloading cloned assets on logout.
            Made it so some errors are caught better.
            Incorporated Water Resistance as done by aedenthorn.
        Version 1.1.5
            Cleaned up the code a lot. Fixed Pieces from getting null values from Server.
            Fixed the piece disable/admin for custom pieces.
            Made it so you can clone stone_floor (4x4 stone prefab) - editing it probably won't make it add to Hammer
            Added special case list for objects that have multiple Gameobjects. (Bow, SpearBronze)
        Version 1.10
            All About Pieces with this Update!
            Adds ability to clone an existing CraftingStation piece and make it a new CraftingStation 
                - The CraftingStation name is "name", add recipes to it with this name.
            Fixed other mods custom pieces. You should be able access and even clone other mods pieces now.
            Added piecehammerCategory so you can change the category where piece appears on the hammer. 
                - Mods might use numbers instead of words though.
            Added m_knockback Added m_backstabbonus Made m_attackStamina set both Primary and Secondary attacks.
        Version 1.05
            Mod Release


</details>

<details><summary> Last notes</summary>

## Last notes:

This mod should load last. It needs to so it can touch all other mods. 

> You can make changes to that OP bow and make it more realistic on damage or build requirements. Or even set a build piece to adminonly.

> Clone the Item and change the material to make it a more appealing color. 

Submit pull requests to https://github.com/Wacky-Mole/WackysDatabase . The primary purpose of this mod is to edit objects, not to create clones/mocks. 

A mispelling like "Like <colorz = blue> Hi</color>?" or dmg modifier that is wrong can break Azu show container contents


(Note!: If you want the game to have default values, close the game and delete the wackysDatabase folder).

</details>


<details><summary>Full Features</summary>

Planned features
- [x] Able to modify item data.
- [x] Able to modify recipes.
- [x] Able to modify pieces.
- [x] Able to modify materials on clones
- [x] Custom items/pieces
- [x] Custom recipes
- [x] Able to modify Set effects 
- [x] Cloned Items show up on MainScreen
- [x] Adjust attack values of items
- [x] Able to add or remove conversions on smelter pieces
- [x] Able to change the size of anything
Wackymole

</details>

<summary><b><span style="color:aqua;font-weight:200;font-size:20px">2.xx ChangeLog</span></b></summary>

| Version | Changes                                                                                                                                                                                                                                                                                                                                |
|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 2.0.0 | - Release 2.0 <br/>


<details><summary>Feedback</summary>


For questions or suggestions please join discord channel: [Odin Plus Team](https://discord.gg/odinplus) or my discord at [Wolf Den](https://discord.gg/yPj7xjs3Xf)

Support me at https://www.buymeacoffee.com/WackyMole  or https://ko-fi.com/wackymole

<a href="https://www.buymeacoffee.com/WackyMole" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>

<a href='https://ko-fi.com/H2H6LL5GA' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

<img src="https://wackymole.com/hosts/bmc_qr.png" width="100"/>

</details> 


<details><summary>Credits</summary>

Credits:
Aedenthorn and all of his Many Mods! https://github.com/aedenthorn/ValheimMods </br>
Thank you AzumattDev for the template. It is very good https://github.com/AzumattDev/ItemManagerModTemplate </br>
Thanks to the Odin Discord server, for being active and good for the valheim community.</br>
CustomArmor code from https://github.com/aedenthorn/ValheimMods/blob/master/CustomArmorStats/BepInExPlugin.cs </br>
Thank you to Azumatt and the Odin team. </br>
Special thank you to @KG for Mock System </br>
A Huge thank you to Rexabit and his Visual Modifier https://github.com/Rexabit/valheim-visuals-modifier
</br>Azumatt for Status Editor contributions. </br>
Do whatever you want with this mod.
</details>