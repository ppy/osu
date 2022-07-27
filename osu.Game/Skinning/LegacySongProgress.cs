// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacySongProgress : SongProgress
    {
        private CircularProgress pie;

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(35);

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(0.95f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Child = pie = new CircularProgress
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderColour = Colour4.White,
                    BorderThickness = 2,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0,
                    }
                },
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Colour4.White,
                    Size = new Vector2(3),
                }
            };
        }

        protected override void PopIn()
        {
        }

        protected override void PopOut()
        {
        }

        protected override void UpdateObjects(IEnumerable<HitObject> objects)
        {
        }

        protected override void UpdateProgress(double progress, double time, bool isIntro)
        {
            if (isIntro)
            {
                pie.Scale = new Vector2(-1, 1);
                pie.Anchor = Anchor.TopRight;
                pie.Colour = new Colour4(199, 255, 47, 153);
                pie.Current.Value = 1 - progress;
            }
            else
            {
                pie.Scale = new Vector2(1);
                pie.Anchor = Anchor.TopLeft;
                pie.Colour = new Colour4(255, 255, 255, 153);
                pie.Current.Value = progress;
            }
        }
    }
}
