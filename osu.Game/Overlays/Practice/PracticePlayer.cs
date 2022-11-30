// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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

        public PracticePlayer(PracticePlayerLoader loader)
        {
            this.loader = loader;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            createPauseOverlay();

            addButtons(colour);

            createHUDElements(colour);
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

            practiceOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(PracticeOverlay);

            if (loader.IsFirstTry)
            {
                PracticeOverlay.Show();
            }
            else
            {
                //Todo: PauseOverlay seems to be being triggered by something unduly
                PauseOverlay.Hide();
            }

            OnGameplayStarted += () =>
            {
                //Todo:Find a way to make this delay not run when paused
                DrawableRuleset.Delay(customEndTime - customStartTime).Then().Schedule(() =>
                {
                    /* Restart()*/
                });
                GameplayClockContainer.Delay(grace_period).Then().Schedule(() => BlockFail = false);
            };
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
                Restart = () => Restart(),
                OnHide = () => PauseOverlay.Show(),
                OnShow = () => PauseOverlay.Hide()
            });
        }

        private void addButtons(OsuColour colour)
        {
            PauseOverlay.AddButton("Practice", colour.Blue, () => PracticeOverlay.Show());
            FailOverlay.AddButton("Practice", colour.Blue, () => PracticeOverlay.Show());
        }

        private void createHUDElements(OsuColour colour)
        {
            HUDOverlay.Add(new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,

                //We don't want it clipping the health bars on the default skin, this offset also avoids elements on *Most* legacy skins
                Position = new Vector2(-20, 110),
                AutoSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colour.B5.Opacity(.5f),
                        RelativeSizeAxes = Axes.Both,
                        BypassAutoSizeAxes = Axes.Both
                    },
                    new OsuSpriteText
                    {
                        Colour = colour.YellowLight,
                        Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                        Padding = new MarginPadding(10),
                        Text = $"Practicing {Math.Round(loader.CustomStart.Value * 100)}% to {Math.Round(loader.CustomEnd.Value * 100)}%"
                    }
                }
            });
        }

        protected override bool CheckModsAllowFailure()
        {
            if (BlockFail) return false;

            return base.CheckModsAllowFailure();
        }
    }
}
