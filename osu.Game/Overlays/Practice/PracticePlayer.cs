// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    public partial class PracticePlayer : Player
    {
        private PracticeOverlay practiceOverlay = null!;
        private readonly PracticePlayerLoader loader;

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
            double customStartTime = loader.CustomStart.Value * (playableBeatmap.HitObjects.Last().StartTime - playableBeatmap.HitObjects.First().StartTime);

            //Make sure to only use custom startTime if it is bigger ( later) than the original one.
            if (customStartTime - 1 > gameplayStart)
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
            blockFail = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            practiceOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(practiceOverlay);

            if (loader.IsFirstTry)
            {
                practiceOverlay.Show();
                PauseOverlay.Hide();
            }
            else
            {
                //Todo: remove this hack!. PauseOverlay seems to be being triggered by something
                PauseOverlay.Hide();
            }
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
                OnHide = () => PauseOverlay.Show()
            });
        }

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => practiceOverlay.Show());
        }

        protected override bool CheckModsAllowFailure()
        {
            return blockFail && base.CheckModsAllowFailure();
        }
    }
}
