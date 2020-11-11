// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DropletPiece : CompositeDrawable
    {
        public DropletPiece()
        {
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS / 2);
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            DrawableCatchHitObject drawableCatchObject = (DrawableCatchHitObject)drawableObject;
            var hitObject = drawableCatchObject.HitObject;

            InternalChild = new Pulp
            {
                RelativeSizeAxes = Axes.Both,
                AccentColour = { BindTarget = drawableObject.AccentColour }
            };

            if (hitObject.HyperDash)
            {
                AddInternal(new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(2f),
                    Depth = 1,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            BorderColour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                            BorderThickness = 6,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    AlwaysPresent = true,
                                    Alpha = 0.3f,
                                    Blending = BlendingParameters.Additive,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
