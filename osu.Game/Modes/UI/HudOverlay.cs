// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using osu.Game.Modes.Scoring;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using OpenTK;

namespace osu.Game.Modes.UI
{
    public abstract class HudOverlay : Container
    {
        private const int duration = 100;

        private readonly Container content;
        public readonly KeyCounterCollection KeyCounter;
        public readonly RollingCounter<int> ComboCounter;
        public readonly ScoreCounter ScoreCounter;
        public readonly RollingCounter<double> AccuracyCounter;
        public readonly HealthDisplay HealthDisplay;
        public readonly SongProgress Progress;

        private Bindable<bool> showKeyCounter;
        private Bindable<bool> showHud;
        private Bindable<bool> showProgress;

        private static bool hasShownNotificationOnce;
        private bool hasPaused;

        protected abstract KeyCounterCollection CreateKeyCounter();
        protected abstract RollingCounter<int> CreateComboCounter();
        protected abstract RollingCounter<double> CreateAccuracyCounter();
        protected abstract ScoreCounter CreateScoreCounter();
        protected abstract HealthDisplay CreateHealthDisplay();
        protected abstract SongProgress CreateProgress();

        protected HudOverlay()
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
                }
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationManager notificationManager)
        {
            showKeyCounter = config.GetBindable<bool>(OsuConfig.KeyOverlay);
            showKeyCounter.ValueChanged += keyCounterVisibility =>
            {
                if (keyCounterVisibility)
                    KeyCounter.FadeIn(duration);
                else
                    KeyCounter.FadeOut(duration);
            };
            showKeyCounter.TriggerChange();

            showHud = config.GetBindable<bool>(OsuConfig.ShowInterface);
            showHud.ValueChanged += hudVisibility =>
            {
                if (hudVisibility)
                    content.FadeIn(duration);
                else
                    content.FadeOut(duration);
            };
            showHud.TriggerChange();

            showProgress = config.GetBindable<bool>(OsuConfig.ShowProgress);
            showProgress.ValueChanged += progressVisibility =>
            {
                if (showProgress)
                {
                    Progress.FadeIn(duration);
                    KeyCounter.MoveToY(-TwoLayerButton.SIZE_RETRACTED.Y, duration);
                }
                else
                {
                    if (!hasPaused)
                    {
                        Progress.FadeOut(duration);
                        KeyCounter.MoveToY(0, duration);
                    }
                }
            };
            showProgress.TriggerChange();

            if (!showHud && !hasShownNotificationOnce)
            {
                hasShownNotificationOnce = true;

                notificationManager?.Post(new SimpleNotification
                {
                    Text = @"The score overlay is currently disabled. You can toggle this by pressing Shift+Tab."
                });
            }
        }

        public void ShowProgress()
        {
            hasPaused = true;
            Progress.FadeIn(duration);
            KeyCounter.MoveToY(-TwoLayerButton.SIZE_RETRACTED.Y, duration);
        }

        public void HideProgress()
        {
            hasPaused = false;
            if (!showProgress)
            {
                Progress.FadeOut(duration);
                KeyCounter.MoveToY(0, duration);
            }
        }

        public void BindProcessor(ScoreProcessor processor)
        {
            ScoreCounter?.Current.BindTo(processor.TotalScore);
            AccuracyCounter?.Current.BindTo(processor.Accuracy);
            ComboCounter?.Current.BindTo(processor.Combo);
            HealthDisplay?.Current.BindTo(processor.Health);
        }

        public void BindHitRenderer(HitRenderer hitRenderer)
        {
            hitRenderer.InputManager.Add(KeyCounter.GetReceptor());
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
    }
}
