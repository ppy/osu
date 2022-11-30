// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    public partial class PracticePlayer : Player
    {
        private PracticeOverlay practiceOverlay = null!;
        private readonly PracticePlayerLoader loader;

        private const double grace_period = 3000;

        private double customEndTime;
        private double customStartTime;

        private bool blockFail = true;

        public PracticePlayer(PracticePlayerLoader loader)
        {
            this.loader = loader;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            createPauseOverlay();
            addButtons(colour);
        }

        [Resolved(CanBeNull = true)]
        internal IOverlayManager? OverlayManager { get; private set; }

        private IDisposable? practiceOverlayRegistration;

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
        {
            var masterGameplayClockContainer = new MasterGameplayClockContainer(beatmap, gameplayStart);
            var playableBeatmap = beatmap.GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset);

            double beatmapLength = playableBeatmap.HitObjects.LastOrDefault()!.GetEndTime() - playableBeatmap.HitObjects.FirstOrDefault()!.StartTime;

            //If this isn't done infinite value weirdness happens
            if (loader.CustomStart.Value != loader.CustomStart.Default)
            {
                customStartTime = playableBeatmap.HitObjects.FirstOrDefault()!.StartTime + loader.CustomStart.Value * beatmapLength;
            }

            customEndTime = playableBeatmap.HitObjects.FirstOrDefault()!.StartTime + beatmapLength * loader.CustomEnd.Value;

            //Make sure to only use custom startTime if it is bigger ( later) than the original one by a meaningful amount.
            if (customStartTime - 10 > gameplayStart)
                masterGameplayClockContainer.Reset(customStartTime);

            return masterGameplayClockContainer;
        }

        protected override void StartGameplay()
        {
            //We dont want the gameplay running on the first attempt since the practice screen is being shown automatically
            if (!loader.IsFirstTry)
            {
                base.StartGameplay();
            }

            //set it to false after the last action that depends on it
            loader.IsFirstTry = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            practiceOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(practiceOverlay);

            if (loader.IsFirstTry)
            {
                practiceOverlay.Show();
            }
            else
            {
                //Todo: PauseOverlay seems to be being triggered by something unduly
                PauseOverlay.Hide();
            }

            OnGameplayStarted += () =>
            {
                GameplayClockContainer.Delay(customEndTime - customStartTime).Then().Schedule(() => Restart());
                GameplayClockContainer.Delay(grace_period).Then().Schedule(() => blockFail = false);
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            practiceOverlayRegistration?.Dispose();
        }

        private void createPauseOverlay()
        {
            LoadComponent(practiceOverlay = new PracticeOverlay(loader)
            {
                Restart = () => Restart(),
                OnHide = () => PauseOverlay.Show(),
                OnShow = () => PauseOverlay.Hide()
            });
        }

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
        }

        protected override bool CheckModsAllowFailure()
        {
            if (blockFail) return false;

            return base.CheckModsAllowFailure();
        }
    }
}
