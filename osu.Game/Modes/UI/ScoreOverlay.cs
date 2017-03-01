﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Modes.Objects;
using OpenTK;
using osu.Framework.Graphics.Primitives;
using osu.Game.Screens.Play;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Framework.Configuration;

namespace osu.Game.Modes.UI
{
    public abstract class ScoreOverlay : Container
    {
        public KeyCounterCollection KeyCounter;
        public ComboCounter ComboCounter;
        public ScoreCounter ScoreCounter;
        public PercentageCounter AccuracyCounter;
        public HealthDisplay HealthDisplay;
        public Score Score { get; set; }

        private Bindable<bool> showKeyCounter;

        protected abstract KeyCounterCollection CreateKeyCounter();
        protected abstract ComboCounter CreateComboCounter();
        protected abstract PercentageCounter CreateAccuracyCounter();
        protected abstract ScoreCounter CreateScoreCounter();
        protected virtual HealthDisplay CreateHealthDisplay() => new HealthDisplay
        {
            Size = new Vector2(1, 5),
            RelativeSizeAxes = Axes.X,
            Margin = new MarginPadding { Top = 20 }
        };

        public virtual void OnHit(HitObject h)
        {
            ComboCounter?.Increment();
            ScoreCounter?.Increment(300);
            AccuracyCounter?.Set(Math.Min(1, AccuracyCounter.Count + 0.01f));
        }

        public virtual void OnMiss(HitObject h)
        {
            ComboCounter?.Roll();
            AccuracyCounter?.Set(AccuracyCounter.Count - 0.01f);
        }

        public ScoreOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[] {
                KeyCounter = CreateKeyCounter(),
                ComboCounter = CreateComboCounter(),
                ScoreCounter = CreateScoreCounter(),
                AccuracyCounter = CreateAccuracyCounter(),
                HealthDisplay = CreateHealthDisplay(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            showKeyCounter = config.GetBindable<bool>(OsuConfig.KeyOverlay);
            showKeyCounter.ValueChanged += visibilityChanged;
            showKeyCounter.TriggerChange();
        }

        private void visibilityChanged(object sender, EventArgs e)
        {
            if (showKeyCounter)
                KeyCounter.Show();
            else
                KeyCounter.Hide();
        }

        public void BindProcessor(ScoreProcessor processor)
        {
            //bind processor bindables to combocounter, score display etc.   
            processor.TotalScore.ValueChanged += delegate { ScoreCounter?.Set((ulong)processor.TotalScore.Value); };
            processor.Accuracy.ValueChanged += delegate { AccuracyCounter?.Set((float)processor.Accuracy.Value); };
            processor.Combo.ValueChanged += delegate { ComboCounter?.Set((ulong)processor.Combo.Value); };
            HealthDisplay?.Current.BindTo(processor.Health);
        }
    }
}
