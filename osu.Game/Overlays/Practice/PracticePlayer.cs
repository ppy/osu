// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    [Cached]
    public class PracticePlayer : Player
    {
        public PracticeOverlay PracticeOverlay = null!;

        public Ruleset CurrentRuleset = null!;

        [Resolved]
        private PracticePlayerLoader loader { get; set; } = null!;

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
