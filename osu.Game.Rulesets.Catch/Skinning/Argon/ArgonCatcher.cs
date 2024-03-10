// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Catch.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    public partial class ArgonCatcher : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 10,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.White,
                            Width = Catcher.ALLOWED_CATCH_RANGE,
                        },
                        new Box
                        {
                            Name = "long line left",
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreRight,
                            Colour = Color4.White,
                            Alpha = 0.25f,
                            RelativeSizeAxes = Axes.X,
                            Width = 20,
                            Height = 1.8f,
                        },
                        new Circle
                        {
                            Name = "bumper left",
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.X,
                            Width = (1 - Catcher.ALLOWED_CATCH_RANGE) / 2,
                            Height = 4,
                        },
                        new Box
                        {
                            Name = "long line right",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreLeft,
                            Colour = Color4.White,
                            Alpha = 0.25f,
                            RelativeSizeAxes = Axes.X,
                            Width = 20,
                            Height = 1.8f,
                        },
                        new Circle
                        {
                            Name = "bumper right",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.X,
                            Width = (1 - Catcher.ALLOWED_CATCH_RANGE) / 2,
                            Height = 4,
                        },
                    }
                },
            };
        }
    }
}
