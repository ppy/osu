// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using System;
using osu.Game.Modes.Scoring;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Modes.UI
{
    public abstract class HudOverlay : Container
    {
        public readonly KeyCounterCollection KeyCounter;
        public readonly ComboCounter ComboCounter;
        public readonly ScoreCounter ScoreCounter;
        public readonly PercentageCounter AccuracyCounter;
        public readonly HealthDisplay HealthDisplay;

        private Bindable<bool> showKeyCounter;
        private Bindable<bool> showHud;

        private readonly Container hud;

        private static bool has_shown_notification_once = false;

        protected abstract KeyCounterCollection CreateKeyCounter();
        protected abstract ComboCounter CreateComboCounter();
        protected abstract PercentageCounter CreateAccuracyCounter();
        protected abstract ScoreCounter CreateScoreCounter();
        protected abstract HealthDisplay CreateHealthDisplay();

        protected HudOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Add(hud = new Container
            {
                RelativeSizeAxes = Axes.Both,

                Children = new Drawable[]
                {
                    KeyCounter = CreateKeyCounter(),
                    ComboCounter = CreateComboCounter(),
                    ScoreCounter = CreateScoreCounter(),
                    AccuracyCounter = CreateAccuracyCounter(),
                    HealthDisplay = CreateHealthDisplay(),
                }
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationManager notificationManager)
        {
            showKeyCounter = config.GetBindable<bool>(OsuConfig.KeyOverlay);
            showKeyCounter.ValueChanged += keyCounterVisibilityChanged;
            showKeyCounter.TriggerChange();

            showHud = config.GetBindable<bool>(OsuConfig.ShowInterface);
            showHud.ValueChanged += hudVisibilityChanged;
            showHud.TriggerChange();

            if (!showHud && !has_shown_notification_once)
            {
                has_shown_notification_once = true;

                notificationManager?.Post(new SimpleNotification
                {
                    Text = @"The score overlay is currently disabled. You can toogle this by pressing Shift + Tab."
                });
            }
        }

        private void keyCounterVisibilityChanged(object sender, EventArgs e)
        {
            if (showKeyCounter)
                KeyCounter.Show();
            else
                KeyCounter.Hide();
        }

        private void hudVisibilityChanged(object sender, EventArgs e)
        {
            if (showHud)
                hud.Show();
            else
                hud.Hide();
        }

        public void BindProcessor(ScoreProcessor processor)
        {
            //bind processor bindables to combocounter, score display etc.
            //TODO: these should be bindable binds, not events!
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
