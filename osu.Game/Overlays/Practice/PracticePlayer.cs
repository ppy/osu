// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    public class PracticePlayer : Player
    {
        private PracticeOverlay practiceOverlay = null!;

        public PracticePlayer(PlayerConfiguration? configuration = null)
            : base(configuration)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, IBindable<WorkingBeatmap> beatmap, PracticePlayerLoader loader)
        {
            var playableBeatmap = beatmap.Value.GetPlayableBeatmap(beatmap.Value.BeatmapInfo.Ruleset);

            SetGameplayStartTime(loader.CustomStart.Value * (playableBeatmap.HitObjects.Last().StartTime - playableBeatmap.HitObjects.First().StartTime));

            AddInternal(practiceOverlay = new PracticeOverlay(() => Restart())
            {
                State = { Value = Visibility.Visible }
            });
            addButtons(colour);
        }

        protected override bool CheckModsAllowFailure() => false; // never fail. Todo: find a way to avoid instantly failing after initial seek

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
        }
    }
}
