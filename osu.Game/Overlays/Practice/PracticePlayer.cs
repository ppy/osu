// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Practice.PracticeOverlayComponents;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Overlays.Practice
{
    public partial class PracticePlayer : Player
    {
        protected PracticeOverlay PracticeOverlay = null!;
        private readonly PracticePlayerLoader loader;

        private const double grace_period = 3000;

        private double customEndTime;
        private double customStartTime;

        protected bool BlockFail = true;

        public PracticePlayer(PracticePlayerLoader loader) => this.loader = loader;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            createPauseOverlay();

            addButtons(colour);

            createHUDElements();
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

            //Set it to false after the last action that depends on it
            loader.IsFirstTry = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            practiceOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(PracticeOverlay);

            if (loader.IsFirstTry)
                PracticeOverlay.Show();

            else
                //Todo: PauseOverlay seems to be being triggered by something unduly
                PauseOverlay.Hide();

            OnGameplayStarted += () =>
                GameplayClockContainer.Delay(grace_period).Then().Schedule(() => BlockFail = false);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            practiceOverlayRegistration?.Dispose();
        }

        private void createPauseOverlay()
        {
            LoadComponent(PracticeOverlay = new PracticeOverlay(loader)
            {
                Restart = () =>
                {
                    //Stops the pauseoverlay showing for a moment as we restart
                    PauseOverlay.Expire();
                    Restart();
                },
                OnShow = () =>
                {
                    GameplayClockContainer.Hide();
                },
                OnHide = () =>
                {
                    GameplayClockContainer.Show();

                    // We don't want the pause overlay triggering on exit if the player is not alive
                    if (GameplayState.HasFailed)
                    {
                        FailOverlay.Show();
                        return;
                    }

                    PauseOverlay.Show();
                }
            });
        }

        //Probably not ideal, but avoids issues that calculating custom end based on gameplaystart has with pausing
        private bool isRestarting = true;

        protected override void Update()
        {
            base.Update();

            if (!(GameplayClockContainer.CurrentTime > customEndTime) || !isRestarting) return;

            isRestarting = false;
            Restart();
        }

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => PracticeOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => PracticeOverlay.Show());
        }

        private void createHUDElements() =>
            HUDOverlay.Add(new PracticePercentageCounter(loader)
            {
                State = { Value = Visibility.Visible },
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,

                //We don't want it clipping the health bars on the default skin, this offset also avoids elements on *Most* legacy skins
                Position = new Vector2(-20, 120),
            });

        protected override bool CheckModsAllowFailure() =>
            !BlockFail && base.CheckModsAllowFailure();
    }
}
