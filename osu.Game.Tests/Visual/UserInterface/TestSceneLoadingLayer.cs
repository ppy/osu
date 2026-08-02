// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLoadingLayer : OsuManualInputManagerTestScene
    {
        private TestLoadingLayer overlay;

        private Container content;

        private PressableButton pressableButton;

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
                                pressableButton = new PressableButton { Text = "puush me", Width = 200 },
                            }
                        },
                    }
                },
            };
        });

        [Test]
        public void TestShowHide()
        {
            AddStep("create loading layer", () => content.Add(overlay = new TestLoadingLayer(true)));

            AddAssert("not visible", () => !overlay.IsPresent);

            AddStep("show", () => overlay.Show());
            AddUntilStep("wait for content dim", () => overlay.Alpha > 0);

            AddStep("hide", () => overlay.Hide());
            AddUntilStep("wait for content restore", () => Precision.AlmostEquals(overlay.Alpha, 0));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBlockPositional(bool blockInput)
        {
            AddStep("create loading layer", () => content.Add(overlay = new TestLoadingLayer(true) { BlockPositionalInput = blockInput }));
            AddStep("show", () => overlay.Show());

            AddStep("click button", () =>
            {
                InputManager.MoveMouseTo(pressableButton);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("check pressed", () => pressableButton.Pressed, () => Is.EqualTo(!blockInput));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBlockNonPositional(bool blockKeyboardInput)
        {
            AddStep("create loading layer", () => content.Add(overlay = new TestLoadingLayer(true) { BlockNonPositionalInput = blockKeyboardInput }));
            AddStep("show", () => overlay.Show());

            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddAssert("check pressed", () => pressableButton.Pressed, () => Is.EqualTo(!blockKeyboardInput));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBlockNonPositionalGlobalAction(bool blockKeyboardInput)
        {
            AddStep("create loading layer", () => content.Add(overlay = new TestLoadingLayer(true) { BlockNonPositionalInput = blockKeyboardInput }));
            AddStep("show", () => overlay.Show());

            AddStep("press enter", () => InputManager.Key(Key.F8));

            AddAssert("check pressed", () => pressableButton.Pressed, () => Is.EqualTo(!blockKeyboardInput));
        }

        [Test]
        public void TestLargeArea()
        {
            AddStep("create loading layer", () => content.Add(overlay = new TestLoadingLayer(true)));

            AddStep("show", () =>
            {
                content.RelativeSizeAxes = Axes.Both;
                content.Size = new Vector2(1);

                overlay.Show();
            });

            AddStep("hide", () => overlay.Hide());
        }

        public partial class PressableButton : RoundedButton, IKeyBindingHandler<GlobalAction>
        {
            public PressableButton()
            {
                Action = () => Pressed = true;
            }

            public bool Pressed { get; private set; }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (e.Key == Key.Enter)
                {
                    Pressed = true;
                    return true;
                }

                return base.OnKeyDown(e);
            }

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Action == GlobalAction.ToggleChat)
                {
                    Pressed = true;
                    return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
            }
        }

        private partial class TestLoadingLayer : LoadingLayer
        {
            public TestLoadingLayer(bool dimBackground = false, bool withBox = true)
                : base(dimBackground, withBox)
            {
            }
        }
    }
}
