// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLoadingLayer : OsuTestScene
    {
        private TestLoadingLayer overlay;

        private Container content;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Children = new[]
            {
                content = new Container
                {
                    Size = new Vector2(300),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.SlateGray,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.9f),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText { Text = "Sample content" },
                                new RoundedButton { Text = "can't puush me", Width = 200, },
                                new RoundedButton { Text = "puush me", Width = 200, Action = () => { } },
                            }
                        },
                        overlay = new TestLoadingLayer(true),
                    }
                },
            };
        });

        [Test]
        public void TestShowHide()
        {
            AddAssert("not visible", () => !overlay.IsPresent);

            AddStep("show", () => overlay.Show());

            AddUntilStep("wait for content dim", () => overlay.BackgroundDimLayer.Alpha > 0);

            AddStep("hide", () => overlay.Hide());

            AddUntilStep("wait for content restore", () => Precision.AlmostEquals(overlay.BackgroundDimLayer.Alpha, 0));
        }

        [Test]
        public void TestLargeArea()
        {
            AddStep("show", () =>
            {
                content.RelativeSizeAxes = Axes.Both;
                content.Size = new Vector2(1);

                overlay.Show();
            });

            AddStep("hide", () => overlay.Hide());
        }

        private partial class TestLoadingLayer : LoadingLayer
        {
            public new Box BackgroundDimLayer => base.BackgroundDimLayer;

            public TestLoadingLayer(bool dimBackground = false, bool withBox = true)
                : base(dimBackground, withBox)
            {
            }
        }
    }
}
