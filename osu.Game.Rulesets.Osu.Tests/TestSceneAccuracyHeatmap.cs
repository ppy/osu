// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Scoring;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneAccuracyHeatmap : OsuManualInputManagerTestScene
    {
        private Box background;
        private Drawable object1;
        private Drawable object2;
        private TestAccuracyHeatmap accuracyHeatmap;
        private ScheduledDelegate automaticAdditionDelegate;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            automaticAdditionDelegate?.Cancel();
            automaticAdditionDelegate = null;

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
                    Position = new Vector2(100, 300),
                },
                accuracyHeatmap = new TestAccuracyHeatmap(new ScoreInfo { BeatmapInfo = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(300)
                }
            };
        });

        [Test]
        public void TestManyHitPointsAutomatic()
        {
            AddStep("add scheduled delegate", () =>
            {
                automaticAdditionDelegate = Scheduler.AddDelayed(() =>
                {
                    var randomPos = new Vector2(
                        RNG.NextSingle(object1.DrawPosition.X - object1.DrawSize.X / 2, object1.DrawPosition.X + object1.DrawSize.X / 2),
                        RNG.NextSingle(object1.DrawPosition.Y - object1.DrawSize.Y / 2, object1.DrawPosition.Y + object1.DrawSize.Y / 2));

                    // The background is used for ToLocalSpace() since we need to go _inside_ the DrawSizePreservingContainer (Content of TestScene).
                    accuracyHeatmap.AddPoint(object2.Position, object1.Position, randomPos, RNG.NextSingle(10, 500));
                    InputManager.MoveMouseTo(background.ToScreenSpace(randomPos));
                }, 1, true);
            });

            AddWaitStep("wait for some hit points", 10);
        }

        [Test]
        public void TestManualPlacement()
        {
            AddStep("return user input", () => InputManager.UseParentInput = true);
        }

        [Test]
        public void TestAllPoints()
        {
            AddStep("add points", () =>
            {
                float minX = object1.DrawPosition.X - object1.DrawSize.X / 2;
                float maxX = object1.DrawPosition.X + object1.DrawSize.X / 2;

                float minY = object1.DrawPosition.Y - object1.DrawSize.Y / 2;
                float maxY = object1.DrawPosition.Y + object1.DrawSize.Y / 2;

                for (int i = 0; i < 10; i++)
                {
                    for (float x = minX; x <= maxX; x += 0.5f)
                    {
                        for (float y = minY; y <= maxY; y += 0.5f)
                        {
                            accuracyHeatmap.AddPoint(object2.Position, object1.Position, new Vector2(x, y), RNG.NextSingle(10, 500));
                        }
                    }
                }
            });
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            accuracyHeatmap.AddPoint(object2.Position, object1.Position, background.ToLocalSpace(e.ScreenSpaceMouseDownPosition), 50);
            return true;
        }

        private partial class TestAccuracyHeatmap : AccuracyHeatmap
        {
            public TestAccuracyHeatmap(ScoreInfo score)
                : base(score, new TestBeatmap(new OsuRuleset().RulesetInfo))
            {
            }

            public new void AddPoint(Vector2 start, Vector2 end, Vector2 hitPoint, float radius)
                => base.AddPoint(start, end, hitPoint, radius);
        }

        private partial class BorderCircle : CircularContainer
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
