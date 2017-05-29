// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="Barline"/>. Although this derives DrawableManiaHitObject,
    /// this does not handle input/sound like a normal hit object.
    /// </summary>
    public class DrawableBarline : DrawableManiaHitObject<Barline>
    {
        public DrawableBarline(Barline hitObject)
            : base(hitObject, null)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Add(new Box
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Height = 1
            });

            int signatureRelativeIndex = hitObject.BeatIndex % (int)hitObject.ControlPoint.TimeSignature;

            switch (signatureRelativeIndex)
            {
                case 0:
                    Add(new EquilateralTriangle
                    {
                        Name = "Left triangle",
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(12),
                        X = -9,
                        Rotation = 90,
                        BypassAutoSizeAxes = Axes.Both
                    });

                    Add(new EquilateralTriangle
                    {
                        Name = "Right triangle",
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(12),
                        X = 9,
                        Rotation = -90,
                        BypassAutoSizeAxes = Axes.Both,
                    });
                    break;
                case 1:
                case 3:
                    Alpha = 0.2f;
                    break;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}