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
        private Dictionary<Letter, long> letterAge = new Dictionary<Letter, long>();

        private List<Letter> letterList = new List<Letter>();
        private List<long> letterTime = new List<long>();

        private DateTime nextCheck = DateTime.Now;

        public static LetterManager Instance => instance;

        public LetterManager(World world) : base(world) 
        {
            instance = this;
        }

        public override void WorldComponentUpdate()
        {
            if (!AutoDismissMod.Settings.enabled || !AutoDismissMod.Settings.realtime) return;
            if (DateTime.Compare(nextCheck, DateTime.Now) > 0) return;

            nextCheck += new TimeSpan(AutoDismissMod.Settings.checkEverySecondsAmount * TimeSpan.TicksPerSecond); 

            List<Letter> RemoveList = new List<Letter>();
            foreach ((Letter letter, long age) in letterAge)
            {
                if (DateTime.Compare(new DateTime(age), DateTime.Now) > 0) return;

                if (letter.CanDismissWithRightClick)
                {
                    RemoveList.Add(letter);

                    if (AutoDismissMod.Settings.makeSoundWhenLetterRemoved)
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }

            }
        
            foreach (Letter letter in RemoveList)
            {
                letterAge.Remove(letter);
                Find.LetterStack.RemoveLetter(letter);
            }
        }

        public override void WorldComponentTick()
        {
            if (!AutoDismissMod.Settings.enabled || AutoDismissMod.Settings.realtime) return;
            if (DateTime.Compare(nextCheck, DateTime.Now) > 0) return;

            nextCheck += new TimeSpan(AutoDismissMod.Settings.checkEverySecondsAmount * TimeSpan.TicksPerSecond);

            List<Letter> RemoveList = new List<Letter>();
            foreach ((Letter letter, long age) in letterAge)
            {
                if (Find.TickManager.TicksGame - age < 0) return;

                if (letter.CanDismissWithRightClick)
                {
                    RemoveList.Add(letter);

                    if (AutoDismissMod.Settings.makeSoundWhenLetterRemoved)
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }

            }

            foreach (Letter letter in RemoveList)
            {
                letterAge.Remove(letter);
                Find.LetterStack.RemoveLetter(letter);
            }
        }

        public void RefreshAllLetters()
        {
            Letter[] list = letterAge.Keys.ToArray();
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
}
