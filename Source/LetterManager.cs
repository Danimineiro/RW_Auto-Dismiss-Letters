using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;
using DismissLetters.Settings;
using HarmonyLib;
using System.Linq;

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
            if (!AutoDismissMod.Settings.enabled || Find.TickManager?.CurTimeSpeed != TimeSpeed.Paused) return;
            long curTime = DateTime.UtcNow.ToFileTimeUtc();
            if (curTime < nextCheck) return;

            List<Letter> RemoveList = new List<Letter>();

            foreach (KeyValuePair<Letter, long> pair in letters)
            {
                bool flag = (AutoDismissMod.Settings.realtime ? curTime : Find.TickManager.TicksGame) > pair.Value && pair.Key.CanDismissWithRightClick;

                if (flag)
                {
                    RemoveList.Add(pair.Key);

                    if (AutoDismissMod.Settings.makeSoundWhenLetterRemoved)
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }

            }
        
            foreach (Letter letter in RemoveList)
            {
                letters.Remove(letter);
                Find.LetterStack.RemoveLetter(letter);
            }

            nextCheck = NextCheckTime;
            base.WorldComponentTick();
        }

        public void RefreshAllLetters()
        {
            List<Letter> list = letters.Keys.ToList();
            letters.Clear();
            foreach (Letter letter in list)
            {
                AddLetter(letter);
            }
        }

        public static void AddLetter(Letter let)
        {
            if (AutoDismissMod.Settings.realtime)
            {
                Instance.letters.Add(let, DateTime.UtcNow.AddSeconds(AutoDismissMod.Settings.dismissLetterIfOlderThanSeconds).ToFileTimeUtc());
                return;
            }

            Instance.letters.Add(let, Find.TickManager.TicksGame + AutoDismissMod.Settings.dismissLetterIfOlderThanSeconds);
        }

        public static void RemoveLetter(Letter let) => Instance.letters.Remove(let);

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref letters, nameof(letters), LookMode.Reference, LookMode.Value, ref letterList, ref letterTime);

            base.ExposeData();
        }
    }
}
