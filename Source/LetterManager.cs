using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;
using DismissLetters.Settings;

namespace DismissLetters
{
    internal class LetterManager : WorldComponent
    {
        private static LetterManager instance;
        private Dictionary<Letter, long> letters = new Dictionary<Letter, long>();
        private List<Letter> letterList = new List<Letter>();
        private List<long> letterTime = new List<long>();
        private long nextCheck = NextCheckTime;

        public static LetterManager Instance => instance;
        private static long NextCheckTime => DateTime.UtcNow.AddSeconds(AutoDismissMod.Settings.checkEverySecondsAmount).ToFileTimeUtc();

        public LetterManager(World world) : base(world) 
        {
            instance = this;
        }

        public override void WorldComponentUpdate()
        {
            if (!AutoDismissMod.Settings.enabled) return;

            long curTime = DateTime.UtcNow.ToFileTimeUtc();

            if (curTime < nextCheck) return;

            letters.RemoveAll(pair =>
            {
                bool flag = curTime > pair.Value && pair.Key.CanDismissWithRightClick;

                if (flag)
                {
                    Find.LetterStack.RemoveLetter(pair.Key);
                    if (AutoDismissMod.Settings.makeSoundWhenLetterRemoved)
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }

                return flag;
            });

            nextCheck = NextCheckTime;
            base.WorldComponentTick();
        }

        public static void AddLetter(Letter let) => Instance.letters.Add(let, DateTime.UtcNow.AddSeconds(AutoDismissMod.Settings.dismissLetterIfOlderThanSeconds).ToFileTimeUtc());

        public static void RemoveLetter(Letter let) => Instance.letters.Remove(let);

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref letters, nameof(letters), LookMode.Reference, LookMode.Value, ref letterList, ref letterTime);

            base.ExposeData();
        }
    }
}
