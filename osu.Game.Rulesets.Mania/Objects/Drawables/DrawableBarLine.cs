// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="BarLine"/>. Although this derives DrawableManiaHitObject,
    /// this does not handle input/sound like a normal hit object.
    /// </summary>
    public class DrawableBarLine : DrawableManiaHitObject<BarLine>
    {
        public DrawableBarLine(BarLine barLine)
            : base(barLine)
        {
            RelativeSizeAxes = Axes.X;
            Height = barLine.Major ? 1.7f : 1.2f;

            AddInternal(new Box
            {
                Name = "Bar line",
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Alpha = barLine.Major ? 0.5f : 0.2f
            });

            if (barLine.Major)
            {
                Vector2 size = new Vector2(22, 6);
                const float line_offset = 4;

                AddInternal(new Circle
                {
                    Name = "Left line",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,

                    Size = size,
                    X = -line_offset,
                });

                AddInternal(new Circle
                {
                    Name = "Right line",
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    Size = size,
                    X = line_offset,
                });
            }
        }

        protected override void UpdateInitialTransforms()
        {
        }

        protected override void UpdateStartTimeStateTransforms() => this.FadeOut(150);
    }
}
