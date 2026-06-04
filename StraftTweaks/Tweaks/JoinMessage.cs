using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;

[HarmonyPatch(typeof(ClientInstance), "OnStartClient")]
static class SetJoinMessage
{
    internal static ConfigEntry<string> message;

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var joinMessage = AccessTools.Method(typeof(SetJoinMessage), "JoinMessage");

        return new CodeMatcher(instructions)
        .MatchForward(useEnd: false,
        new CodeMatch(OpCodes.Ldstr, " joined the lobby"))
        .Advance(-1)
        .RemoveInstructions(3)
        .Insert(
            new CodeInstruction(OpCodes.Call, joinMessage))
        .InstructionEnumeration();
    }

    static string JoinMessage()
    {
        return message.Value.Replace("{USER}", SteamFriends.GetPersonaName());
    }
}