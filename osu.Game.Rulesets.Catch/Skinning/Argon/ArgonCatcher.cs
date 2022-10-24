// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Catch.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    public class ArgonCatcher : CompositeDrawable
    {
        [Resolved]
        private Bindable<CatcherAnimationState> currentState { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

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
                            Colour = Color4.White,
                        },
                        new Box
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreRight,
                            Colour = Color4.White,
                            Alpha = 0.25f,
                            RelativeSizeAxes = Axes.X,
                            X = -2,
                            Width = 20,
                            Height = 1.8f,
                        },
                        new Circle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreRight,
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.X,
                            X = -2,
                            Width = 15 / 170f,
                            Height = 4,
                        },
                        new Box
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreLeft,
                            Colour = Color4.White,
                            Alpha = 0.25f,
                            RelativeSizeAxes = Axes.X,
                            X = 2,
                            Width = 20,
                            Height = 1.8f,
                        },
                        new Circle
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreLeft,
                            Colour = Color4.White,
                            X = 2,
                            RelativeSizeAxes = Axes.X,
                            Width = 15 / 170f,
                            Height = 4,
                        },
                    }
                },
            };
        }
    }
}
