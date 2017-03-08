// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Taiko.UI;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawables.BarLines
{
    public class DrawableBarLine : Container
    {
        protected Box Tracker;

        protected readonly BarLine BarLine;

        public DrawableBarLine(BarLine barLine)
        {
            BarLine = barLine;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
            Size = new Vector2(1, TaikoPlayfield.PLAYFIELD_BASE_HEIGHT);

            Children = new[]
            {
                Tracker = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Alpha = 0.75f
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LifetimeStart = BarLine.StartTime - BarLine.PreEmpt * 2;
            LifetimeEnd = BarLine.StartTime + BarLine.PreEmpt;

            Delay(BarLine.StartTime);
            FadeOut(100 * BarLine.PreEmpt / 1000);
        }

        protected virtual void MoveToOffset(double time)
        {
            MoveToX((float)((BarLine.StartTime - time) / BarLine.PreEmpt));
        }

        protected override void Update()
        {
            base.Update();

            MoveToOffset(Time.Current);
        }
    }
}
