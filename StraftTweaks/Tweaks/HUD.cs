
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(PlayerHealth), "Update")]
static class MinHUDPatch
{
    internal static ConfigEntry<bool> hidePing;
    internal static ConfigEntry<bool> hideFPS;

    static void Postfix(PlayerHealth __instance)
    {
        if (hidePing.Value)
            __instance.controller.pauseManager.minimalPingText.text = "";
        if (hideFPS.Value)
            __instance.controller.pauseManager.minimalFpsText.text = "";
    }
}