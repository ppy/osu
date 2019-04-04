// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Play
{
    public class HUDOverlay : Container
    {
        private const int duration = 100;

        public readonly KeyCounterDisplay KeyCounter;
        public readonly RollingCounter<int> ComboCounter;
        public readonly ScoreCounter ScoreCounter;
        public readonly RollingCounter<double> AccuracyCounter;
        public readonly HealthDisplay HealthDisplay;
        public readonly SongProgress Progress;
        public readonly ModDisplay ModDisplay;
        public readonly HoldForMenuButton HoldToQuit;
        public readonly PlayerSettingsOverlay PlayerSettingsOverlay;

        private Bindable<bool> showHud;
        private readonly Container visibilityContainer;
        private readonly BindableBool replayLoaded = new BindableBool();

        private static bool hasShownNotificationOnce;

        public Action<double> RequestSeek;

        public HUDOverlay(ScoreProcessor scoreProcessor, DrawableRuleset drawableRuleset, WorkingBeatmap working)
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                visibilityContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Y = 30,
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
                }
            };

            BindProcessor(scoreProcessor);
            BindDrawableRuleset(drawableRuleset);

            Progress.Objects = drawableRuleset.Objects;
            Progress.AllowSeeking = drawableRuleset.HasReplayLoaded.Value;
            Progress.RequestSeek = time => RequestSeek(time);

            ModDisplay.Current.BindTo(working.Mods);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationOverlay notificationOverlay)
        {
            showHud = config.GetBindable<bool>(OsuSetting.ShowInterface);
            showHud.ValueChanged += visible => visibilityContainer.FadeTo(visible.NewValue ? 1 : 0, duration);
            showHud.TriggerChange();

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

            replayLoaded.ValueChanged += replayLoadedValueChanged;
            replayLoaded.TriggerChange();
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

            if (e.ShiftPressed)
            {
                switch (e.Key)
                {
                    case Key.Tab:
                        showHud.Value = !showHud.Value;
                        return true;
                }
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

            if (HealthDisplay is StandardHealthDisplay shd)
                processor.NewJudgement += shd.Flash;
        }
    }
}
