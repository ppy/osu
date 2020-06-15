// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Statistics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneAccuracyHeatmap : OsuManualInputManagerTestScene
    {
        private readonly Box background;
        private readonly Drawable object1;
        private readonly Drawable object2;
        private readonly Heatmap heatmap;

        public TestSceneAccuracyHeatmap()
        {
            Children = new[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#333"),
                },
                object1 = new BorderCircle
                {
                    Position = new Vector2(256, 192),
                    Colour = Color4.Yellow,
                },
                object2 = new BorderCircle
                {
                    Position = new Vector2(500, 300),
                },
                heatmap = new Heatmap
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scheduler.AddDelayed(() =>
            {
                var randomPos = new Vector2(
                    RNG.NextSingle(object1.DrawPosition.X - object1.DrawSize.X / 2, object1.DrawPosition.X + object1.DrawSize.X / 2),
                    RNG.NextSingle(object1.DrawPosition.Y - object1.DrawSize.Y / 2, object1.DrawPosition.Y + object1.DrawSize.Y / 2));

                // The background is used for ToLocalSpace() since we need to go _inside_ the DrawSizePreservingContainer (Content of TestScene).
                heatmap.AddPoint(object2.Position, object1.Position, randomPos, RNG.NextSingle(10, 500));
                InputManager.MoveMouseTo(background.ToScreenSpace(randomPos));
            }, 1, true);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            heatmap.AddPoint(object2.Position, object1.Position, background.ToLocalSpace(e.ScreenSpaceMouseDownPosition), 50);
            return true;
        }

        private class BorderCircle : CircularContainer
        {
            public BorderCircle()
            {
                Origin = Anchor.Centre;
                Size = new Vector2(100);

                Masking = true;
                BorderThickness = 2;
                BorderColour = Color4.White;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    },
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(4),
                    }
                };
            }
        }
    }
}
