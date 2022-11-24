// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    [Cached]
    public class PracticePlayer : Player
    {
        public readonly Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore;

        public PracticeOverlay PracticeOverlay = null!;

        public Ruleset CurrentRuleset = null!;

        [Resolved]
        private PracticePlayerLoader loader { get; set; } = null!;

        public PracticePlayer(Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore, PlayerConfiguration? configuration = null)
            : base(configuration)
        {
            this.createScore = createScore;
        }

        protected override Score CreateScore(IBeatmap beatmap) => createScore(beatmap, Mods.Value);

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            var rulesetInfo = Ruleset.Value ?? Beatmap.Value.BeatmapInfo.Ruleset;

            CurrentRuleset = rulesetInfo.CreateInstance();

            AddInternal(PracticeOverlay = new PracticeOverlay());

            addButtons(colour);

            SetGameplayStartTime(loader.CustomStart.Value * PlayableBeatmap.HitObjects.Last().StartTime);
        }

        //Hack to avoid auto failing due to lag upon entry
        protected override bool CheckModsAllowFailure() => false; // never fail.

        protected override void Update()
        {
            base.Update();

            if (!PracticeOverlay.IsPresent) return;

            GameplayClockContainer.Stop();
            GameplayClockContainer.Hide();
        }

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => PracticeOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => PracticeOverlay.Show());
        }
    }
}
