// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Input
{
    public class TestSceneVirtualInputHandler : OsuTestScene
    {
        private readonly VirtualInputHandler virtualInputHandler;
        private readonly DrawableActions receptor;

        public TestSceneVirtualInputHandler()
        {
            TestInputManager inputManager;
            Add(inputManager = new TestInputManager());

            virtualInputHandler = inputManager.VirtualInputHandler = new VirtualInputHandler<TestAction>();
            receptor = inputManager.DrawableActions;
        }

        [Test]
        public void TestIndividualActions()
        {
            setAction(TestAction.FirstAction, false);
            setAction(TestAction.FirstAction, true);
            setAction(TestAction.SecondAction, false);
            setAction(TestAction.SecondAction, true);
        }

        [Test]
        public void TestAllActions()
        {
            setAction(TestAction.FirstAction, false);
            setAction(TestAction.SecondAction, false);
            setAction(TestAction.FirstAction, true);
            setAction(TestAction.SecondAction, true);
        }

        private void setAction(TestAction action, bool release)
        {
            AddStep($"{(release ? "Release" : "Press")} {action}", () =>
            {
                if (release)
                    virtualInputHandler.Actions.Remove(action);
                else
                    virtualInputHandler.Actions.Add(action);
            });

            AddAssert("Actions synced", () => receptor.PressedActions.SequenceEqual(virtualInputHandler.Actions.Cast<TestAction>()));
        }

        private class TestInputManager : RulesetInputManager<TestAction>
        {
            public readonly DrawableActions DrawableActions;

            public TestInputManager()
                : base(null, 0, SimultaneousBindingMode.Unique)
            {
                KeyBindingContainer.Add(DrawableActions = new DrawableActions());
            }
        }

        private class DrawableActions : FillFlowContainer<DrawableAction>, IKeyBindingHandler<TestAction>
        {
            public readonly List<TestAction> PressedActions = new List<TestAction>();

            public DrawableActions()
            {
                RelativeSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5);
                Children = new[]
                {
                    new DrawableAction { Size = new Vector2(100) },
                    new DrawableAction { Size = new Vector2(100) },
                };
            }

            public bool OnPressed(TestAction action)
            {
                Children[(int)action].Pressed = true;
                PressedActions.Add(action);
                return true;
            }

            public bool OnReleased(TestAction action)
            {
                Children[(int)action].Pressed = false;
                PressedActions.Remove(action);
                return true;
            }
        }

        private class DrawableAction : CompositeDrawable
        {
            private readonly Box background;
            private readonly OsuSpriteText text;

            public bool Pressed
            {
                set
                {
                    background.Colour = value ? Color4.White : Color4.Gray;
                    text.Text = value ? "Pressed" : "Released";
                }
            }

            public DrawableAction()
            {
                CornerRadius = 5;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    background = new Box
                    {
                        Colour = Color4.Gray,
                        RelativeSizeAxes = Axes.Both,
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.Black,
                        Font = OsuFont.GetFont(size: 24, weight: FontWeight.Bold),
                        Text = "Released",
                    }
                };
            }
        }

        private enum TestAction
        {
            FirstAction,
            SecondAction,
        }
    }
}
