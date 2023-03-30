﻿using HarmonyLib;
using ItemManager;
using ServerSync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using wackydatabase.SetData;
using wackydatabase.Datas;
using wackydatabase.Read;
using System.Security.Policy;
using wackydatabase.SetData.SetOldData;

namespace wackydatabase.Startup
{

    internal class Startup 
    {

        [HarmonyPatch(typeof(FejdStartup), "SetupObjectDB")]
        static class FejdStartupObjectDBSetup
        {
            static void Postfix()
            {
                WMRecipeCust.Dbgl("Loading Cloned items for Menu");
                ReadFiles clones = new ReadFiles();
                clones.GetCacheClonesOnly();          
                SetData.Reload Startup = new SetData.Reload();
                Startup.LoadClonedItems();
                WMRecipeCust.FirstSessionRun = true;
            }

        }

        [HarmonyPatch(typeof(FejdStartup), "OnCharacterStart")]
        static class FejdEndClone
        {
            static void Prefix()
            {
                DestroyStartupItems(); // destory old item clones
                WMRecipeCust.Dbgl("Unloading Cloned Items from MainMenu");
            }

        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        [HarmonyPriority(Priority.Last)]
        static class ZNetScene_Awake_Patch_WackysDatabase
        {
            static void Postfix()
            {
                if (!WMRecipeCust.modEnabled.Value)
                    return;

                
                WMRecipeCust.context.StartCoroutine(DelayedLoadRecipes());// very importrant for last sec load

                if (!ZNet.instance.IsServer() && WMRecipeCust.HasLobbied) // is client now
                {
                    WMRecipeCust.ForceLogout = true;
                    // Has Lobbied in Past and could try to use this to get around lockout. 
                    // issettoSinglePlayer = true;
                    if (WMRecipeCust.extraSecurity.Value)
                    {
                        WMRecipeCust.ConfigSync.CurrentVersion = "0.0.1"; // kicking player from server
                        WMRecipeCust.WLog.LogWarning("You hosted a COOP game before trying to connect to a server - LOCKOUT - 0.0.1 - Restart Game " + WMRecipeCust.ConfigSync.CurrentVersion);
                    }
                }
                //LoadAllRecipeData(true);
            }
        }

        [HarmonyPatch(typeof(ZSteamMatchmaking), "RegisterServer")]
        private class COOPCheckSteam
        {
            private static void Postfix()
            {
                WMRecipeCust.WLog.LogWarning("Steam Lobby is active");
                WMRecipeCust.LobbyRegistered = true;
            }

        }

        [HarmonyPatch(typeof(ZPlayFabMatchmaking), "RegisterServer")]
        private class COOPCheckPlayfab
        {
            private static void Postfix()
            {
                WMRecipeCust.WLog.LogWarning("Zplay Lobby is active");
                WMRecipeCust.LobbyRegistered = true;
            }

        }
        
        public static void DestroyStartupItems()
        {
            var delObj = ObjectDB.instance;
            foreach (var citem in WMRecipeCust.ClonedI)
            {
                try
                {
                    var go = DataHelpers.CheckforSpecialObjects(citem);// check for special cases
                    if (go == null)
                        go = delObj.GetItemPrefab(citem); // normal check
                    delObj.m_items.Remove(go);
                    GameObject.Destroy(go);
                }
                catch { WMRecipeCust.Dbgl($"Error Destorying item {citem}"); }

            }
            WMRecipeCust.Dbgl($"Destroyed cloned items used in startup ");
            WMRecipeCust.ClonedI.Clear();
        }

        public static bool SinglePlayerchecker
        {
            get { return WMRecipeCust.issettoSinglePlayer; }
            set
            {
                WMRecipeCust.issettoSinglePlayer = true;
                return;
            }
        }
        public static bool IsLocalInstance(ZNet znet)
        {
            if (znet.IsServer() && !znet.IsDedicated() && !WMRecipeCust.LobbyRegistered)
            {
                WMRecipeCust.issettoSinglePlayer = true;
                WMRecipeCust.ForceLogout = true;
                if (WMRecipeCust.extraSecurity.Value)
                {
                    WMRecipeCust.ConfigSync.CurrentVersion = "0.0.1"; // kicking player from server
                    WMRecipeCust.WLog.LogWarning("You Will be kicked from Multiplayer Servers! " + WMRecipeCust.ConfigSync.CurrentVersion);
                }
            }

            if (WMRecipeCust.LobbyRegistered)
            {
                WMRecipeCust.ConfigSync.CurrentVersion = WMRecipeCust.ModVersion; // Just in case goes from singleplayer to hosting. 
                WMRecipeCust.HasLobbied = true;

            }


            return WMRecipeCust.issettoSinglePlayer;
        }

        public static IEnumerator CleartoReload()
        {
            if (!WMRecipeCust.ReloadingOkay)
            {
                WMRecipeCust.WLog.LogInfo("Waiting...");
                yield return new WaitForSeconds(0.2f);
            }
            WMRecipeCust.WLog.LogInfo("Load Sync Data");
            SetData.Reload temp = new SetData.Reload();
            WMRecipeCust.CurrentReload = temp;

            if (WMRecipeCust.FirstSessionRun)
            {
                WMRecipeCust.context.StartCoroutine(temp.LoadAllRecipeData(true)); // Dedicated Sync Reload
                WMRecipeCust.FirstSessionRun = false; // reset in a destory patch
            }else
            {
                WMRecipeCust.context.StartCoroutine(temp.LoadAllRecipeData(true, true)); // Dedicated Sync Reload SLOW
            }
                

        }

        public static IEnumerator DelayedLoadRecipes()
        {
            yield return new WaitForSeconds(0.1f);

            WMRecipeCust.ReloadingOkay = true;

            SetData.Reload temp = new SetData.Reload();
            WMRecipeCust.CurrentReload = temp;
            //ReadFiles readnow = new ReadFiles(); // should already be read
            //readnow.GetDataFromFiles(); Don't need to reload files on first run, only on reload otherwise might override skillConfigData.Value
            OldReloadSet oldset = new OldReloadSet();


            if (WMRecipeCust.jsonsFound) 
            {
                WMRecipeCust.WLog.LogWarning("Jsons Found, loading jsons for conversion");

                oldset.OldGetJsons();

                WMRecipeCust.WLog.LogWarning("Jsons Loading into Database, Please stand by");
                oldset.OldReload();

                WMRecipeCust.WLog.LogWarning("Jsons being converted, Please stand by");

                WMRecipeCust.startupserver.SaveYMLBasedONJsons(WMRecipeCust.jsonfiles);

                WMRecipeCust.WLog.LogWarning("Jsons found have been moved to wackysDatabase-OldJsons, any left over should be recreated using console commands");

                WMRecipeCust.WLog.LogError("You should Now Exit, please remove any jsons leftover from wackydatabase");


                //Application.Quit();

            }
            WMRecipeCust.context.StartCoroutine(temp.LoadAllRecipeData(true)); // Singleplayer Reload
            yield break;
        }


    }
}
