// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    public partial class PracticePlayer : Player
    {
        private PracticeOverlay practiceOverlay = null!;

        private bool blockFailure = true;

        public PracticePlayer(PlayerConfiguration? configuration = null)
            : base(configuration)
        {
        }

        [Resolved]
        private PracticePlayerLoader loader { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        internal IOverlayManager? OverlayManager { get; private set; }

        private IDisposable? practiceOverlayRegistration;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, IBindable<WorkingBeatmap> beatmap)
        {
            var playableBeatmap = beatmap.Value.GetPlayableBeatmap(beatmap.Value.BeatmapInfo.Ruleset);

            double customStartTime = loader.CustomStart.Value * (playableBeatmap.HitObjects.Last().StartTime - playableBeatmap.HitObjects.First().StartTime);

            if (customStartTime > playableBeatmap.HitObjects.First().StartTime)
            {
                GameplayClockContainer.Reset(customStartTime);

                OnGameplayStarted += () => Task.Run(async () =>
                {
                    //Todo : create some form of visual indicator of this grace period, possibly settings for its length?
                    await Task.Delay(2000);
                    blockFailure = false;
                });
            }

            addButtons(colour);

            createPauseOverlay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            practiceOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(practiceOverlay);

            if (loader.IsFirstTry)
            {
                practiceOverlay.Show();
            }

            loader.IsFirstTry = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            practiceOverlayRegistration?.Dispose();
        }

        private void createPauseOverlay()
        {
            LoadComponent(practiceOverlay = new PracticeOverlay
            {
                State = { Value = Visibility.Hidden },
                Restart = () => Restart(),
                OnHide = () => { PauseOverlay.Show(); },
            });
        }

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
        }

        protected override bool CheckModsAllowFailure()
        {
            if (blockFailure)
            {
                return false;
            }

            return base.CheckModsAllowFailure();
        }
    }
}
