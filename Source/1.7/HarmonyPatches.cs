using Verse;
using HarmonyLib;

namespace DismissLetters;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    private static readonly Harmony harmony = new("dani.autoNotifRemover");

    static HarmonyPatches()
    {
        harmony.Patch(typeof(LetterStack).GetMethod(nameof(LetterStack.ReceiveLetter), [typeof(Letter), typeof(string), typeof(int), typeof(bool)]), postfix: new HarmonyMethod(typeof(LetterManager), nameof(LetterManager.AddLetter)));
        harmony.Patch(typeof(LetterStack).GetMethod(nameof(LetterStack.RemoveLetter)), postfix: new HarmonyMethod(typeof(LetterManager), nameof(LetterManager.RemoveLetter)));
    }
}
