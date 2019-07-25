// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Play
{
    public class HUDOverlay : Container
    {
        private const int duration = 250;
        private const Easing easing = Easing.OutQuint;

        public readonly KeyCounterDisplay KeyCounter;
        public readonly RollingCounter<int> ComboCounter;
        public readonly ScoreCounter ScoreCounter;
        public readonly RollingCounter<double> AccuracyCounter;
        public readonly HealthDisplay HealthDisplay;
        public readonly SongProgress Progress;
        public readonly ModDisplay ModDisplay;
        public readonly HoldForMenuButton HoldToQuit;
        public readonly PlayerSettingsOverlay PlayerSettingsOverlay;
        public readonly InGameLeaderboard InGameLeaderboard;

        public Bindable<bool> ShowHealthbar = new Bindable<bool>(true);

        public readonly IBindable<bool> IsBreakTime = new Bindable<bool>();

        private Bindable<bool> alwaysShowLeaderboard;
        private readonly OsuSpriteText leaderboardText;

        private readonly ScoreProcessor scoreProcessor;
        private readonly DrawableRuleset drawableRuleset;
        private readonly IReadOnlyList<Mod> mods;

        private Bindable<bool> showHud;
        private readonly Container visibilityContainer;
        private readonly BindableBool replayLoaded = new BindableBool();

        private static bool hasShownNotificationOnce;

        public Action<double> RequestSeek;

        private readonly Container topScoreContainer;

        public HUDOverlay(ScoreProcessor scoreProcessor, DrawableRuleset drawableRuleset, IReadOnlyList<Mod> mods)
        {
            this.scoreProcessor = scoreProcessor;
            this.drawableRuleset = drawableRuleset;
            this.mods = mods;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                visibilityContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        topScoreContainer = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            AutoSizeDuration = 200,
                            AutoSizeEasing = Easing.Out,
                            Children = new Drawable[]
                            {
                                AccuracyCounter = CreateAccuracyCounter(),
                                ScoreCounter = CreateScoreCounter(),
                                ComboCounter = CreateComboCounter(),
                            },
                        },
                        HealthDisplay = CreateHealthDisplay(),
                        Progress = CreateProgress(),
                        ModDisplay = CreateModsContainer(),
                    }
                },
                PlayerSettingsOverlay = CreatePlayerSettingsOverlay(),
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = -new Vector2(5, TwoLayerButton.SIZE_RETRACTED.Y),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        KeyCounter = CreateKeyCounter(),
                        HoldToQuit = CreateHoldForMenuButton(),
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = -new Vector2(5, TwoLayerButton.SIZE_RETRACTED.Y),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Bottom = 30, Left = 20 },
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        InGameLeaderboard = CreateInGameLeaderboard(),
                        leaderboardText = CreateLeaderboardText(),
                    }
                }
            };
        }

        private void updateLeaderboardState()
        {
            if (alwaysShowLeaderboard.Value)
                return;

            if (IsBreakTime.Value)
                InGameLeaderboard.FadeIn(duration, easing);
            else
                InGameLeaderboard.FadeTo(0.001f, duration, easing); // we don't want to fade the leaderboard entirely else the leaderboard text will fall to the bottom
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationOverlay notificationOverlay)
        {
            BindProcessor(scoreProcessor);
            BindDrawableRuleset(drawableRuleset);

            Progress.Objects = drawableRuleset.Objects;
            Progress.AllowSeeking = drawableRuleset.HasReplayLoaded.Value;
            Progress.RequestSeek = time => RequestSeek(time);
            Progress.ReferenceClock = drawableRuleset.FrameStableClock;

            ModDisplay.Current.Value = mods;

            showHud = config.GetBindable<bool>(OsuSetting.ShowInterface);
            showHud.BindValueChanged(visible => visibilityContainer.FadeTo(visible.NewValue ? 1 : 0, duration, easing), true);

            IsBreakTime.ValueChanged += _ => updateLeaderboardState();

            alwaysShowLeaderboard = config.GetBindable<bool>(OsuSetting.AlwaysShowInGameLeaderboard);
            alwaysShowLeaderboard.ValueChanged += alwaysShow =>
            {
                if (!InGameLeaderboard.ScoresContainer.Any())
                    return;

                if (alwaysShow.NewValue)
                    InGameLeaderboard.FadeIn(duration, easing);
                else
                    updateLeaderboardState();

                if (IsBreakTime.Value)
                {
                    leaderboardText.Text = alwaysShow.NewValue
                        ? "The scoreboard will be shown at all times!"
                        : "The scoreboard will be hidden after this break ends!";

                    leaderboardText.FadeOutFromOne(1000, Easing.InQuint);
                }
            };

            ShowHealthbar.BindValueChanged(healthBar =>
            {
                if (healthBar.NewValue)
                {
                    HealthDisplay.FadeIn(duration, easing);
                    topScoreContainer.MoveToY(30, duration, easing);
                }
                else
                {
                    HealthDisplay.FadeOut(duration, easing);
                    topScoreContainer.MoveToY(0, duration, easing);
                }
            }, true);

            if (!showHud.Value && !hasShownNotificationOnce)
            {
                hasShownNotificationOnce = true;

                notificationOverlay?.Post(new SimpleNotification
                {
                    Text = @"The score overlay is currently disabled. You can toggle this by pressing Shift+Tab."
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            replayLoaded.BindValueChanged(replayLoadedValueChanged, true);

            if (InGameLeaderboard.ScoresContainer.Any())
            {
                leaderboardText.Text = "Hit <TAB> to toggle scoreboard!";
                leaderboardText.FadeOutFromOne(2000, Easing.InQuint);

                if (!alwaysShowLeaderboard.Value)
                    InGameLeaderboard.FadeOut(2000, Easing.InQuint);
            }
        }

        private void replayLoadedValueChanged(ValueChangedEvent<bool> e)
        {
            PlayerSettingsOverlay.ReplayLoaded = e.NewValue;

            if (e.NewValue)
            {
                PlayerSettingsOverlay.Show();
                ModDisplay.FadeIn(200);
                KeyCounter.Margin = new MarginPadding(10) { Bottom = 30 };
            }
            else
            {
                PlayerSettingsOverlay.Hide();
                ModDisplay.Delay(2000).FadeOut(200);
                KeyCounter.Margin = new MarginPadding(10);
            }
        }

        protected virtual void BindDrawableRuleset(DrawableRuleset drawableRuleset)
        {
            (drawableRuleset as ICanAttachKeyCounter)?.Attach(KeyCounter);

            replayLoaded.BindTo(drawableRuleset.HasReplayLoaded);

            Progress.BindDrawableRuleset(drawableRuleset);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            if (e.Key == Key.Tab)
            {
                if (e.ShiftPressed)
                    showHud.Value = !showHud.Value;
                else if (!e.AltPressed && !e.ControlPressed)
                    alwaysShowLeaderboard.Value = !alwaysShowLeaderboard.Value;

                return true;
            }

            return base.OnKeyDown(e);
        }

        protected virtual RollingCounter<double> CreateAccuracyCounter() => new PercentageCounter
        {
            TextSize = 20,
            BypassAutoSizeAxes = Axes.X,
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopRight,
            Margin = new MarginPadding { Top = 5, Right = 20 },
        };

        protected virtual ScoreCounter CreateScoreCounter() => new ScoreCounter(6)
        {
            TextSize = 40,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
        };

        protected virtual RollingCounter<int> CreateComboCounter() => new SimpleComboCounter
        {
            TextSize = 20,
            BypassAutoSizeAxes = Axes.X,
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopLeft,
            Margin = new MarginPadding { Top = 5, Left = 20 },
        };

        protected virtual HealthDisplay CreateHealthDisplay() => new StandardHealthDisplay
        {
            Size = new Vector2(1, 5),
            RelativeSizeAxes = Axes.X,
            Margin = new MarginPadding { Top = 20 }
        };

        protected virtual KeyCounterDisplay CreateKeyCounter() => new KeyCounterDisplay
        {
            FadeTime = 50,
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
            Margin = new MarginPadding(10),
        };

        protected virtual SongProgress CreateProgress() => new SongProgress
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            RelativeSizeAxes = Axes.X,
        };

        protected virtual InGameLeaderboard CreateInGameLeaderboard() => new InGameLeaderboard
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            Width = 80,
        };

        protected virtual OsuSpriteText CreateLeaderboardText() => new OsuSpriteText
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            Font = OsuFont.GetFont(weight: FontWeight.Bold),
            Alpha = 0,
        };

        protected virtual HoldForMenuButton CreateHoldForMenuButton() => new HoldForMenuButton
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
        };

        protected virtual ModDisplay CreateModsContainer() => new ModDisplay
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            AutoSizeAxes = Axes.Both,
            Margin = new MarginPadding { Top = 20, Right = 10 },
        };

        protected virtual PlayerSettingsOverlay CreatePlayerSettingsOverlay() => new PlayerSettingsOverlay();

        protected virtual void BindProcessor(ScoreProcessor processor)
        {
            ScoreCounter?.Current.BindTo(processor.TotalScore);
            AccuracyCounter?.Current.BindTo(processor.Accuracy);
            ComboCounter?.Current.BindTo(processor.Combo);
            HealthDisplay?.Current.BindTo(processor.Health);

            InGameLeaderboard?.UserTotalScore.BindTo(processor.TotalScore);

            if (HealthDisplay is StandardHealthDisplay shd)
                processor.NewJudgement += shd.Flash;
        }
    }
}
