using DismissLetters.Settings;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace DismissLetters;

internal class LetterManager : WorldComponent
{
    private static LetterManager instance;
    private Dictionary<Letter, long> letterAge = [];

    private List<Letter> letterList = [];
    private List<long> letterTime = [];

    private DateTime nextCheck = DateTime.Now;

    public static LetterManager Instance => instance;

    public LetterManager(World world) : base(world) => instance = this;

    public override void WorldComponentUpdate()
    {
        if (!AutoDismissMod.Settings.enabled || !AutoDismissMod.Settings.realtime) return;
        if (DateTime.Compare(nextCheck, DateTime.Now) > 0) return;

        RemoveLetters(deathTime => DateTime.Now > new DateTime(deathTime));
        nextCheck += new TimeSpan(AutoDismissMod.Settings.checkEverySecondsAmount * TimeSpan.TicksPerSecond);
    }

    public override void WorldComponentTick()
    {
        if (!AutoDismissMod.Settings.enabled || AutoDismissMod.Settings.realtime) return;
        if (DateTime.Compare(nextCheck, DateTime.Now) > 0) return;

        RemoveLetters(deathTime => Find.TickManager.TicksGame > deathTime);
        nextCheck += new TimeSpan(AutoDismissMod.Settings.checkEverySecondsAmount * TimeSpan.TicksPerSecond);
    }

    private void RemoveLetters(Func<long, bool> predicate)
    {
        Letter[] lettersToBeRemoved = [.. letterAge.Where(kvp =>
        {
            (Letter letter, long age) = kvp;

            if (!letter.CanDismissWithRightClick) return false;
            if (!predicate(age)) return false;

            return predicate(age); 
        }).Select(kvp => kvp.Key)];

        if (lettersToBeRemoved.Length > 0 && AutoDismissMod.Settings.makeSoundWhenLetterRemoved)
        {
            SoundDefOf.Click.PlayOneShotOnCamera();
        }

        foreach(Letter letter in lettersToBeRemoved)
        {
            letterAge.Remove(letter);
            Find.LetterStack.RemoveLetter(letter);
        }
    }

    public void RefreshAllLetters()
    {
        Letter[] list = [.. letterAge.Keys];
        letterAge.Clear();
        foreach (Letter letter in list)
        {
            AddLetter(letter);
        }
    }

    public static void AddLetter(Letter let)
    {
        if (AutoDismissMod.Settings.realtime)
        {
            Instance.letterAge.Add(let, (DateTime.Now + new TimeSpan(AutoDismissMod.Settings.dismissLetterIfOlderThanSeconds * TimeSpan.TicksPerSecond)).Ticks);
            return;
        }

        Instance.letterAge.Add(let, Find.TickManager.TicksGame + AutoDismissMod.Settings.dismissLetterIfOlderThanSeconds);
    }

    public static void RemoveLetter(Letter let) => Instance.letterAge.Remove(let);

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref letterAge, nameof(letterAge), LookMode.Reference, LookMode.Value, ref letterList, ref letterTime);

        base.ExposeData();
    }
}
