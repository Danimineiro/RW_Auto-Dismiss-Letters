using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DismissLetters.Settings;

internal class Settings : ModSettings
{
    public bool enabled = true;
    public bool realtime = true;
    public bool makeSoundWhenLetterRemoved = true;
    public int checkEverySecondsAmount = 1;
    public int dismissLetterIfOlderThanSeconds = 30;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enabled, nameof(enabled), true);
        Scribe_Values.Look(ref realtime, nameof(realtime), true);
        Scribe_Values.Look(ref makeSoundWhenLetterRemoved, nameof(makeSoundWhenLetterRemoved), true);
        Scribe_Values.Look(ref checkEverySecondsAmount, nameof(checkEverySecondsAmount), 1);
        Scribe_Values.Look(ref dismissLetterIfOlderThanSeconds, nameof(dismissLetterIfOlderThanSeconds), 30);

        base.ExposeData();
    }
}

internal class AutoDismissMod : Mod
{
    private static Settings settings;
    private string buffer0 = "";
    private string buffer1 = "";

    /// <summary>
    ///     The point in time at which the reset button becomes unavailable again.
    /// </summary>
    private DateTime ResetButtonAvailableTill { get; set; } = DateTime.MinValue;

    public static Settings Settings => settings;

    private bool ResetIsAvailable => DateTime.Now < ResetButtonAvailableTill;

    public AutoDismissMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();
        buffer0 = settings.checkEverySecondsAmount.ToString();
        buffer1 = settings.dismissLetterIfOlderThanSeconds.ToString();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        using DisposableListing_Standard ls = new();

        ls.Begin(inRect);
        ls.CheckboxLabeled("DNADL_Enabled".Translate(), ref settings.enabled);
        ls.CheckboxLabeled("DNADL_Realtime".Translate(), ref settings.realtime, "DNADL_RealtimeTT".Translate());
        ls.CheckboxLabeled("DNADL_Sound".Translate(), ref settings.makeSoundWhenLetterRemoved);
        ls.TextFieldNumericLabeled("DNADL_CheckEverySeconds".Translate(), ref settings.checkEverySecondsAmount, ref buffer0, 1f);
        ls.TextFieldNumericLabeled(settings.realtime ? "DNADL_DismissLetterAgeReal".Translate() : "DNADL_DismissLetterAgeTick".Translate(), ref settings.dismissLetterIfOlderThanSeconds, ref buffer1, settings.checkEverySecondsAmount);
        
        if (ResetIsAvailable)
        {
            DoResetButton(ls);
            return;
        }

        if (!ls.ButtonText("DNADL_Reset".Translate())) return;
        
        ResetButtonAvailableTill = DateTime.Now.AddSeconds(3d);
        SoundDefOf.Click.PlayOneShotOnCamera();
    }

    private void DoResetButton(Listing_Standard ls)
    {
        if (ls.ButtonText("DNADL_YouSure".Translate()))
        {
            settings.enabled = true;
            settings.realtime = true;
            settings.makeSoundWhenLetterRemoved = true;
            settings.checkEverySecondsAmount = 1;
            settings.dismissLetterIfOlderThanSeconds = 30;

            buffer0 = settings.checkEverySecondsAmount.ToString();
            buffer1 = settings.dismissLetterIfOlderThanSeconds.ToString();

            ResetButtonAvailableTill = DateTime.MinValue;
            SoundDefOf.Click.PlayOneShotOnCamera();
        }
    }

    public override void WriteSettings()
    {
        LetterManager.Instance?.RefreshAllLetters();
        base.WriteSettings();
    }

    public override string SettingsCategory() => "Auto Dismiss Letters";
}
