﻿// A Lot of the credit goes to  aedenthorn  and all of his Many Mods! https://github.com/aedenthorn/ValheimMods
// Thank you AzumattDev for the template. It is very good https://github.com/AzumattDev/ItemManagerModTemplate
// Thanks to the Odin Discord server, for being active and good for the valheim community.
// Do whatever you want with this mod. // except sale it as per Aedenthorn Permissions https://www.nexusmods.com/valheim/mods/1245
// Goal for this mod is RecipeCustomization + Recipe LVL station Requirement + Server Sync
// Taking from Azu OpenDatabase code and the orginal now. https://www.nexusmods.com/valheim/mods/319?tab=description
// CustomArmor code from https://github.com/aedenthorn/ValheimMods/blob/master/CustomArmorStats/BepInExPlugin.cs
// Thx Aedenthorn again
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ItemManager;
using ServerSync;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using System.Linq;
using PieceManager;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection.Emit;

namespace wackydatabase
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class WMRecipeCust : BaseUnityPlugin
    {
        internal const string ModName = "WackysDatabase";
        internal const string ModVersion = "1.0.6";
        internal const string Author = "WackyMole";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        public static ConfigEntry<string> NexusModID;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<bool> isautoreload;
        public static ConfigEntry<bool> isDebugString;
        private static bool issettoSinglePlayer = false;
        private static bool isSettoAutoReload;
        private static bool isSetStringisDebug = false;
        private static bool recieveServerInfo = false;
        private static bool NoMoreLoading = false; // for shutdown from Server
        internal static string ConnectionError = "";
        private static WMRecipeCust context;
        private static int kickcount = 0;
       // private static bool IsServer = false;
       // private static bool IsServer => (int)SystemInfo.get_graphicsDeviceType() == 4;
       // private static bool IsServer => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        //private static bool IsServer = SystemInfo.graphicsDeviceType();

        public static ConfigEntry<float> globalArmorDurabilityLossMult;
        public static ConfigEntry<float> globalArmorMovementModMult;
        public static ConfigEntry<string> waterModifierName;

        private static List<RecipeData> recipeDatas = new List<RecipeData>();
        private static List<WItemData> ItemDatas = new List<WItemData>();
        private static List<PieceData> PieceDatas = new List<PieceData>();
        private static List<ArmorData> armorDatas = new List<ArmorData>();// load with others
        private static List<string> Cloned = new List<string>();
        private static string assetPath;
        private static string assetPathItems;
        private static string assetPathRecipes;
        private static string assetPathPieces; 
        private static string jsonstring;
        private static bool Admin = true; // for single player, sets to false for multiplayer on connect
        private static List<string> pieceWithLvl = new List<string>();
        bool admin = !ConfigSync.IsLocked;
        readonly bool admin2 = ConfigSync.IsAdmin;
        private static GameObject Root;
        private static bool Firstrun = true;
        private static PieceTable selectedPiecehammer;
        private static List<string> piecemods = new List<string>();
        private static PieceTable[] MaybePieceStations;
        public static List<string> RealPieceStations = new List<string>();



        #region extra functions
        private enum NewDamageTypes
        {
            Water = 1024
        }


        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug.Value)
                Debug.Log((pref ? ModName + " " : "") + str);
        }

        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource WackysRecipeCustomizationLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
        { DisplayName = ModName,  MinimumRequiredVersion = "1.0.6" }; // it is very picky on version number


        #endregion


        public void Awake() // start
        {
            StartupConfig(); // startup varables 
            assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "wackysDatabase");
            assetPathItems = Path.Combine(assetPath, "Items");
            assetPathRecipes = Path.Combine(assetPath, "Recipes");
            assetPathPieces = Path.Combine(assetPath, "Pieces");
           // testme(); // function for testing things

            // ending files
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher(); // so if files change after startup it reloads recipes/ but doesn't input them.
            GetRecipeDataFromFilesForServer();
            skillConfigData.ValueChanged += CustomSyncEventDetected; // custom watcher for json file synced from server
 

        }
        public static bool SinglePlayerchecker
        {
            get { return issettoSinglePlayer; }
            set
            {
                issettoSinglePlayer = true;
                return;
            }
        }
        public static bool IsLocalInstance(ZNet znet)
        {
            if (znet.IsServer() && !znet.IsDedicated())
            {
                issettoSinglePlayer = true;
                ConfigSync.CurrentVersion = "0.0.1"; // kicking player from server
                WackysRecipeCustomizationLogger.LogWarning("You Will be kicked from Multiplayer Servers! " + ConfigSync.CurrentVersion);
            }
            return issettoSinglePlayer;
        }

        #region ConfigReading

        private static ConfigEntry<bool>? _serverConfigLocked;
        private static readonly CustomSyncedValue<string> skillConfigData = new(ConfigSync, "skillConfig", ""); // doesn't show up in config

        private void StartupConfig()
        {
            _serverConfigLocked = config("General", "Force Server Config", true, "Force Server Config");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Root = new GameObject("myroot");
            Root.SetActive(false);
            DontDestroyOnLoad(Root);

            // modded hammers to look for. Pieces only - JVL pieces don't really work, have to look for them a different way. 
            piecemods.Add("_ClutterPieceTable");
            piecemods.Add("_RKCustomTable");
            piecemods.Add("_PlantitPieceTable");
            piecemods.Add("BJORKSNAS");

            // ^^ // starting files
            context = this;
            modEnabled = config<bool>("General", "Enabled", true, "Enable this mod");
            NexusModID = config<string>("General", "NexusModID", "1825", "NexusModID Number", false);
            isDebug = config<bool>("General", "IsDebug", true, "Enable debug logs", false);
            isDebugString = config<bool>("General", "StringisDebug", false, "Do You want to see the String Debug Log - extra logs");
            isautoreload = config<bool>("General", "IsAutoReload", false, new ConfigDescription("Enable auto reload after wackydb_save or wackydb_clone for singleplayer", null, new ConfigurationManagerAttributes { Browsable = false }), false); // not browseable and can only be set before launch
            //isSinglePlayer = config<bool>("General", "IsSinglePlayerOnly", false, new ConfigDescription("Allow Single Player- Must be off for Multiplayer", null, new ConfigurationManagerAttributes { Browsable = false }), false); // doesn't allow you to connect if set to true
            ConfigSync.CurrentVersion = ModVersion;
            if (isDebugString.Value)
                isSetStringisDebug = true;

            WackysRecipeCustomizationLogger.LogDebug("Mod Version " + ConfigSync.CurrentVersion);
            if (isautoreload.Value)
                isSettoAutoReload = true;
            else isSettoAutoReload = false;

        }
        private void OnDestroy()
        {
            Config.Save();
            WackysRecipeCustomizationLogger.LogWarning("Calling the Destoryer of Worlds -End Game");
            //need to unload cloned objects
        }

        private void SetupWatcher()
        {
            CheckModFolder();
            /* // watch config files // cheating danger
            FileSystemWatcher watcher2 = new(Paths.ConfigPath, ConfigFileName);
            watcher2.Changed += ReadConfigValues;
            watcher2.Created += ReadConfigValues;
            watcher2.Renamed += ReadConfigValues;
            watcher2.IncludeSubdirectories = true;
            watcher2.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher2.EnableRaisingEvents = true;
            */

            FileSystemWatcher watcher = new(assetPath); // jsons
            watcher.Changed += ReadJsonValues;
            watcher.Created += ReadJsonValues;
            watcher.Renamed += ReadJsonValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Config.Reload();
            }
            catch { WackysRecipeCustomizationLogger.LogError($"There was an issue loading Config File "); }

        }
        private void ReadJsonValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
               if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated() || issettoSinglePlayer && isSettoAutoReload) {  // should only load for the server now
                 Dbgl("Jsons files have changed and access is either on a dedicated server or singleplayer with autoreload on therefore reloading everything");
                 GetRecipeDataFromFiles(); // load stuff in mem
                 skillConfigData.Value = jsonstring; //Sync Event // Single player forces client to reload as well. 
                    }
            }
            catch
            {
                //WackysRecipeCustomizationLogger.LogError($"There was an issue loading your Sync ");
                if (issettoSinglePlayer)
                    WackysRecipeCustomizationLogger.LogError("Please check your JSON entries for spelling and format!");
                else
                {
                    WackysRecipeCustomizationLogger.LogDebug("Not checking Json Files because either in Main Screen or ....");
                }
            }
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }


        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        private static void LoadAllRecipeData(bool reload)
        {
            if (reload)
            {
                ZNet Net = new ZNet();
                IsLocalInstance(Net);
            }
            if (reload && (issettoSinglePlayer || recieveServerInfo) ) // single player only or recievedServerInfo
                {
                    if (recieveServerInfo && issettoSinglePlayer)
                    {
                        WackysRecipeCustomizationLogger.LogWarning($" You Loaded into Singleplayer local first and therefore will NOT be allowed to reload Server Configs");
                        return; // naughty boy no recipes for you
                    }
                    else
                    {
                        GetRecipeDataFromFiles();
                        foreach (var data in ItemDatas) // call items first
                        {
                            SetItemData(data);
                        }
                        foreach (var data in recipeDatas)
                        {
                            // Dbgl($"trying to load recipes");
                            SetRecipeData(data);
                        }
                        foreach (var data in PieceDatas)
                        {
                            SetPieceRecipeData(data);
                        }

                    }
                           // move to the end of updating all componets
                    try { 
                            ObjectDB.instance.UpdateItemHashes();
                    } catch
                    {
                        Dbgl($"failed to update Hashes- probably due to too many calls");
                    }
             } 
            else {
               
                Dbgl($" You did NOT reload LOCAL Files");
            }
        }
        //private static get private admin
        private void CustomSyncEventDetected()
        {
            if (Firstrun)
            {
                GetAllMaterials();
                Firstrun = false;
                GetPieceStations();
            }
            if (NoMoreLoading)
            {
                //startupSync++;
                recieveServerInfo = true;
                NoMoreLoading = false;
                Dbgl($" No More Loading was true");
                WackysRecipeCustomizationLogger.LogWarning("Warning any ServerFiles will see be On Your Local Games Until Restart! ");
            }
            else
            {
                WackysRecipeCustomizationLogger.LogDebug("CustomSyncEventDetected was called ");
                Dbgl($" You did reload SERVER Files");
                Admin = admin2;
                recipeDatas.Clear();
                ItemDatas.Clear();
                PieceDatas.Clear();
                armorDatas.Clear();
               // GetAllMaterials(); // disable this, called from server start now?
                string SyncedString = skillConfigData.Value;
                if (SyncedString != null && SyncedString != "")
                {
                    WackysRecipeCustomizationLogger.LogDebug("Synced String was  " + SyncedString);
                    string[] jsons = SyncedString.Split('@');
                    foreach (var word in jsons) // Should really do a first pass for clones?
                    {
                        if (word.Contains("m_name")) //item
                        {
                            WItemData data2 = JsonUtility.FromJson<WItemData>(word);
                            ItemDatas.Add(data2);
                            ArmorData data3 = JsonUtility.FromJson<ArmorData>(word);
                            armorDatas.Add(data3);

                        }
                        else if (word.Contains("piecehammer"))
                        {
                            PieceData data = JsonUtility.FromJson<PieceData>(word);
                            PieceDatas.Add(data);
                        }
                        else
                        {
                            RecipeData data = JsonUtility.FromJson<RecipeData>(word);
                            recipeDatas.Add(data);
                        }

                        //WackysRecipeCustomizationLogger.LogDebug(word);
                    }
                    pieceWithLvl.Clear(); // ready for new
                    foreach (var data2 in ItemDatas)
                    {
                        if (data2 != null)
                        {
                            SetItemData(data2);
                        }
                    }
                    foreach (var data in recipeDatas)
                    {
                        if (data != null)
                        {
                            SetRecipeData(data);
                        }
                    }
                    foreach (var data3 in PieceDatas)
                    {
                        if (data3 != null)
                        {
                            SetPieceRecipeData(data3);
                        }
                    }

                    WackysRecipeCustomizationLogger.LogDebug("done with customSyncEvent");
                }
                else
                {
                    WackysRecipeCustomizationLogger.LogDebug("Synced String was blank " + SyncedString);
                }
                // isaclient = true; // don't allow reload
            }

        }

        private static void GetRecipeDataFromFiles()
        {
            if (Firstrun)
            {
                CheckModFolder();
                GetAllMaterials();
                GetPieceStations();
                Firstrun = false;
            }
            recipeDatas.Clear();
            ItemDatas.Clear();
            PieceDatas.Clear();
            armorDatas.Clear();
            var amber = new System.Text.StringBuilder();
            foreach (string file in Directory.GetFiles(assetPath, "*.json", SearchOption.AllDirectories))
            {
                if (file.Contains("Item") || file.Contains("item")) // items are being rather mean with the damage classes
                {
                    //Items = new List<WDamages>();//string content = File.ReadAllText(file);
                    // string[] divide = content.Split('}');
                    // WItemData data = JsonUtility.FromJson<WItemData>(content);// WItemData data = JsonUtility.FromJson<WItemData>(divide[0]);
                    try
                    {
                        WItemData data = JsonUtility.FromJson<WItemData>(File.ReadAllText(file));
                        amber.Append(File.ReadAllText(file));
                        amber.Append("@");
                        ItemDatas.Add(data);
                        ArmorData data3 = JsonUtility.FromJson<ArmorData>(File.ReadAllText(file));
                        armorDatas.Add(data3);
                    }
                    catch { WackysRecipeCustomizationLogger.LogWarning("Something went wrong in file " + file); }

                }
                else if (file.Contains("Piece") || file.Contains("piece"))
                {
                    try
                    {
                        PieceData data = JsonUtility.FromJson<PieceData>(File.ReadAllText(file));
                        amber.Append(File.ReadAllText(file));
                        amber.Append("@");
                        PieceDatas.Add(data);
                    }
                    catch { WackysRecipeCustomizationLogger.LogWarning("Something went wrong in file " + file); }
                }
                else // recipes
                {
                    try
                    {
                        RecipeData data = JsonUtility.FromJson<RecipeData>(File.ReadAllText(file));
                        amber.Append(File.ReadAllText(file));
                        amber.Append("@");
                        recipeDatas.Add(data);
                    }
                    catch { WackysRecipeCustomizationLogger.LogWarning("Something went wrong in file " + file); }

                }
            }

            jsonstring = amber.ToString();
            // skillConfigData.Value = jsonstring; Only for server 1st time
            if (isSetStringisDebug)
                Dbgl(jsonstring); 
        }
        private static void GetRecipeDataFromFilesForServer()
        {
            CheckModFolder();
            var amber = new System.Text.StringBuilder();
            foreach (string file in Directory.GetFiles(assetPath, "*.json", SearchOption.AllDirectories))
            {
                if (file.Contains("Item") || file.Contains("item")) // items are being rather mean with the damage classes
                {
                    try
                    {
                        WItemData data = JsonUtility.FromJson<WItemData>(File.ReadAllText(file));
                        amber.Append(File.ReadAllText(file));
                        amber.Append("@");
                        ItemDatas.Add(data);
                        ArmorData data3 = JsonUtility.FromJson<ArmorData>(File.ReadAllText(file));
                        armorDatas.Add(data3);
                    }
                    catch { WackysRecipeCustomizationLogger.LogWarning("Something went wrong in file " + file); }

                }
                else if (file.Contains("Piece") || file.Contains("piece"))
                {
                    try
                    {
                        PieceData data = JsonUtility.FromJson<PieceData>(File.ReadAllText(file));
                        amber.Append(File.ReadAllText(file));
                        amber.Append("@");
                        PieceDatas.Add(data);
                    }
                    catch { WackysRecipeCustomizationLogger.LogWarning("Something went wrong in file " + file); }
                }
                else // recipes
                {
                    try
                    {
                        RecipeData data = JsonUtility.FromJson<RecipeData>(File.ReadAllText(file));
                        amber.Append(File.ReadAllText(file));
                        amber.Append("@");
                        recipeDatas.Add(data);
                    }
                    catch { WackysRecipeCustomizationLogger.LogWarning("Something went wrong in file " + file); }

                }
            }
            jsonstring = amber.ToString();
            skillConfigData.Value = jsonstring;
            
            WackysRecipeCustomizationLogger.LogDebug("Loaded files");
        }

        private static void CheckModFolder()
        {
            if (!Directory.Exists(assetPath))
            {
                Dbgl("Creating mod folder");
                Directory.CreateDirectory(assetPath);
                Directory.CreateDirectory(assetPathItems);
                Directory.CreateDirectory(assetPathPieces);  
                Directory.CreateDirectory(assetPathRecipes);
            }
            if (!Directory.Exists(assetPathItems))
            {
                Directory.CreateDirectory(assetPathItems);
                Directory.CreateDirectory(assetPathPieces);
                Directory.CreateDirectory(assetPathRecipes);
            }
        }

        #endregion
        #region Set Object

        private static Vector3 tempvalue;
        private static void SetRecipeData(RecipeData data)
        {
            bool skip = false;
            foreach (var citem in Cloned)
            {
                if (citem == data.name)
                    skip = true;   
            }
            string tempname = data.name;
            if (data.clone) // both skip and
            {
                data.name = data.clonePrefabName;
            }

            GameObject go = ObjectDB.instance.GetItemPrefab(data.name);
            if (go == null)
            {  
                //SetPieceRecipeData(data);
                Dbgl("maybe null " + data.name  +" Should not get here");
                return;
            }

            if (go.GetComponent<ItemDrop>() == null)
            {
                Dbgl($"Item recipe data for {data.name} not found!");
                return;
            } // it is a prefab and it is an item.
            if (data.clone && !skip)
            {
                if (!data.disabled)
                {
                    Dbgl("Setting CLONED Recipe for " + tempname);
                    Recipe clonerecipe = ScriptableObject.CreateInstance<Recipe>();
                    Cloned.Add(tempname); // add to list

                    clonerecipe.m_item = go.GetComponent<ItemDrop>();
                    clonerecipe.m_craftingStation = GetCraftingStation(data.craftingStation);
                    clonerecipe.m_minStationLevel = data.minStationLevel;
                    clonerecipe.m_amount = data.amount;
                    clonerecipe.name = tempname; //maybe
                   // clonerecipe.name = $"<color =#4f34eb>{tempname}</color>";

                    List <Piece.Requirement> reqs = new List<Piece.Requirement>();

                    // Dbgl("Made it to RecipeData!");
                    foreach (string req in data.reqs)
                    {
                        if (!string.IsNullOrEmpty(req))
                        {
                            string[] array = req.Split(':'); // safer vewrsion
                            string itemname = array[0];
                            if (ObjectDB.instance.GetItemPrefab(itemname))
                            {
                                int amount = ((array.Length < 2) ? 1 : int.Parse(array[1]));
                                int amountPerLevel = ((array.Length < 3) ? 1 : int.Parse(array[2]));
                                bool recover = array.Length != 4 || bool.Parse(array[3]);
                                Piece.Requirement item = new Piece.Requirement
                                {
                                    m_amount = amount,
                                    m_recover = recover,
                                    m_resItem = ObjectDB.instance.GetItemPrefab(itemname).GetComponent<ItemDrop>(),
                                    m_amountPerLevel = amountPerLevel
                                };
                                reqs.Add(item);
                            }
                        }
                    }// foreach
                    int index = 0;
                    clonerecipe.m_resources = reqs.ToArray();
                    for (int i = ObjectDB.instance.m_recipes.Count - 1; i > 0; i--)
                    {
                        if (ObjectDB.instance.m_recipes[i].m_item?.m_itemData.m_shared.m_name == go.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)
                        {
                            index = i++; // some extra resourses, but I think it's worth it
                            break;
                        }
                    }
                    ObjectDB.instance.m_recipes.Insert(index, clonerecipe);
                }
                else
                {
                    Dbgl("Cloned Recipe is disabled for " + data.clonePrefabName + " Will not unload if already loaded");
                    return;
                }

            }
            else if (skip) // if a previous clone
            {
                for (int i = ObjectDB.instance.m_recipes.Count - 1; i > 0; i--)
                {
                    if (ObjectDB.instance.m_recipes[i].name == tempname)
                    {
                        Dbgl("ReSetting Recipe for " + tempname);
                        Recipe clonerecipe = ObjectDB.instance.m_recipes[i];
                        clonerecipe.m_item = go.GetComponent<ItemDrop>();
                        clonerecipe.m_craftingStation = GetCraftingStation(data.craftingStation);
                        clonerecipe.m_minStationLevel = data.minStationLevel;
                        clonerecipe.m_amount = data.amount;
                        clonerecipe.name = tempname; //maybe

                        List<Piece.Requirement> reqs = new List<Piece.Requirement>();

                        // Dbgl("Made it to RecipeData!");  
                        foreach (string req in data.reqs)
                        {
                            if (!string.IsNullOrEmpty(req))
                            {
                                string[] array = req.Split(':'); // safer vewrsion
                                string itemname = array[0];
                                if (ObjectDB.instance.GetItemPrefab(itemname))
                                {
                                    int amount = ((array.Length < 2) ? 1 : int.Parse(array[1]));
                                    int amountPerLevel = ((array.Length < 3) ? 1 : int.Parse(array[2]));
                                    bool recover = array.Length != 4 || bool.Parse(array[3]);
                                    Piece.Requirement item = new Piece.Requirement
                                    {
                                        m_amount = amount,
                                        m_recover = recover,
                                        m_resItem = ObjectDB.instance.GetItemPrefab(itemname).GetComponent<ItemDrop>(),
                                        m_amountPerLevel = amountPerLevel
                                    };
                                    reqs.Add(item);
                                }
                            }
                        }// foreach
                        clonerecipe.m_resources = reqs.ToArray();
                    }
                }
            }else // ingame item
            {

                for (int i = ObjectDB.instance.m_recipes.Count - 1; i > 0; i--)
                {
                    if (ObjectDB.instance.m_recipes[i].m_item?.m_itemData.m_shared.m_name == go.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)
                    {
                        // if not clone normal edit
                        Dbgl("Setting Recipe for " + data.name);
                        if (data.disabled)
                        {
                            Dbgl($"Removing recipe for {data.name} from the game");
                            ObjectDB.instance.m_recipes.RemoveAt(i);
                            return;
                        }

                        ObjectDB.instance.m_recipes[i].m_amount = data.amount;
                        ObjectDB.instance.m_recipes[i].m_minStationLevel = data.minStationLevel;
                        ObjectDB.instance.m_recipes[i].m_craftingStation = GetCraftingStation(data.craftingStation);
                        List<Piece.Requirement> reqs = new List<Piece.Requirement>();
                        // Dbgl("Made it to RecipeData!");
                        foreach (string req in data.reqs)
                        {
                            string[] parts = req.Split(':');
                            reqs.Add(new Piece.Requirement() { m_resItem = ObjectDB.instance.GetItemPrefab(parts[0]).GetComponent<ItemDrop>(), m_amount = int.Parse(parts[1]), m_amountPerLevel = int.Parse(parts[2]), m_recover = parts[3].ToLower() == "true" });
                        }
                        // Dbgl("Amost done with RecipeData!");
                        ObjectDB.instance.m_recipes[i].m_resources = reqs.ToArray();
                        return;
                    } // end normal
                } // checking recipes
            }
        }

        private static void SetPieceRecipeData(PieceData data)
        {
            bool skip = false;
            foreach (var citem in Cloned)
            {
                if (citem == data.name)
                    skip = true;
            }
            string tempname = data.name;
            if (data.clone && !skip)
            {
                data.name = data.clonePrefabName;
            }
            Piece piece = null;
            GameObject go = GetPieces().Find(g => Utils.GetPrefabName(g) == data.name); // vanilla search
            if (go == null)
            {
                go = GetModdedPieces(data.name); // known modded Hammer search
                if (go == null)
                {
                    go = ObjectDB.instance.GetItemPrefab(data.name); // last chance for modded hammers before JVL
                    if (go == null)
                    {
                        if (Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn")) // JVL hammer search
                        {
                            object PieceManager = Chainloader.PluginInfos["com.jotunn.jotunn"].Instance.GetType().Assembly.GetType("Jotunn.Managers.PieceManager").GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                            object cr = AccessTools.Method(PieceManager.GetType(), "GetPiece").Invoke(PieceManager, new[] { data.name });
                            if (cr != null)
                            {
                                piece = (Piece)AccessTools.Property(cr.GetType(), "Piece").GetValue(cr);
                                Dbgl($"Jotunn recipe: found for {data.name}");
                                go = piece.gameObject;
                            }
                        }
                        if (go == null) // 4th layer
                        {
                            Dbgl($"Piece {data.name} not found! 4 layer search");
                            return;
                        }
                    }
                    else // 3rd layer
                    {
                        Dbgl($"Piece {data.name} from itself ");
                        piece = go.GetComponent<Piece>();
                    }
                }
                else // 2nd layer
                {
                    Dbgl($"Piece {data.name} from known hammer {selectedPiecehammer}");
                    piece = go.GetComponent<Piece>();
                }
            }
            else // 1st layer
            {
                piece = go.GetComponent<Piece>();
            }
            if (piece == null) // final check
            {
                Dbgl("Piece data not found!");
                return;
            }
                if (data.clone && !skip) // object is a clone do clonethings
                {
                Dbgl($"Item CLONE DATA in SetPiece for {tempname} ");
                Transform RootT = Root.transform; // Root set to inactive to perserve components. 
                GameObject newItem = Instantiate(go, RootT, false);
                Piece NewItemComp = newItem.GetComponent<Piece>();

                Cloned.Add(tempname); // check against
                newItem.name = tempname; // resets the orginal name- needs to be unquie
                NewItemComp.name = tempname; // ingame name
                 
                var hash = newItem.name.GetStableHashCode();   
                ZNetScene znet = ZNetScene.instance;
                if (znet)
                {
                    string name = newItem.name;
                    if (znet.m_namedPrefabs.ContainsKey(hash))
                    {
                        Dbgl($"Prefab {name} already in ZNetScene");
                    }
                    else
                    {
                        if (newItem.GetComponent<ZNetView>() != null)
                        {
                            znet.m_prefabs.Add(newItem);
                        }
                        else
                        {
                            znet.m_nonNetViewPrefabs.Add(newItem);
                        }
                        znet.m_namedPrefabs.Add(hash, newItem);
                        Dbgl($"Added prefab {name}");
                    }
                }
                CraftingStation craft = GetCraftingStation(data.craftingStation);
                newItem.GetComponent<Piece>().m_craftingStation = craft; // sets crafing item place

                if (!string.IsNullOrEmpty(data.cloneMaterial))
                {
                    Dbgl($"Material name searching for {data.cloneMaterial}");
                    try
                    {
                        renderfinder = newItem.GetComponentsInChildren<Renderer>();// "weapons1_fire" glowing orange
                        if (data.cloneMaterial.Contains(','))
                        {
                            string[] materialstr = data.cloneMaterial.Split(',');
                            Material mat = originalMaterials[materialstr[0]];
                            Material part = originalMaterials[materialstr[1]];

                            foreach (Renderer renderitem in renderfinder)
                            {
                                if (renderitem.receiveShadows && materialstr[0] != "none")
                                    renderitem.material = mat;
                                else if (!renderitem.receiveShadows)
                                    renderitem.material = part;
                            }
                        }else
                        {
                            Material mat = originalMaterials[data.cloneMaterial];
                            foreach (Renderer renderitem in renderfinder)
                            {
                                if (renderitem.receiveShadows )
                                    renderitem.material = mat;
                            }
                        }
                    }
                    catch { WackysRecipeCustomizationLogger.LogWarning("Material was not found or was not set correctly"); }
                }

                GameObject piecehammer = ObjectDB.instance.GetItemPrefab(data.piecehammer);
                if (piecehammer == null)
                {
                    if (selectedPiecehammer == null)
                    {
                        Dbgl($"piecehammer named {data.piecehammer} will not be use because the Item prefab was not found and it is not a PieceTable, so setting to Hammer in Misc");
                        piecehammer = ObjectDB.instance.GetItemPrefab("Hammer");

                        newItem.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces.m_selectedCategory = Piece.PieceCategory.Misc; // set the category
                        piecehammer.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces.m_pieces.Add(newItem);
                    }
                    else
                    {
                        selectedPiecehammer.m_pieces.Add(newItem); // adding item to PiceTable
                    }
                }
                else
                {
                    piecehammer?.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces.m_pieces.Add(newItem); // if piecehammer is the actual item and not the PieceTable
                }
                 
                data.name = tempname; // putting back name
                go = GetPieces().Find(g => Utils.GetPrefabName(g) == data.name); // just verifying
                {
                    Dbgl($"Item {data.name} not found in SetPiece! after clone");
                    return;
                }
                if (go.GetComponent<Piece>() == null)
                {
                    Dbgl($"Item data for {data.name} not found! after clone");
                    return;
                }
                go.GetComponent<Piece>().m_name = tempname; // set pieces name
            } // end clone 
            if (data.adminonly)
            {
                if (Admin)
                {
                    // do nothing
                }else
                {
                    data.disabled = true;
                    Dbgl($"{data.name} is set for Adminonly, you are not an admin");

                }
            }

            if (data.disabled)
            {
                Dbgl($"Removing recipe for {data.name} from the game");

                ItemDrop hammer = ObjectDB.instance.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>();
                ItemDrop hoe = ObjectDB.instance.GetItemPrefab("Hoe")?.GetComponent<ItemDrop>();
                if (hammer && hammer.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(go))
                {
                    hammer.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(go);
                    return;
                }
                else if (hoe && hoe.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(go))
                {
                    hoe.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(go);
                    return;
                }
                else
                {
                    Dbgl($"Tryiny to remove Item {data.name} from custom Hammer");
                    ItemDrop hammertry = ObjectDB.instance.GetItemPrefab(data.piecehammer)?.GetComponent<ItemDrop>();
                    if (hammertry && hammertry.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(go))
                    {
                        hammertry.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(go);
                        return;
                    }
                }

            }
            Dbgl("Setting Piece data for " + data.name);
            if (string.IsNullOrEmpty(data.m_name)) { 
            
            }else
            {
                go.GetComponent<Piece>().m_name = data.m_name;
                go.GetComponent<Piece>().m_description = data.m_description;
            }
            CraftingStation currentStation = GetCraftingStation(data.craftingStation);
            CraftingStation checkifStation = null;
            if (data.clone)
                 checkifStation = GetCraftingStation(data.clonePrefabName);// check if they are cloning a crafting station.
            if (data.clone && checkifStation != null)
            {
                checkifStation.m_name = "$"+ data.name;
                Dbgl($"Congratulations on your new CraftingStation named {checkifStation} ");
            }
            go.GetComponent<Piece>().m_craftingStation = GetCraftingStation(data.craftingStation);
            if (data.minStationLevel > 1)
            {
                pieceWithLvl.Add(go.name + "." + data.minStationLevel);
            }
            List<Piece.Requirement> reqs = new List<Piece.Requirement>();
            foreach (string req in data.reqs)
            {
                string[] parts = req.Split(':');
                reqs.Add(new Piece.Requirement() { m_resItem = ObjectDB.instance.GetItemPrefab(parts[0]).GetComponent<ItemDrop>(), m_amount = int.Parse(parts[1]), m_amountPerLevel = int.Parse(parts[2]), m_recover = parts[3].ToLower() == "true" });
            }

            go.GetComponent<Piece>().m_resources = reqs.ToArray();

        }

        public static Component[] renderfinder;
        private static void SetItemData(WItemData data)
        {
            // Dbgl("Loaded SetItemData!");
            bool skip = false;
            foreach (var citem in Cloned)
            {
                if (citem == data.name)
                    skip = true;
            }
            string tempname = data.name;
            if (data.clone && !skip)
            {
                data.name = data.clonePrefabName;
            }
            GameObject go = ObjectDB.instance.GetItemPrefab(data.name);

            if (go == null)
            {
                Dbgl(" item in SetItemData null " + data.name);
                return;
            }
            if (go.GetComponent<ItemDrop>() == null)
            {
                Dbgl($"Item data in SetItemData for {data.name} not found!");
                return;
            } // it is a prefab and it is an item.
            if (string.IsNullOrEmpty(tempname) && data.clone)
            {
                Dbgl($"Item cloned name is empty!");
                return;
            }
            for (int i = ObjectDB.instance.m_items.Count - 1; i > 0; i--)  // need to handle clones
            {
                if (ObjectDB.instance.m_items[i]?.GetComponent<ItemDrop>().m_itemData.m_shared.m_name == go.GetComponent<ItemDrop>().m_itemData.m_shared.m_name) //if (ObjectDB.instance.m_recipes[i].m_item?.m_itemData.m_shared.m_name == go.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)
                {
                    ItemDrop.ItemData PrimaryItemData = ObjectDB.instance.m_items[i].GetComponent<ItemDrop>().m_itemData;
                    if (data.clone && !skip) // object is a clone do clonethings
                    {
                        Dbgl($"Item CLONE DATA in SetItemData for {tempname} ");
                        Cloned.Add(tempname);
                        Transform RootT = Root.transform; // Root set to inactive to perserve components. 
                        GameObject newItem = Instantiate(go, RootT, false); 
                        ItemDrop NewItemComp = newItem.GetComponent<ItemDrop>();
   
                        NewItemComp.name = tempname; // added and seems to be the issue
                        newItem.name = tempname; // resets the orginal name- needs to be unquie
                        NewItemComp.m_itemData.m_shared.m_name = data.m_name; // ingame name
                        var hash = newItem.name.GetStableHashCode();
                        ObjectDB.instance.m_items.Add(newItem);

                        ZNetScene znet = ZNetScene.instance;
                        if (znet)
                        {
                            string name = newItem.name;
                            if (znet.m_namedPrefabs.ContainsKey(hash))
                                WackysRecipeCustomizationLogger.LogWarning($"Prefab {name} already in ZNetScene");
                            else
                            {
                                if (newItem.GetComponent<ZNetView>() != null)
                                    znet.m_prefabs.Add(newItem);
                                else
                                    znet.m_nonNetViewPrefabs.Add(newItem);

                                znet.m_namedPrefabs.Add(hash, newItem);
                                Dbgl($"Added prefab {name}");    
                            }
                        }
                       
                         ObjectDB.instance.UpdateItemHashes();
                        if (!string.IsNullOrEmpty(data.cloneMaterial))
                        {
                            Dbgl($"Material name searching for {data.cloneMaterial}");
                            try
                            {
                                renderfinder = newItem.GetComponentsInChildren<Renderer>();// "weapons1_fire" glowing orange
                                if (data.cloneMaterial.Contains(','))
                                {
                                    string[] materialstr = data.cloneMaterial.Split(',');
                                    Material mat = originalMaterials[materialstr[0]];
                                    Material part = originalMaterials[materialstr[1]];

                                    foreach (Renderer renderitem in renderfinder)
                                    {
                                        if (renderitem.receiveShadows && materialstr[0] != "none")
                                            renderitem.material = mat;
                                        else if (!renderitem.receiveShadows)
                                            renderitem.material = part;
                                    }
                                }
                                else
                                {
                                    Material mat = originalMaterials[data.cloneMaterial];
                                    foreach (Renderer renderitem in renderfinder)
                                    {
                                        if (renderitem.receiveShadows)
                                            renderitem.material = mat;
                                    }
                                }
                            }
                            catch { WackysRecipeCustomizationLogger.LogWarning("Material was not found or was not set correctly"); }
                        }

                        PrimaryItemData = ObjectDB.instance.GetItemPrefab(tempname).GetComponent<ItemDrop>().m_itemData; // get ready to set stuff
                         data.name = tempname; // putting back name
                        try { 
                            // ObjectDB.instance.UpdateItemHashes();
                            } catch
                        {
                            Dbgl($"Item {tempname} failed to update Hashes");
                        }
                    }
                    Dbgl($"Item being Set in SetItemData for {data.name} ");

                    if (data.m_damages != null && data.m_damages != "")
                    {
                        Dbgl($"   {data.name} Item has damage values ");
                        // has to be in order, should be
                        char[] delims = new[] { ',' };
                        string[] divideme = data.m_damages.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                        //Dbgl($"Item damge for 0 {divideme[0]} " + $" Item damge for 10 {divideme[10]} ");
                        HitData.DamageTypes damages = default(HitData.DamageTypes);
                        damages.m_blunt = stringtoFloat(divideme[0]);
                        damages.m_chop = stringtoFloat(divideme[1]);
                        damages.m_damage = stringtoFloat(divideme[2]);
                        damages.m_fire = stringtoFloat(divideme[3]);
                        damages.m_frost = stringtoFloat(divideme[4]);
                        damages.m_lightning = stringtoFloat(divideme[5]);
                        damages.m_pickaxe = stringtoFloat(divideme[6]);
                        damages.m_pierce = stringtoFloat(divideme[7]);
                        damages.m_poison = stringtoFloat(divideme[8]);
                        damages.m_slash = stringtoFloat(divideme[9]);
                        damages.m_spirit = stringtoFloat(divideme[10]);
                        PrimaryItemData.m_shared.m_damages = damages;
                    }
                    if (data.m_damagesPerLevel != null && data.m_damagesPerLevel != "")
                    {
                        char[] delims = new[] { ',' };
                        string[] divideme = data.m_damagesPerLevel.Split(delims, StringSplitOptions.RemoveEmptyEntries);

                        HitData.DamageTypes damagesPerLevel = default(HitData.DamageTypes);
                        damagesPerLevel.m_blunt = stringtoFloat(divideme[0]);
                        damagesPerLevel.m_chop = stringtoFloat(divideme[1]);
                        damagesPerLevel.m_damage = stringtoFloat(divideme[2]);
                        damagesPerLevel.m_fire = stringtoFloat(divideme[3]);
                        damagesPerLevel.m_frost = stringtoFloat(divideme[4]);
                        damagesPerLevel.m_lightning = stringtoFloat(divideme[5]);
                        damagesPerLevel.m_pickaxe = stringtoFloat(divideme[6]);
                        damagesPerLevel.m_pierce = stringtoFloat(divideme[7]);
                        damagesPerLevel.m_poison = stringtoFloat(divideme[8]);
                        damagesPerLevel.m_slash = stringtoFloat(divideme[9]);
                        damagesPerLevel.m_spirit = stringtoFloat(divideme[10]);
                        PrimaryItemData.m_shared.m_damagesPerLevel = damagesPerLevel;
                    }
                    PrimaryItemData.m_shared.m_name = data.m_name;
                    PrimaryItemData.m_shared.m_description = data.m_description;
                    PrimaryItemData.m_shared.m_weight = data.m_weight;
                    PrimaryItemData.m_shared.m_maxStackSize = data.m_maxStackSize;
                    PrimaryItemData.m_shared.m_food = data.m_foodHealth;
                    PrimaryItemData.m_shared.m_foodStamina = data.m_foodStamina;
                    PrimaryItemData.m_shared.m_foodRegen = data.m_foodRegen;
                    PrimaryItemData.m_shared.m_foodBurnTime = data.m_foodBurnTime;
                    if (data.m_foodColor != null && data.m_foodColor != "" && data.m_foodColor.StartsWith("#"))
                    {
                        PrimaryItemData.m_shared.m_foodColor = ColorUtil.GetColorFromHex(data.m_foodColor);
                    }
                    PrimaryItemData.m_shared.m_armor = data.m_armor;
                    PrimaryItemData.m_shared.m_armorPerLevel = data.m_armorPerLevel;
                    PrimaryItemData.m_shared.m_blockPower = data.m_blockPower;
                    PrimaryItemData.m_shared.m_blockPowerPerLevel = data.m_blockPowerPerLevel;
                    PrimaryItemData.m_shared.m_canBeReparied = data.m_canBeReparied;
                    PrimaryItemData.m_shared.m_timedBlockBonus = data.m_timedBlockBonus;
                    PrimaryItemData.m_shared.m_deflectionForce = data.m_deflectionForce;
                    PrimaryItemData.m_shared.m_deflectionForcePerLevel = data.m_deflectionForcePerLevel;
                    PrimaryItemData.m_shared.m_backstabBonus = data.m_backstabbonus;
                    PrimaryItemData.m_shared.m_destroyBroken = data.m_destroyBroken;
                    PrimaryItemData.m_shared.m_dodgeable = data.m_dodgeable;
                    PrimaryItemData.m_shared.m_maxDurability = data.m_maxDurability;
                    PrimaryItemData.m_shared.m_durabilityDrain = data.m_durabilityDrain;
                    PrimaryItemData.m_shared.m_durabilityPerLevel = data.m_durabilityPerLevel;
                    PrimaryItemData.m_shared.m_equipDuration = data.m_equipDuration;
                    PrimaryItemData.m_shared.m_holdDurationMin = data.m_holdDurationMin;
                    PrimaryItemData.m_shared.m_holdStaminaDrain = data.m_holdStaminaDrain;
                    PrimaryItemData.m_shared.m_maxQuality = data.m_maxQuality;
                    PrimaryItemData.m_shared.m_useDurability = data.m_useDurability;
                    PrimaryItemData.m_shared.m_useDurabilityDrain = data.m_useDurabilityDrain;
                    PrimaryItemData.m_shared.m_questItem = data.m_questItem;
                    PrimaryItemData.m_shared.m_teleportable = data.m_teleportable;
                    PrimaryItemData.m_shared.m_toolTier = data.m_toolTier;
                    PrimaryItemData.m_shared.m_value = data.m_value;
                    PrimaryItemData.m_shared.m_movementModifier = data.m_movementModifier;
                    PrimaryItemData.m_shared.m_attack.m_attackStamina = data.m_attackStamina;
                    PrimaryItemData.m_shared.m_secondaryAttack.m_attackStamina = data.m_attackStamina; // set for both
                    PrimaryItemData.m_shared.m_attackForce = data.m_knockback;
                    // someone is going to complain that I am adding too many... I just know it.
                    int skillme = Enum.TryParse<Skills.SkillType>(data.m_skillType, out Skills.SkillType skillresult) ? (int)skillresult : (int)Enum.Parse(typeof(Skills.SkillType), data.m_skillType);
                    PrimaryItemData.m_shared.m_skillType = (Skills.SkillType)skillme;
                    PrimaryItemData.m_shared.m_attack = data.primaryAttack;
                    PrimaryItemData.m_shared.m_holdAnimationState = data.m_holdAnimationState;
                    PrimaryItemData.m_shared.m_animationState = (ItemDrop.ItemData.AnimationState)data.m_animationState;
                    ItemDrop.ItemData.AnimationState
                    Attack
                    /* What do I want
                     * m_speedFactor
                     * m_speedFactorRotation
                     * m_staggerMultiplier
                     * AttackType
                     * m_attackChainLevels ? doesn't change animation so probably no?
                     * m_attackStamina
                     * m_forceMultiplier
                     * m_attackStartNoise
                     * m_attackHitNoise
                     * m_attackRange
                     * m_attackHeight
                     * m_consumeItem
                     * m_attackAnimation
                     * m_spawnOnTrigger
                     * m_lastChainDamageMultiplier
                     * m_multiHit
                     * m_attackProjectile
                     * m_projectileVel
                     * m_projectileVelMin
                     * m_projectileAccuracy
                     * m_projectileAccuracyMin
                     * m_projectiles
                     * m_projectileBursts
                     * m_burstInterval
                     * m_destroyPreviousProjectile
                     */

                    foreach (string AttString in data.primaryAttack)
                    {
                        string[] mod = AttString.Split(':');
                        int modType = Enum.TryParse<NewDamageTypes>(mod[0], out NewDamageTypes result) ? (int)result : (int)Enum.Parse(typeof(HitData.DamageType), mod[0]);
                        PrimaryItemData.m_shared.m_damageModifiers.Add(new HitData.DamageModPair() { m_type = (HitData.DamageType)modType, m_modifier = (HitData.DamageModifier)Enum.Parse(typeof(HitData.DamageModifier), mod[1]) }); // end aedenthorn code
                    }

                    foreach (string SecAttString in data.secondaryAttack)
                    {
                        string[] mod = SecAttString.Split(':');
                        int modType = Enum.TryParse<NewDamageTypes>(mod[0], out NewDamageTypes result) ? (int)result : (int)Enum.Parse(typeof(HitData.DamageType), mod[0]);
                        PrimaryItemData.m_shared.m_damageModifiers.Add(new HitData.DamageModPair() { m_type = (HitData.DamageType)modType, m_modifier = (HitData.DamageModifier)Enum.Parse(typeof(HitData.DamageModifier), mod[1]) }); // end aedenthorn code
                    }


                    PrimaryItemData.m_shared.m_damageModifiers.Clear(); // from aedenthorn start -  thx
                    foreach (string modString in data.damageModifiers)
                    {
                        string[] mod = modString.Split(':');
                        int modType = Enum.TryParse<NewDamageTypes>(mod[0], out NewDamageTypes result) ? (int)result : (int)Enum.Parse(typeof(HitData.DamageType), mod[0]);
                        PrimaryItemData.m_shared.m_damageModifiers.Add(new HitData.DamageModPair() { m_type = (HitData.DamageType)modType, m_modifier = (HitData.DamageModifier)Enum.Parse(typeof(HitData.DamageModifier), mod[1]) }); // end aedenthorn code
                    }
                    if (PrimaryItemData.m_shared.m_value > 0)
                    {
                        string valu = "              <color=#edd221>Valuable</color>";
                        PrimaryItemData.m_shared.m_description = data.m_description + valu;
                    }
                    return; // done, I don't need to continue?
                } // Dbgl("Amost done with SetItemData!");
               
               
            }

        }


        #endregion
        #region GetObject

        private static CraftingStation GetCraftingStation(string name)
        {
            if (name == "" || name == null)
                return null;
                
            //Dbgl("Looking for crafting station " + name);

            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
            {
                if (recipe?.m_craftingStation?.m_name == name)
                {

                  //  Dbgl("got crafting station " + name);

                    return recipe.m_craftingStation;
                }
            }
            foreach (GameObject piece in GetPieces())
            {

                if (piece.GetComponent<Piece>()?.m_craftingStation?.m_name == name)
                {

                   // Dbgl("got crafting station " + name);

                    return piece.GetComponent<Piece>().m_craftingStation;

                }
            }

            return null;
        }
        private static List<GameObject> GetPieces()
        {
            var pieces = new List<GameObject>();
            if (!ObjectDB.instance)
                return pieces;

            ItemDrop hammer = ObjectDB.instance.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>();

            if (hammer)
                pieces.AddRange(Traverse.Create(hammer.m_itemData.m_shared.m_buildPieces).Field("m_pieces").GetValue<List<GameObject>>());

            ItemDrop hoe = ObjectDB.instance.GetItemPrefab("Hoe")?.GetComponent<ItemDrop>();
            if (hoe)
                pieces.AddRange(Traverse.Create(hoe.m_itemData.m_shared.m_buildPieces).Field("m_pieces").GetValue<List<GameObject>>());

            return pieces;

        }
        private static GameObject GetModdedPieces(string name)
        {
            selectedPiecehammer = null;
            GameObject Searchingfor = null;
            foreach (PieceTable Station in MaybePieceStations) // look for known modded hammers, Forget the indivual item, now just PieceTable
            {
                Searchingfor = Station.m_pieces.Find(g => Utils.GetPrefabName(g) == name);
                if (Searchingfor != null)
                {
                    selectedPiecehammer = Station;
                    return Searchingfor;
                }
            }
            return Searchingfor;
        }

        private static void GetPieceStations()
        {
            MaybePieceStations = Resources.FindObjectsOfTypeAll<PieceTable>(); // so easy
            /*
            GameObject temp;
            foreach (PieceTable p in MaybePieceStations)
            {
                //Dbgl($"PieceStation found: {p.name}, now filtering the names now");
                p.m_pieces.
                switch (p.name)
                {
                    case "_HoePieceTable": RealPieceStations.Add("_HoePieceTable"); break;
                    case "_HammerPieceTable": RealPieceStations.Add("_HammerPieceTable"); break; 
                    case "_CultivatorPieceTable": RealPieceStations.Add("_CultivatorPieceTable"); break;
                    case "_BJORKSNASPieceTable": RealPieceStations.Add("_BJORKSNASPieceTable"); break;
                   // case "JVLmock__RKCustomTable": break; // I don't think there is any reason to go through this, but just in case, it will be added by default
                    case "_RKCustomTable": RealPieceStations.Add("_RKCustomTable"); break;
                    case "_PlantitPieceTable": RealPieceStations.Add("_PlantitPieceTable"); break;
                    case "_ClutterPieceTable": RealPieceStations.Add("_ClutterPieceTable"); break;
                    // additional mods go here


                    default: RealPieceStations.Add(p.name); break;
                }
            } 
            */
            /*  It is close working, but out of order causes problems, needs a second pass. 
                      try
                        {z`
                            temp = ObjectDB.instance.GetItemPrefab(p.name);
                            if (temp != null)
                            {
                                Dbgl($"Found new Hammer that is real {p.name} ");
                                RealPieceStations.Add(p.name);
                            } // this will not catch unknown JVL hammers
                            else // this will probably catch unknown JVL hammers
                            {
                                if (Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn"))
                                {
                                    Dbgl($"JVL is loaded, going to try to catch those unknown hammer: {p.name}");
                                    object PieceManager = Chainloader.PluginInfos["com.jotunn.jotunn"].Instance.GetType().Assembly.GetType("Jotunn.Managers.PieceManager").GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                                    object tryd = AccessTools.Field(PieceManager.GetType(), "PieceTableNameMap").GetValue(PieceManager);
                                    //https://github.com/Valheim-Modding/Jotunn/blob/dev/JotunnLib/Managers/PieceManager.cs#L40 based on, and reflection
                                    if (tryd != null)
                                    {
                                        Dictionary<string, string> pauler = (Dictionary<string, string>)tryd;
                                        string temp2 = null;
                                        string temp1 = null;
                                        bool inarray = false;
                                        // Dbgl($"All PieceTableNameMap in JVL loaded:");
                                        foreach (KeyValuePair<string, string> entry in pauler) // run though Maybe pieces to make sure piece hasn't already been entered, looking for value
                                        {
                                            Dbgl($" key {entry.Key} value {entry.Value}"); // list all 
                                            temp2 = entry.Value;
                                            temp1 = entry.Key;
                                            inarray = false;
                                            foreach (string t in RealPieceStations)
                                            {
                                                //Dbgl($" compare {t}");
                                                if (t == (temp2)) // checking key and value
                                                    inarray = true;
                                                if (t == (temp1))
                                                    inarray = true;
                                            }
                                            if (!inarray) // not in RealPieceStations
                                            {
                                                Dbgl($"Value {entry.Value} and Key {entry.Key} not found in RealPieceStation, which is the real one? - going with value since it starts usually with _");
                                                RealPieceStations.Add(entry.Value);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                       }catch { Dbgl($"Error out on {p.name}"); break; }
                }
            } // end looking for hammers
            Dbgl($"end looking");
            */
        }


            private static RecipeData GetRecipeDataByName(string name)
        {
            GameObject go = ObjectDB.instance.GetItemPrefab(name);

            if (go == null)
            {
                Dbgl($"Recipe {name} not found!");
                return null; //GetPieceRecipeByName(name);
            }

            ItemDrop.ItemData item = go.GetComponent<ItemDrop>().m_itemData;
            if (item == null)
            {
                Dbgl("Item data not found!");
                return null;
            }
            Recipe recipe = ObjectDB.instance.GetRecipe(item);
            if (!recipe)
            {
                if (Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn"))
                {
                    object itemManager = Chainloader.PluginInfos["com.jotunn.jotunn"].Instance.GetType().Assembly.GetType("Jotunn.Managers.ItemManager").GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                    object cr = AccessTools.Method(itemManager.GetType(), "GetRecipe").Invoke(itemManager, new[] { item.m_shared.m_name });
                    if (cr != null)
                    {
                        recipe = (Recipe)AccessTools.Property(cr.GetType(), "Recipe").GetValue(cr);
                        Dbgl($"Jotunn recipe: {item.m_shared.m_name} {recipe != null}");
                    }
                }

                if (!recipe)
                {
                    Dbgl($"Recipe not found for item {item.m_shared.m_name}!");
                    return null;
                }
            }

            var data = new RecipeData()
            {
                name = name,
                amount = recipe.m_amount,
                craftingStation = recipe.m_craftingStation?.m_name ?? "",
                minStationLevel = recipe.m_minStationLevel, 
            };
            foreach (Piece.Requirement req in recipe.m_resources)
            {
                data.reqs.Add($"{Utils.GetPrefabName(req.m_resItem.gameObject)}:{req.m_amount}:{req.m_amountPerLevel}:{req.m_recover}");
            }

            return data;
        }

        private static PieceData GetPieceRecipeByName(string name, bool warn = true)
        {
            Piece piece = null;
            string piecehammer = "Hammer"; // default
            GameObject go = GetPieces().Find(g => Utils.GetPrefabName(g) == name); // vanilla search
            if (go == null)
            {
                go = GetModdedPieces(name);// known modded Hammer search
                if (go == null)
                {
                    go = ObjectDB.instance.GetItemPrefab(name); // last chance for modded hammers before JVL
                    if (go == null)
                    {
                        if (Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn"))
                        {
                            object PieceManager = Chainloader.PluginInfos["com.jotunn.jotunn"].Instance.GetType().Assembly.GetType("Jotunn.Managers.PieceManager").GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                            object cr = AccessTools.Method(PieceManager.GetType(), "GetPiece").Invoke(PieceManager, new[] { name });
                            if (cr != null)
                            {
                                piece = (Piece)AccessTools.Property(cr.GetType(), "Piece").GetValue(cr);
                                Dbgl($"Jotunn recipe: found for {name}");
                                go = piece.gameObject;
                                warn = true;
                                /*
                                Dbgl($"Clutter mod uses _ClutterPieceTable");
                                Dbgl($"Buildit mod uses _RKCustomTable");
                                Dbgl($"PlantIt mod uses _PlantitPieceTable");
                                */

                            }
                        }
                        if (go == null) // 4th layer
                        {
                            Dbgl($"Piece {name} not found! 4 layer search");
                            return null;
                        }
                    } else // 3rd layer
                    {
                        Dbgl($"Piece {name} from itself or unknown hammer - setting default to Hammer");
                        piece = go.GetComponent<Piece>();
                        warn = true;
                    }
                }else // 2nd layer
                {
                    Dbgl($"Piece {name} from known custom hammer {selectedPiecehammer}");
                    piece = go.GetComponent<Piece>();
                    piecehammer = selectedPiecehammer.name;
                    warn = false;
                }
            }else // 1st layer
            {
                piece = go.GetComponent<Piece>();
                warn = false;
            }   
            if (piece == null) // final check
            {
                Dbgl("Piece data not found!");
                return null;
            }

            // these are kind of reduntant. You could easily get rid of 100 lines of code here // just use GetModdedPieces // but unsure if it would cover every case
            ItemDrop hammer = ObjectDB.instance.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>();
            if (hammer && hammer.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(go))
                piecehammer = "Hammer";

            ItemDrop hoe = ObjectDB.instance.GetItemPrefab("Hoe")?.GetComponent<ItemDrop>();
            if (hoe && hoe.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(go))
                piecehammer = "Hoe";

            if (warn)
                WackysRecipeCustomizationLogger.LogWarning("If using a custom Hammer, make sure to set this in piecehammer otherwise it will default to Hammer");
            string wackyname ="";
            string wackydesc ="";
            wackydesc = piece.m_description;
            wackyname = piece.m_name;

            var data = new PieceData()
            {
                name = name,
                amount = 1,
                craftingStation = piece.m_craftingStation?.m_name ?? "",
                minStationLevel = 1,
                piecehammer = piecehammer,
                adminonly = false,
                m_name = wackyname,
                m_description = wackydesc,
            };
            foreach (Piece.Requirement req in piece.m_resources)
            {
                data.reqs.Add($"{Utils.GetPrefabName(req.m_resItem.gameObject)}:{req.m_amount}:{req.m_amountPerLevel}:{req.m_recover}");
            }

            return data;
        }

        private static WItemData GetItemDataByName(string name)
        {
            GameObject go = ObjectDB.instance.GetItemPrefab(name);
            if (go == null)
            {
                Dbgl("GetItemDataByName data not found!");
                return null;
            }

            ItemDrop.ItemData data = go.GetComponent<ItemDrop>().m_itemData;
            if (data == null)
            {
                Dbgl("Item GetItemDataByName not found! - componets");
                return null;
            }
            /*
            Recipe recipe = ObjectDB.instance.GetRecipe(item);
            if (!recipe)
            {
                if (Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn"))
                {
                    object itemManager = Chainloader.PluginInfos["com.jotunn.jotunn"].Instance.GetType().Assembly.GetType("Jotunn.Managers.ItemManager").GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                    object cr = AccessTools.Method(itemManager.GetType(), "GetRecipe").Invoke(itemManager, new[] { item.m_shared.m_name });
                    if (cr != null)
                    {
                        recipe = (Recipe)AccessTools.Property(cr.GetType(), "Recipe").GetValue(cr);
                        Dbgl($"Jotunn recipe: {item.m_shared.m_name} {recipe != null}");
                    }
                }
            */ // not sure about this stuff for Jotunn mangers


            WDamages damages = null;
            string damagestring ="";
           // Dbgl("Item "+ name + " data.m_shared.m_damages.mslash" + data.m_shared.m_damages.m_slash);
            if (data.m_shared.m_damages.m_blunt > 0f || data.m_shared.m_damages.m_chop > 0f || data.m_shared.m_damages.m_damage > 0f || data.m_shared.m_damages.m_fire > 0f || data.m_shared.m_damages.m_frost > 0f || data.m_shared.m_damages.m_lightning > 0f || data.m_shared.m_damages.m_pickaxe > 0f || data.m_shared.m_damages.m_pierce > 0f || data.m_shared.m_damages.m_poison > 0f || data.m_shared.m_damages.m_slash > 0f || data.m_shared.m_damages.m_spirit > 0f)
            {
                Dbgl("Item " + name + " damage on ");

                damages = new WDamages // not used
                {

                    m_blunt = data.m_shared.m_damages.m_blunt,
                    m_chop = data.m_shared.m_damages.m_chop,
                    m_damage = data.m_shared.m_damages.m_damage,
                    m_fire = data.m_shared.m_damages.m_fire,
                    m_frost = data.m_shared.m_damages.m_frost,
                    m_lightning = data.m_shared.m_damages.m_lightning,
                    m_pickaxe = data.m_shared.m_damages.m_pickaxe,
                    m_pierce = data.m_shared.m_damages.m_pierce,
                    m_poison = data.m_shared.m_damages.m_poison,
                    m_slash = data.m_shared.m_damages.m_slash,
                    m_spirit = data.m_shared.m_damages.m_spirit
                };
                 damagestring = $"m_blunt:{data.m_shared.m_damages.m_blunt},"
                + $"m_chop:{data.m_shared.m_damages.m_chop},"
                + $"m_damage:{data.m_shared.m_damages.m_damage},"
                + $"m_fire:{data.m_shared.m_damages.m_fire},"
                + $"m_frost:{data.m_shared.m_damages.m_frost},"
                + $"m_lightning:{data.m_shared.m_damages.m_lightning},"
                + $"m_pickaxe:{data.m_shared.m_damages.m_pickaxe},"
                + $"m_pierce:{data.m_shared.m_damages.m_pierce},"
                + $"m_poison:{data.m_shared.m_damages.m_poison},"
                + $"m_slash:{data.m_shared.m_damages.m_slash},"
                + $"m_spirit:{data.m_shared.m_damages.m_spirit},"

                ;
                damagestring = damagestring.Replace(",", ", " );
            }
            WDamages damagesPerLevel = null;
            string damgelvlstring = "";
            if (data.m_shared.m_damagesPerLevel.m_blunt > 0f || data.m_shared.m_damagesPerLevel.m_chop > 0f || data.m_shared.m_damagesPerLevel.m_damage > 0f || data.m_shared.m_damagesPerLevel.m_fire > 0f || data.m_shared.m_damagesPerLevel.m_frost > 0f || data.m_shared.m_damagesPerLevel.m_lightning > 0f || data.m_shared.m_damagesPerLevel.m_pickaxe > 0f || data.m_shared.m_damagesPerLevel.m_pierce > 0f || data.m_shared.m_damagesPerLevel.m_poison > 0f || data.m_shared.m_damagesPerLevel.m_slash > 0f || data.m_shared.m_damagesPerLevel.m_spirit > 0f)
            {
                damagesPerLevel = new WDamages // not used
                {
                    m_blunt = data.m_shared.m_damagesPerLevel.m_blunt,
                    m_chop = data.m_shared.m_damagesPerLevel.m_chop,
                    m_damage = data.m_shared.m_damagesPerLevel.m_damage,
                    m_fire = data.m_shared.m_damagesPerLevel.m_fire,
                    m_frost = data.m_shared.m_damagesPerLevel.m_frost,
                    m_lightning = data.m_shared.m_damagesPerLevel.m_lightning,
                    m_pickaxe = data.m_shared.m_damagesPerLevel.m_pickaxe,
                    m_pierce = data.m_shared.m_damagesPerLevel.m_pierce,
                    m_poison = data.m_shared.m_damagesPerLevel.m_poison,
                    m_slash = data.m_shared.m_damagesPerLevel.m_slash,
                    m_spirit = data.m_shared.m_damagesPerLevel.m_spirit
                };           
                 damgelvlstring = $"m_blunt:{data.m_shared.m_damagesPerLevel.m_blunt},"
                + $"m_chop:{data.m_shared.m_damagesPerLevel.m_chop},"
                + $"m_damage:{data.m_shared.m_damagesPerLevel.m_damage},"
                + $"m_fire:{data.m_shared.m_damagesPerLevel.m_fire},"
                + $"m_frost:{data.m_shared.m_damagesPerLevel.m_frost},"
                + $"m_lightning:{data.m_shared.m_damagesPerLevel.m_lightning},"
                + $"m_pickaxe:{data.m_shared.m_damagesPerLevel.m_pickaxe},"
                + $"m_pierce:{data.m_shared.m_damagesPerLevel.m_pierce},"
                + $"m_poison:{data.m_shared.m_damagesPerLevel.m_poison},"
                + $"m_slash:{data.m_shared.m_damagesPerLevel.m_slash},"
                + $"m_spirit:{data.m_shared.m_damagesPerLevel.m_spirit},"

                ;
                damgelvlstring = damgelvlstring.Replace(",", ", " );
            }
            /*
             * f
            foreach (Piece.Requirement req in piece.m_resources) // maybe use in future
            {
                data.reqs.Add($"{Utils.GetPrefabName(req.m_resItem.gameObject)}:{req.m_amount}:{req.m_amountPerLevel}:{req.m_recover}");
            }*/

            WItemData jItemData = new WItemData
            {

                name = name,
                m_armor = data.m_shared.m_armor,
                clone = false,
                m_armorPerLevel = data.m_shared.m_armorPerLevel,
                m_blockPower = data.m_shared.m_blockPower,
                m_blockPowerPerLevel = data.m_shared.m_blockPowerPerLevel,
                m_deflectionForce = data.m_shared.m_deflectionForce,
                m_deflectionForcePerLevel = data.m_shared.m_deflectionForcePerLevel,
                m_description = data.m_shared.m_description,
                m_durabilityDrain = data.m_shared.m_durabilityDrain,
                m_durabilityPerLevel = data.m_shared.m_durabilityPerLevel,
                m_backstabbonus = data.m_shared.m_backstabBonus,
                m_equipDuration = data.m_shared.m_equipDuration,
                m_foodHealth = data.m_shared.m_food,
                m_foodColor = ColorUtil.GetHexFromColor(data.m_shared.m_foodColor),
                m_foodBurnTime = data.m_shared.m_foodBurnTime,
                m_foodRegen = data.m_shared.m_foodRegen,
                m_foodStamina = data.m_shared.m_foodStamina,
                m_holdDurationMin = data.m_shared.m_holdDurationMin,
                m_holdStaminaDrain = data.m_shared.m_holdStaminaDrain,
                m_maxDurability = data.m_shared.m_maxDurability,
                m_maxQuality = data.m_shared.m_maxQuality,
                m_maxStackSize = data.m_shared.m_maxStackSize,
                m_toolTier = data.m_shared.m_toolTier,
                m_useDurability = data.m_shared.m_useDurability,
                m_useDurabilityDrain = data.m_shared.m_useDurabilityDrain,
                m_value = data.m_shared.m_value,
                m_weight = data.m_shared.m_weight,
                m_destroyBroken = data.m_shared.m_destroyBroken,
                m_dodgeable = data.m_shared.m_dodgeable,
                m_canBeReparied = data.m_shared.m_canBeReparied,
                m_damages = damagestring,
                m_damagesPerLevel = damgelvlstring,
                m_name = data.m_shared.m_name,
                m_questItem = data.m_shared.m_questItem,
                m_teleportable = data.m_shared.m_teleportable,
                m_timedBlockBonus = data.m_shared.m_timedBlockBonus,
                m_movementModifier = data.m_shared.m_movementModifier,
                m_attackStamina = data.m_shared.m_attack.m_attackStamina,
                m_knockback = data.m_shared.m_attackForce,
                damageModifiers = data.m_shared.m_damageModifiers.Select(m => m.m_type + ":" + m.m_modifier).ToList(),
                
            };
           // Dbgl("Item " + name + " damages " + damages.m_slash); // I think damages is being overwritten?
            if (jItemData.m_foodHealth == 0f && jItemData.m_foodRegen == 0f && jItemData.m_foodStamina == 0f)
            {
                jItemData.m_foodColor = null;
            }

            return jItemData;
            

        }
        #endregion
        #region Armor

        [HarmonyPatch(typeof(ItemDrop), "SlowUpdate")] //checks every once in a while
        static class ItemDrop_SlowUpdate_Patch
        {
            static void Postfix(ref ItemDrop __instance)
            {
                if (!modEnabled.Value)
                    return;
                CheckArmorData(ref __instance.m_itemData);
            }
        }
        [HarmonyPatch(typeof(SE_Stats), "GetDamageModifiersTooltipString")]
        static class GetDamageModifiersTooltipString_Patch
        {
            static void Postfix(ref string __result, List<HitData.DamageModPair> mods)
            {
                if (!modEnabled.Value)
                    return;

                __result = Regex.Replace(__result, @"\n.*<color=orange></color>", "");
                foreach (HitData.DamageModPair damageModPair in mods)
                {
                    if (Enum.IsDefined(typeof(HitData.DamageType), damageModPair.m_type))
                        continue;

                    if (damageModPair.m_modifier != HitData.DamageModifier.Ignore && damageModPair.m_modifier != HitData.DamageModifier.Normal)
                    {
                        switch (damageModPair.m_modifier)
                        {
                            case HitData.DamageModifier.Resistant:
                                __result += "\n$inventory_dmgmod: <color=orange>$inventory_resistant</color> VS ";
                                break;
                            case HitData.DamageModifier.Weak:
                                __result += "\n$inventory_dmgmod: <color=orange>$inventory_weak</color> VS ";
                                break;
                            case HitData.DamageModifier.Immune:
                                __result += "\n$inventory_dmgmod: <color=orange>$inventory_immune</color> VS ";
                                break;
                            case HitData.DamageModifier.VeryResistant:
                                __result += "\n$inventory_dmgmod: <color=orange>$inventory_veryresistant</color> VS ";
                                break;
                            case HitData.DamageModifier.VeryWeak:
                                __result += "\n$inventory_dmgmod: <color=orange>$inventory_veryweak</color> VS ";
                                break;
                        }
                        if ((int)damageModPair.m_type == (int)NewDamageTypes.Water)
                        {
                            __result += "<color=orange>" + waterModifierName.Value + "</color>";
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Player), "UpdateEnvStatusEffects")]
        static class UpdateEnvStatusEffects_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // I really wish I knew what this transpiler code was doing for the frost and immune. Going to trust she knows what she's doing
            {
                Dbgl($"Transpiling UpdateEnvStatusEffects");

                var codes = new List<CodeInstruction>(instructions);
                var outCodes = new List<CodeInstruction>();
                bool notFound = true;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (notFound && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldc_I4_1 && codes[i + 2].opcode == OpCodes.Beq && codes[i + 3].opcode == OpCodes.Ldloc_S && codes[i + 3].operand == codes[i].operand && codes[i + 4].opcode == OpCodes.Ldc_I4_5)
                    {
                        Dbgl($"Adding frost immune and ignore");

                        outCodes.Add(new CodeInstruction(codes[i]));
                        outCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_3));
                        outCodes.Add(new CodeInstruction(codes[i + 2]));
                        outCodes.Add(new CodeInstruction(codes[i]));
                        outCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_4));
                        outCodes.Add(new CodeInstruction(codes[i + 2]));
                        notFound = false;
                    }
                    outCodes.Add(codes[i]);
                }

                return outCodes.AsEnumerable();
            }
            //skipping wet patches
        }
        private static bool ShouldOverride(HitData.DamageModifier a, HitData.DamageModifier b)
        {
            return a != HitData.DamageModifier.Ignore && (b == HitData.DamageModifier.Immune || ((a != HitData.DamageModifier.VeryResistant || b != HitData.DamageModifier.Resistant) && (a != HitData.DamageModifier.VeryWeak || b != HitData.DamageModifier.Weak)));
        }
        
        private static void LoadAllArmorData(ZNetScene scene)
        {
            foreach (var armor in armorDatas)
            {
                GameObject go = scene.GetPrefab(armor.name);
                if (go == null)
                    continue;
                ItemDrop.ItemData item = go.GetComponent<ItemDrop>().m_itemData;
                SetArmorData(ref item, armor);
                go.GetComponent<ItemDrop>().m_itemData = item;
            }
        }

        private static void CheckArmorData(ref ItemDrop.ItemData instance)
        {
            try
            {
                var name = instance.m_dropPrefab.name;
                var armor = armorDatas.First(d => d.name == name);
                SetArmorData(ref instance, armor);
                //Dbgl($"Set armor data for {instance.name}");
            }
            catch
            {

            }
        }

        private static void SetArmorData(ref ItemDrop.ItemData item, ArmorData armor)
        {
            //item.m_shared.m_armor = armor.armor;
            //item.m_shared.m_armorPerLevel = armor.armorPerLevel;
           // item.m_shared.m_movementModifier = armor.movementModifier;

            item.m_shared.m_damageModifiers.Clear();
            foreach (string modString in armor.damageModifiers)
            {
                string[] mod = modString.Split(':');
                int modType = Enum.TryParse<NewDamageTypes>(mod[0], out NewDamageTypes result) ? (int)result : (int)Enum.Parse(typeof(HitData.DamageType), mod[0]);
                item.m_shared.m_damageModifiers.Add(new HitData.DamageModPair() { m_type = (HitData.DamageType)modType, m_modifier = (HitData.DamageModifier)Enum.Parse(typeof(HitData.DamageModifier), mod[1]) });
            }
        }

        private static HitData.DamageModifier GetNewDamageTypeMod(NewDamageTypes type, Character character)
        {
            Traverse t = Traverse.Create(character);
            return GetNewDamageTypeMod(type, t.Field("m_chestItem").GetValue<ItemDrop.ItemData>(), t.Field("m_legItem").GetValue<ItemDrop.ItemData>(), t.Field("m_helmetItem").GetValue<ItemDrop.ItemData>(), t.Field("m_shoulderItem").GetValue<ItemDrop.ItemData>());
        }

        private static HitData.DamageModifier GetNewDamageTypeMod(NewDamageTypes type, ItemDrop.ItemData chestItem, ItemDrop.ItemData legItem, ItemDrop.ItemData helmetItem, ItemDrop.ItemData shoulderItem)
        {
            HitData.DamageModPair modPair = new HitData.DamageModPair();

            if (chestItem != null)
                modPair = chestItem.m_shared.m_damageModifiers.FirstOrDefault(s => (int)s.m_type == (int)type);

            if (legItem != null)
            {
                var leg = legItem.m_shared.m_damageModifiers.FirstOrDefault(s => (int)s.m_type == (int)type);
                if (ShouldOverride(modPair.m_modifier, leg.m_modifier))
                    modPair = leg;
            }
            if (helmetItem != null)
            {
                var helm = helmetItem.m_shared.m_damageModifiers.FirstOrDefault(s => (int)s.m_type == (int)type);
                if (ShouldOverride(modPair.m_modifier, helm.m_modifier))
                    modPair = helm;
            }
            if (shoulderItem != null)
            {
                var shoulder = shoulderItem.m_shared.m_damageModifiers.FirstOrDefault(s => (int)s.m_type == (int)type);
                if (ShouldOverride(modPair.m_modifier, shoulder.m_modifier))
                    modPair = shoulder;
            }
            return modPair.m_modifier;
        }


        #endregion
        #region Patches


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
        public static class Console_Patch
        // need to come up with a syntax now
        /* WackyEditor
         * Wackydbeditor
         * wackyDB
         * wackydb save/reload/dump
         * wackydbsave
         * wackydbitemsave
         * wackydb save item <item>
         * wackydb save <recipe/piece>
         * wackydb_save  <item>
         * wackydb_reload
         * wackydb_dump
         * wackydb_reset
         * wackydb <listcommands>
         * wackydb_save_item <item>
         * 
         */
        {
            private static void Postfix()
            {
                WackysRecipeCustomizationLogger.LogDebug("Patching Updated Console Commands");

                if (!modEnabled.Value)
                    return;
                if (SceneManager.GetActiveScene().name != "main") return; // can't do anything from main

                Terminal.ConsoleCommand WackyShowcommands =
                    new("wackydb", "Display Help ",
                        args =>
                        {
                            string output = $"wackydb_reload\r\n"
                            // + $"wackydb_reset \r\n"
                            + $"wackydb_dump (2)<item/recipe/piece> <ItemName>\r\n"
                            + $"wackydb_dump_all (dumps all info already loaded - may not work with clones very well)\r\n"
                            + $"wackydb_save_recipe (1) <ItemName>(recipe output)\r\n"
                            + $"wackydb_save_piece (1) <ItemName>(piece output) (piecehammer only works for clones)\r\n"
                            + $"wackydb_save_item (1)<ItemName>(item Output)\r\n"
                            + $"wackydb_help\r\n"
                            + $"wackydb_clone (3) <(recipe,item,piece)> <Prefab to clone> <Unique name for the clone> \r\n"
                            + $"4th option example - you can already have item WackySword loaded in game, but now want a recipe. WackySword Uses SwordIron  - wackydb_clone recipe WackySword RWackySword SwordIron - otherwise manually edit\r\n"
                            + $"wackydb_clone_recipeitem (2)(clones item and recipe at same time) <Prefab to clone> <Unique name for the clone, Recipe name will be Rname>\r\n"
                            + $"wackydb_vfx (outputs future Vfx gameobjects available)\r\n"
                            + $"wackydb_material (outputs Materials available)\r\n"

                            ;

                            args.Context?.AddString(output);
                        });

                Terminal.ConsoleCommand WackyShowcommandshelp =
                     new("wackydb_help", "Display Help ",
                         args =>
                         {
                             string output = $"wackydb_reload\r\n"
                             // + $"wackydb_reset \r\n"
                            + $"wackydb_dump (2)<item/recipe/piece> <ItemName>\r\n"
                            + $"wackydb_dump_all (dumps all info already loaded - may not work with clones very well)\r\n"
                            + $"wackydb_save_recipe (1) <ItemName>(recipe output)\r\n"
                            + $"wackydb_save_piece (1) <ItemName>(piece output) (piecehammer only works for clones)\r\n"
                            + $"wackydb_save_item (1)<ItemName>(item Output)\r\n"
                            + $"wackydb_help\r\n"
                            + $"wackydb_clone (3) <(recipe,item,piece)> <Prefab to clone> <Unique name for the clone> \r\n"
                            + $"4th option example - you can already have item WackySword loaded in game, but now want a recipe. WackySword Uses SwordIron  - wackydb_clone recipe WackySword RWackySword SwordIron - otherwise manually edit\r\n"
                            + $"wackydb_clone_recipeitem (2)(clones item and recipe at same time) <Prefab to clone> <Unique name for the clone, Recipe name will be Rname>\r\n"
                            + $"wackydb_vfx (outputs future Vfx gameobjects available)\r\n"
                            + $"wackydb_material (outputs Materials available)\r\n"

                          ;

                             args.Context?.AddString(output);
                         });
                /*
                Terminal.ConsoleCommand WackyReset =
                     new("wackydb_reset", "reload the whole config files", // I should probably delete this one?
                         args =>
                         {
                             context.Config.Reload();
                             context.Config.Save();
                             args.Context?.AddString("Configs reloaded");
                         });
                */
                Terminal.ConsoleCommand WackyReload =
                     new("wackydb_reload", "reload the whole config files",
                         args =>
                         {
                             // GetRecipeDataFromFiles(); called in loadallrecipes
                             if (ObjectDB.instance && issettoSinglePlayer)
                             {

                                 LoadAllRecipeData(true);
                                 args.Context?.AddString($"WackyDatabase reloaded recipes/items/pieces from files");
                                 Dbgl("WackyDatabase reloaded recipes/items/pieces from files");
                             }
                             else
                             {
                                 args.Context?.AddString($"WackyDatabase did NOT reload recipes/items/pieces from files"); // maybe?
                                 Dbgl("WackyDatabase did NOT reload recipes/items/pieces from files");
                             }

                         });

                Terminal.ConsoleCommand WackyDump =
                     new("wackydb_dump", "dump the item or recipe into the logs",
                         args =>
                         {
                             if (args.Length - 1 < 2)
                             {
                                 args.Context?.AddString("Not enough arguments");

                             }
                             else
                             {
                                 string recipe = args[1];
                                 string comtype = args[2];
                                 if (recipe == "item" || recipe == "Item")
                                 {
                                     WItemData recipeData = GetItemDataByName(comtype);
                                     if (recipeData == null)
                                         return;
                                     Dbgl(JsonUtility.ToJson(recipeData));

                                 } else if (recipe == "piece" || recipe == "Piece")
                                 {
                                     PieceData data = GetPieceRecipeByName(comtype);
                                     if (data == null)
                                         return;
                                     Dbgl(JsonUtility.ToJson(data));
                                 }
                                 else
                                 {
                                     RecipeData recipeData = GetRecipeDataByName(comtype);
                                     if (recipeData == null)
                                         return;
                                     Dbgl(JsonUtility.ToJson(recipeData));
                                 }
                                 args.Context?.AddString($"WackyDatabase dumped {comtype}");
                             }
                         });

                Terminal.ConsoleCommand WackyDumpAll =
                     new("wackydb_dump_all", "dump all",
                         args =>
                         {
                             string TheStringMaster = "";
                             string temp = "";
                             if (issettoSinglePlayer)
                             {
                                 foreach (var data in ItemDatas)
                                 {
                                     if (data != null)
                                     {
                                         WItemData output1 = GetItemDataByName(data.name);
                                         if (output1 == null)
                                             continue;
                                         output1.clone = data.clone;
                                         output1.cloneMaterial = data?.cloneMaterial;
                                         output1.clonePrefabName = data?.clonePrefabName;
                                         temp = JsonUtility.ToJson(output1);
                                         TheStringMaster = TheStringMaster + temp + System.Environment.NewLine;
                                         Dbgl(temp);
                                     }
                                 }
                                 foreach (var data2 in PieceDatas)
                                 {
                                     if (data2 != null)
                                     {
                                         PieceData output2 = GetPieceRecipeByName(data2.name, false);
                                         if (output2 == null)
                                             continue;
                                         output2.clone = data2.clone;
                                         output2.cloneMaterial = data2.cloneMaterial;
                                         output2.clonePrefabName = data2?.clonePrefabName;
                                         output2.piecehammer = data2.piecehammer;
                                         temp = JsonUtility.ToJson(output2);
                                         TheStringMaster = TheStringMaster + temp + System.Environment.NewLine;
                                         Dbgl(temp);
                                     }
                                 }
                                 foreach (var data3 in recipeDatas)
                                 {
                                     if (data3 != null)
                                     {
                                         RecipeData output3 = GetRecipeDataByName(data3.name);
                                         if (output3 == null)
                                             continue;
                                         output3.clone = data3.clone;
                                         //output3.cloneColor = data3.cloneColor;
                                         output3.clonePrefabName = data3.clonePrefabName;
                                         temp = JsonUtility.ToJson(output3);
                                         TheStringMaster = TheStringMaster + temp + System.Environment.NewLine;
                                         Dbgl(temp);
                                     }
                                 }
                                 File.WriteAllText(Path.Combine(assetPath, "DumpAll.txt"), TheStringMaster);
                                 args.Context?.AddString($"WackyDatabase dumped all, created file DumpAll.txt");
                             } else
                             {
                                 args.Context?.AddString($"In Multiplayer, so no all dump");
                             }

                         });

                Terminal.ConsoleCommand WackyitemSave =
                    new("wackydb_save_item", "Save an Item ",
                        args =>
                        {
                            string file = args[1];
                            WItemData recipData = GetItemDataByName(file);
                            if (recipData == null)
                                return;
                            CheckModFolder();
                            File.WriteAllText(Path.Combine(assetPathItems, "Item_" + recipData.name + ".json"), JsonUtility.ToJson(recipData, true));
                            args.Context?.AddString($"saved item data to Item_{file}.json");

                        });
                Terminal.ConsoleCommand WackyPieceSave =
                    new("wackydb_save_piece", "Save a piece ",
                        args =>
                        {
                            string file = args[1];
                            PieceData recipData = GetPieceRecipeByName(file);
                            if (recipData == null)
                                return;
                            CheckModFolder();
                            File.WriteAllText(Path.Combine(assetPathPieces, "Piece_" + recipData.name + ".json"), JsonUtility.ToJson(recipData, true));
                            args.Context?.AddString($"saved data to Piece_{file}.json");

                        });
                Terminal.ConsoleCommand WackyRecipeSave =
    new("wackydb_save_recipe", "Save a recipe ",
        args =>
        {
            string file = args[1];
            RecipeData recipData = GetRecipeDataByName(file);
            if (recipData == null)
                return;
            CheckModFolder();
            File.WriteAllText(Path.Combine(assetPathRecipes, "Recipe_" + recipData.name + ".json"), JsonUtility.ToJson(recipData, true));
            args.Context?.AddString($"saved data to Recipe_{file}.json");

        });

                Terminal.ConsoleCommand WackyMaterials =
    new("wackydb_material", "Create txt file of materials",
        args =>
        {
            string theString = GetAllMaterialsFile();
            CheckModFolder();
            File.WriteAllText(Path.Combine(assetPath, "Materials.txt"), theString);
            args.Context?.AddString($"saved data to Materials.txt");

        });

                Terminal.ConsoleCommand Wackyvfx =
    new("wackydb_vfx", "Create txt file of VFX",
        args =>
        {
            string theString2 = GetAllVFXFile();
            CheckModFolder();
            File.WriteAllText(Path.Combine(assetPath, "vfx.txt"), theString2);
            args.Context?.AddString($"saved data to VFX.txt");

        });

                /* syntax for cloning
                 * wackydb_clone <item/recipe/piece> <prefab to clone> <nameofclone>(has to be unquie otherwise we would have to check) 
                 * 
                 */
                Terminal.ConsoleCommand WackyClone =
                    new("wackydb_clone", "Clone an item or piecce with different status, names, effects ect... ",
                        args =>
                        {
                            if (args.Length - 1 < 3)
                            {
                                args.Context?.AddString("<color=lime>Not enough arguments</color>");

                            }
                            else
                            {
                                string commandtype = args[1];
                                string prefab = args[2];
                                string newname = args[3];
                                string file = args[3];
                                if (newname == "SwordTest")
                                {
                                    args.Context?.AddString($"<color=red>{newname} is already a ingame name. -Bad </color>");
                                    return;
                                }

                                if (commandtype == "recipe" || commandtype == "Recipe")
                                {
                                    CheckModFolder();
                                    if (args.Length - 1 < 4)
                                    {
                                        RecipeData clone = GetRecipeDataByName(prefab);// actually it could be a different prefab if cloned item
                                        if (clone == null)
                                            return;
                                        clone.name = newname;
                                        clone.clone = true;
                                        clone.clonePrefabName = prefab;
                                        File.WriteAllText(Path.Combine(assetPathRecipes, "Recipe_" + clone.name + ".json"), JsonUtility.ToJson(clone, true));
                                        file = "Recipe" + clone.name;
                                    } else
                                    {
                                        string prefabitem = args[4];
                                        RecipeData clone = GetRecipeDataByName(prefabitem);//  prefab of cloned item
                                        if (clone == null)
                                            return;
                                        clone.name = newname;
                                        clone.clone = true;
                                        clone.clonePrefabName = prefab; // cloned item
                                        File.WriteAllText(Path.Combine(assetPathRecipes, "Recipe_" + clone.name + ".json"), JsonUtility.ToJson(clone, true));
                                        file = "Cloned Item " + clone.name + " Clone Recipe from " + prefabitem;

                                    } // added optional arugment for cloned items


                                }
                                if (commandtype == "item" || commandtype == "Item")
                                {
                                    WItemData clone = GetItemDataByName(prefab);
                                    if (clone == null)
                                        return;
                                    clone.name = newname;
                                    clone.clone = true;
                                    clone.clonePrefabName = prefab;
                                    clone.m_name = newname;


                                    if (clone == null)
                                        return;
                                    CheckModFolder();
                                    File.WriteAllText(Path.Combine(assetPathItems, "Item_" + clone.name + ".json"), JsonUtility.ToJson(clone, true));
                                    file = "Item_" + clone.name;



                                }
                                if (commandtype == "piece" || commandtype == "Piece")
                                {
                                    PieceData clone = GetPieceRecipeByName(prefab);
                                    if (clone == null)
                                        return;
                                    clone.name = newname;
                                    clone.clone = true;
                                    clone.clonePrefabName = prefab;



                                    if (clone == null)
                                        return;
                                    CheckModFolder();
                                    File.WriteAllText(Path.Combine(assetPathPieces, "Piece_" + clone.name + ".json"), JsonUtility.ToJson(clone, true));
                                    file = "Piece_" + clone.name;

                                }
                                args.Context?.AddString($"saved cloned data to {file}.json");
                            }
                        });
                Terminal.ConsoleCommand WackyCloneRecipe =
                    new("wackydb_clone_recipeitem", "Clone recipe and item with the orginal prefab ",
                        args => {

                            CheckModFolder();
                            if (args.Length - 1 < 2)
                            {
                                args.Context?.AddString("<color=lime>Not enough arguments</color>");
                            }
                            else
                            {
                                string prefab = args[1];
                                string newname = args[2];
                                string file = args[2];
                                WItemData itemclone = GetItemDataByName(prefab);
                                if (itemclone == null)
                                    return;
                                itemclone.name = newname;
                                itemclone.clone = true;
                                itemclone.clonePrefabName = prefab;
                                itemclone.m_name = newname;
                                File.WriteAllText(Path.Combine(assetPathItems, "Item_" + itemclone.name + ".json"), JsonUtility.ToJson(itemclone, true));

                                RecipeData clone = GetRecipeDataByName(prefab);//  prefab of cloned item
                                if (clone == null)
                                    return;
                                clone.name = "R"+newname;
                                clone.clone = true;
                                clone.clonePrefabName = itemclone.name; // cloned item
                                File.WriteAllText(Path.Combine(assetPathRecipes, "Recipe_" + clone.name + ".json"), JsonUtility.ToJson(clone, true));

                                file = "Cloned Item saved as Item_" + itemclone.name + ".json, cloned Recipe saved as Recipe_"+ clone.name +".json which is from the Orginal Recipe " + prefab;
                                args.Context?.AddString($"{file}");
                            } 
                            
                        });


                Terminal.ConsoleCommand Wackyadmin =    //dont look :)
                     new("customizationGuessing", "Gives you reload powers if you can guess the password",  // just for fun. Doesn't really give you admin powers
                   args =>
                   {
                       if (!issettoSinglePlayer && kickcount <3)// backdoor for funizes only availble when on multiplayer mode.. hahaaa
                       {
                           string passguess = "";
                            try
                           {
                                passguess = args[1];
                           }
                           catch
                           {
                               WackysRecipeCustomizationLogger.LogWarning("Congrats on finding the backdoor... You have 3 chances to guess the password or you will be called out that your a dirty cheater in chat and probably being kicked by Azu or an admin");
                               return;
                           }

                            WackysRecipeCustomizationLogger.LogWarning($"guess {kickcount + 1}");

                            string file = passguess;
                            string hash = ComputeSha256Hash(file);
                            string secrethash = "f289b4717485d90d9dee6ce2a9992e4fcfa4317a9439c148053d52c637b0691b"; // real hash is entered
                            if (hash == secrethash)
                            {
                                WackysRecipeCustomizationLogger.LogWarning("Congrats you cheater,  Enjoy nothin");

                            }
                            else
                            {
                                kickcount++;
                                if (kickcount >= 3)
                                {
                                   //List<string> stringList = ZNet.instance.GetPlayerList().Select(player => player.m_name).ToList();
                                  string name = Player.m_localPlayer.name; // someday make this so it shouts to other players
                                   Chat.m_instance.AddString("[WackysDatabase]",
                                $"<color=\"red\">Cheater Cheater, pants on fire. {name} tried to get admin access and failed. Laugh at this person or kick them.</color>",
                                     Talker.Type.Normal);
                                   WackysRecipeCustomizationLogger.LogWarning("Cheater Cheater, pants on fire");
                                }

                            }
                           
                       }

                   });



            }
        }

        [HarmonyPatch(typeof(Player), "PlacePiece")]
        private static class Player_MessageforPortal_Patch
        {
            [HarmonyPrefix]
            private static bool Prefix(ref Player __instance, ref Piece piece)

            {
                if (piece == null) return true;
                foreach (var item in pieceWithLvl)
                {
                    var stringwithnumber = item.Split('.');
                    var PiecetoLookFor = stringwithnumber[0];
                    int CraftingStationlvl = int.Parse(stringwithnumber[1]);

                    if (piece.name == PiecetoLookFor && !__instance.m_noPlacementCost) // portal
                    {
                        if (__instance.transform.position != null)
                            tempvalue = __instance.transform.position; // save position //must be assigned
                        else
                            tempvalue = new Vector3(0, 0, 0); // shouldn't ever be called 

                        var paulstation = CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, tempvalue);
                        var paullvl = paulstation.GetLevel();

                        if (paullvl + 1 > CraftingStationlvl) // just for testing
                        {
                            // piecehaslvl = true;
                        }
                        else
                        {
                            string worktablename = piece.m_craftingStation.name;
                            GameObject temp = GetPieces().Find(g => Utils.GetPrefabName(g) == worktablename);
                            var name = temp.GetComponent<Piece>().m_name;
                            __instance.Message(MessageHud.MessageType.Center, "Need a Level " + CraftingStationlvl + " " + name + " for placement");
                            //var josh = skillConfigData.Value;
                            // WackysRecipeCustomizationLogger.LogDebug("Synced String  " + josh);

                            //piecehaslvl = false;
                            return false;
                        }
                    }
                }
                return true;
            }

        }
        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        [HarmonyPriority(Priority.Last)]
        static class ZNetScene_Awake_Patch
        {
            static void Postfix()
            {
                if (!modEnabled.Value)
                    return;
                context.StartCoroutine(DelayedLoadRecipes());// very importrant for last sec load
                //LoadAllRecipeData(true);
            }
        }
        public static IEnumerator DelayedLoadRecipes()
        {
            yield return new WaitForSeconds(0.1f);
            LoadAllRecipeData(true);
            yield break;
        }

        [HarmonyPatch(typeof(ZNet), "Shutdown")]
        private class PatchZNetDisconnect
        {
            private static bool Prefix()
            {
                WackysRecipeCustomizationLogger.LogWarning("Logoff? So reset - character will look empty if using clone gear"  );
                Cloned.Clear();
                NoMoreLoading = true;
                return true;
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnDestroy")]
        private class PatchZNetDestory
        {
            private static void Postfix()
            { // The Server send once last config sync before destory, but after Shutdown which messes stuff up. 
                recieveServerInfo = false;
                NoMoreLoading = false;
            }
        }


        #endregion
        #region others

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }

        }
        public static float stringtoFloat(string data)
        {
            data = data.Split(':').Last();
            float value = float.Parse(data, CultureInfo.InvariantCulture.NumberFormat);
            return value;
        }
        private static Dictionary<string, Material> originalMaterials;
        private static Dictionary<string, GameObject> originalVFX;

        public static void GetAllMaterials()
        {
            Material[] array = Resources.FindObjectsOfTypeAll<Material>();
            originalMaterials = new Dictionary<string, Material>();
            Material[] array2 = array;
            foreach (Material val in array2)
            {
               // Dbgl($"Material {val.name}" );
                originalMaterials[val.name] = val;
            }
        }
        public static string GetAllMaterialsFile()
        {
            string TheString = "";
            Material[] array = Resources.FindObjectsOfTypeAll<Material>();
            Material[] array2 = array;
            foreach (Material val in array2)
            {
                 Dbgl($"Material {val.name}" );
                TheString = TheString + val.name + System.Environment.NewLine;
            }
            return TheString;
        }
        
        public static string GetAllVFXFile()
        {
            
            string TheString = "";

            GameObject[] array4 = Resources.FindObjectsOfTypeAll<GameObject>();
            originalVFX = new Dictionary<string, GameObject>();
            foreach (GameObject val2 in array4)
            {
                if (val2.name.Contains("vfx"))
                {
                    Dbgl($"VFX {val2.name}");
                    TheString = TheString + val2.name + System.Environment.NewLine;
                }
            }
            return TheString;
        }
        

        #endregion

    }
}
