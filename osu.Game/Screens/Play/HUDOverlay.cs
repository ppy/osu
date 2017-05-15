// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public abstract class HUDOverlay : Container
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

        private Bindable<bool> showKeyCounter;
        private Bindable<bool> showHud;

        private static bool hasShownNotificationOnce;

        protected abstract KeyCounterCollection CreateKeyCounter();
        protected abstract RollingCounter<int> CreateComboCounter();
        protected abstract RollingCounter<double> CreateAccuracyCounter();
        protected abstract ScoreCounter CreateScoreCounter();
        protected abstract HealthDisplay CreateHealthDisplay();
        protected abstract SongProgress CreateProgress();
        protected abstract ModDisplay CreateModsContainer();

        protected HUDOverlay()
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
                }
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationManager notificationManager)
        {
            showKeyCounter = config.GetBindable<bool>(OsuSetting.KeyOverlay);
            showKeyCounter.ValueChanged += keyCounterVisibility =>
            {
                if (keyCounterVisibility)
                    KeyCounter.FadeIn(duration);
                else
                    KeyCounter.FadeOut(duration);
            };
            showKeyCounter.TriggerChange();

            showHud = config.GetBindable<bool>(OsuSetting.ShowInterface);
            showHud.ValueChanged += hudVisibility =>
            {
                if (hudVisibility)
                    content.FadeIn(duration);
                else
                    content.FadeOut(duration);
            };
            showHud.TriggerChange();

            if (!showHud && !hasShownNotificationOnce)
            {
                hasShownNotificationOnce = true;

                notificationManager?.Post(new SimpleNotification
                {
                    Text = @"The score overlay is currently disabled. You can toggle this by pressing Shift+Tab."
                });
            }
        }

        public virtual void BindProcessor(ScoreProcessor processor)
        {
            ScoreCounter?.Current.BindTo(processor.TotalScore);
            AccuracyCounter?.Current.BindTo(processor.Accuracy);
            ComboCounter?.Current.BindTo(processor.Combo);
            HealthDisplay?.Current.BindTo(processor.Health);
        }

        public virtual void BindHitRenderer(HitRenderer hitRenderer)
        {
            hitRenderer.InputManager.Add(KeyCounter.GetReceptor());

            // in the case a replay isn't loaded, we want some elements to only appear briefly.
            if (!hitRenderer.HasReplayLoaded)
                using (ModDisplay.BeginDelayedSequence(2000))
                    ModDisplay.FadeOut(200);
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
