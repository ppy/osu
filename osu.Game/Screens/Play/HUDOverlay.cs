﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class HUDOverlay : Container
    {
        private const int duration = 100;

        private readonly Container content;

        public readonly KeyCounterCollection KeyCounter;
        public readonly RollingCounter<int> ComboCounter;
        public readonly ScoreCounter ScoreCounter;
        public readonly RollingCounter<double> AccuracyCounter;
        public readonly HealthDisplay HealthDisplay;
        public readonly SongProgress Progress;
        public readonly ModDisplay ModDisplay;

        private Bindable<bool> showHud;
        private bool replayLoaded;

        private static bool hasShownNotificationOnce;

        public HUDOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,

                Children = new Drawable[]
                {
                    KeyCounter = CreateKeyCounter(),
                    ComboCounter = CreateComboCounter(),
                    ScoreCounter = CreateScoreCounter(),
                    AccuracyCounter = CreateAccuracyCounter(),
                    HealthDisplay = CreateHealthDisplay(),
                    Progress = CreateProgress(),
                    ModDisplay = CreateModsContainer(),
                    //ReplaySettingsOverlay = CreateReplaySettingsOverlay(),
                }
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationManager notificationManager, OsuColour colours)
        {
            showHud = config.GetBindable<bool>(OsuSetting.ShowInterface);
            showHud.ValueChanged += hudVisibility => content.FadeTo(hudVisibility ? 1 : 0, duration);
            showHud.TriggerChange();

            if (!showHud && !hasShownNotificationOnce)
            {
                hasShownNotificationOnce = true;

                notificationManager?.Post(new SimpleNotification
                {
                    Text = @"The score overlay is currently disabled. You can toggle this by pressing Shift+Tab."
                });
            }

            // todo: the stuff below should probably not be in this base implementation, but in each individual class.
            ComboCounter.AccentColour = colours.BlueLighter;
            AccuracyCounter.AccentColour = colours.BlueLighter;
            ScoreCounter.AccentColour = colours.BlueLighter;

            var shd = HealthDisplay as StandardHealthDisplay;
            if (shd != null)
            {
                shd.AccentColour = colours.BlueLighter;
                shd.GlowColour = colours.BlueDarker;
            }
        }

        public virtual void BindHitRenderer(HitRenderer hitRenderer)
        {
            hitRenderer.InputManager.Add(KeyCounter.GetReceptor());

            replayLoaded = hitRenderer.HasReplayLoaded;

            // in the case a replay isn't loaded, we want some elements to only appear briefly.
            if (!replayLoaded)
            {
                using (ModDisplay.BeginDelayedSequence(2000))
                    ModDisplay.FadeOut(200);
            }
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            if (state.Keyboard.ShiftPressed)
            {
                switch (args.Key)
                {
                    case Key.Tab:
                        showHud.Value = !showHud.Value;
                        return true;
                }
            }

            return base.OnKeyDown(state, args);
        }

        protected virtual RollingCounter<double> CreateAccuracyCounter() => new PercentageCounter
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopRight,
            Position = new Vector2(0, 35),
            TextSize = 20,
            Margin = new MarginPadding { Right = 140 },
        };

        protected virtual RollingCounter<int> CreateComboCounter() => new SimpleComboCounter
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopLeft,
            Position = new Vector2(0, 35),
            Margin = new MarginPadding { Left = 140 },
            TextSize = 20,
        };

        protected virtual HealthDisplay CreateHealthDisplay() => new StandardHealthDisplay
        {
            Size = new Vector2(1, 5),
            RelativeSizeAxes = Axes.X,
            Margin = new MarginPadding { Top = 20 }
        };

        protected virtual KeyCounterCollection CreateKeyCounter() => new KeyCounterCollection
        {
            IsCounting = true,
            FadeTime = 50,
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
            Margin = new MarginPadding(10),
            Y = -TwoLayerButton.SIZE_RETRACTED.Y,
        };

        protected virtual ScoreCounter CreateScoreCounter() => new ScoreCounter(6)
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            TextSize = 40,
            Position = new Vector2(0, 30),
        };

        protected virtual SongProgress CreateProgress() => new SongProgress
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            RelativeSizeAxes = Axes.X,
        };

        protected virtual ModDisplay CreateModsContainer() => new ModDisplay
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            AutoSizeAxes = Axes.Both,
            Margin = new MarginPadding { Top = 20, Right = 10 },
        };

        //protected virtual ReplaySettingsOverlay CreateReplaySettingsOverlay() => new ReplaySettingsOverlay
        //{
        //    Anchor = Anchor.TopRight,
        //    Origin = Anchor.TopRight,
        //    Margin = new MarginPadding { Top = 100, Right = 10 },
        //};

        public virtual void BindProcessor(ScoreProcessor processor)
        {
            ScoreCounter?.Current.BindTo(processor.TotalScore);
            AccuracyCounter?.Current.BindTo(processor.Accuracy);
            ComboCounter?.Current.BindTo(processor.Combo);
            HealthDisplay?.Current.BindTo(processor.Health);

            var shd = HealthDisplay as StandardHealthDisplay;
            if (shd != null)
                processor.NewJudgement += shd.Flash;
        }
    }
}
