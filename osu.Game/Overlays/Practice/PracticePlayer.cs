// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
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
        private PracticeOverlay practiceOverlay = null!;

        public Ruleset CurrentRuleset = null!;

        [Resolved]
        private PracticePlayerLoader loader { get; set; } = null!;

        private readonly Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore;

        public PracticePlayer(PlayerConfiguration? configuration = null)
            : base(configuration)
        {
            createScore = (beatmap, mods) => mods.OfType<ModAutoplay>().First().CreateScoreFromReplayData(beatmap, mods);
        }

        protected override Score CreateScore(IBeatmap beatmap) => createScore(beatmap, Mods.Value);

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            var rulesetInfo = Ruleset.Value ?? Beatmap.Value.BeatmapInfo.Ruleset;

            CurrentRuleset = rulesetInfo.CreateInstance();

            SetGameplayStartTime(loader.CustomStart.Value * PlayableBeatmap.HitObjects.Last().StartTime);

            AddInternal(practiceOverlay = new PracticeOverlay
            {
                State = { Value = Visibility.Visible }
            });
            addButtons(colour);
        }

        protected override bool CheckModsAllowFailure() => false; // never fail.

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
        }
    }
}
