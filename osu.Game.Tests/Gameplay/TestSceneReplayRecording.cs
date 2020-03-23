// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Gameplay
{
    public class TestSceneReplayRecording : OsuTestScene
    {
        public TestSceneReplayRecording()
        {
            Add(new TestRulesetInputManager(new TestSceneModSettings.TestRulesetInfo(), 0, SimultaneousBindingMode.Unique)
            {
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Brown,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new TestConsumer()
                    }
                },
            });
        }

        public class TestConsumer : CompositeDrawable, IKeyBindingHandler<TestAction>
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            private readonly Box box;

            public TestConsumer()
            {
                Size = new Vector2(30);

                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    box = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                this.Position = e.MousePosition;
                return base.OnMouseMove(e);
            }

            public bool OnPressed(TestAction action)
            {
                box.Colour = Color4.White;
                return true;
            }

            public void OnReleased(TestAction action)
            {
                box.Colour = Color4.Black;
            }
        }

        private class TestRulesetInputManager : RulesetInputManager<TestAction>
        {
            public TestRulesetInputManager(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override KeyBindingContainer<TestAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                => new TestKeyBindingContainer();

            internal class TestKeyBindingContainer : KeyBindingContainer<TestAction>
            {
                public override IEnumerable<KeyBinding> DefaultKeyBindings => new[]
                {
                    new KeyBinding(InputKey.MouseLeft, TestAction.Down),
                };
            }
        }

        public enum TestAction
        {
            Down,
        }
    }
}
