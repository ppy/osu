//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes
{
    public abstract class ScoreOverlay : Container
    {
        public KeyCounterCollection KeyCounter;
        public ComboCounter ComboCounter;
        public ScoreCounter ScoreCounter;
        public PercentageCounter AccuracyCounter;

        protected abstract KeyCounterCollection CreateKeyCounter();
        protected abstract ComboCounter CreateComboCounter();
        protected abstract PercentageCounter CreateAccuracyCounter();
        protected abstract ScoreCounter CreateScoreCounter();

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
            };
        }
    }
}
