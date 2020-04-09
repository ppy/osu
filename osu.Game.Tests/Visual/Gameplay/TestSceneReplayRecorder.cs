// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneReplayRecorder : OsuManualInputManagerTestScene
    {
        private TestRulesetInputManager playbackManager;
        private TestRulesetInputManager recordingManager;

        private Replay replay;

        private TestReplayRecorder recorder;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            replay = new Replay();

            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        recordingManager = new TestRulesetInputManager(new TestSceneModSettings.TestRulesetInfo(), 0, SimultaneousBindingMode.Unique)
                        {
                            Recorder = recorder = new TestReplayRecorder(replay)
                            {
                                ScreenSpaceToGamefield = pos => recordingManager.ToLocalSpace(pos),
                            },
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
                                    new OsuSpriteText
                                    {
                                        Text = "Recording",
                                        Scale = new Vector2(3),
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    },
                                    new TestInputConsumer()
                                }
                            },
                        }
                    },
                    new Drawable[]
                    {
                        playbackManager = new TestRulesetInputManager(new TestSceneModSettings.TestRulesetInfo(), 0, SimultaneousBindingMode.Unique)
                        {
                            ReplayInputHandler = new TestFramedReplayInputHandler(replay)
                            {
                                GamefieldToScreenSpace = pos => playbackManager.ToScreenSpace(pos),
                            },
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = Color4.DarkBlue,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = "Playback",
                                        Scale = new Vector2(3),
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    },
                                    new TestInputConsumer()
                                }
                            },
                        }
                    }
                }
            });
        });

        [Test]
        public void TestBasic()
        {
            AddStep("move to center", () => InputManager.MoveMouseTo(recordingManager.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("one frame recorded", () => replay.Frames.Count == 1);
            AddAssert("position matches", () => playbackManager.ChildrenOfType<Box>().First().Position == recordingManager.ChildrenOfType<Box>().First().Position);
        }

        [Test]
        public void TestHighFrameRate()
        {
            ScheduledDelegate moveFunction = null;

            AddStep("move to center", () => InputManager.MoveMouseTo(recordingManager.ScreenSpaceDrawQuad.Centre));
            AddStep("much move", () => moveFunction = Scheduler.AddDelayed(() =>
                InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(-1, 0)), 10, true));
            AddWaitStep("move", 10);
            AddStep("stop move", () => moveFunction.Cancel());
            AddAssert("at least 60 frames recorded", () => replay.Frames.Count > 60);
        }

        [Test]
        public void TestLimitedFrameRate()
        {
            ScheduledDelegate moveFunction = null;

            AddStep("lower rate", () => recorder.RecordFrameRate = 2);
            AddStep("move to center", () => InputManager.MoveMouseTo(recordingManager.ScreenSpaceDrawQuad.Centre));
            AddStep("much move", () => moveFunction = Scheduler.AddDelayed(() =>
                InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(-1, 0)), 10, true));
            AddWaitStep("move", 10);
            AddStep("stop move", () => moveFunction.Cancel());
            AddAssert("less than 10 frames recorded", () => replay.Frames.Count < 10);
        }

        [Test]
        public void TestLimitedFrameRateWithImportantFrames()
        {
            ScheduledDelegate moveFunction = null;

            AddStep("lower rate", () => recorder.RecordFrameRate = 2);
            AddStep("move to center", () => InputManager.MoveMouseTo(recordingManager.ScreenSpaceDrawQuad.Centre));
            AddStep("much move with press", () => moveFunction = Scheduler.AddDelayed(() =>
            {
                InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(-1, 0));
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            }, 10, true));
            AddWaitStep("move", 10);
            AddStep("stop move", () => moveFunction.Cancel());
            AddAssert("at least 60 frames recorded", () => replay.Frames.Count > 60);
        }

        protected override void Update()
        {
            base.Update();
            playbackManager?.ReplayInputHandler.SetFrameFromTime(Time.Current - 100);
        }

        public class TestFramedReplayInputHandler : FramedReplayInputHandler<TestReplayFrame>
        {
            public TestFramedReplayInputHandler(Replay replay)
                : base(replay)
            {
            }

            public override List<IInput> GetPendingInputs()
            {
                return new List<IInput>
                {
                    new MousePositionAbsoluteInput
                    {
                        Position = GamefieldToScreenSpace(CurrentFrame?.Position ?? Vector2.Zero)
                    },
                    new ReplayState<TestAction>
                    {
                        PressedActions = CurrentFrame?.Actions ?? new List<TestAction>()
                    }
                };
            }
        }

        public class TestInputConsumer : CompositeDrawable, IKeyBindingHandler<TestAction>
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent.ReceivePositionalInputAt(screenSpacePos);

            private readonly Box box;

            public TestInputConsumer()
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
                Position = e.MousePosition;
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

        public class TestRulesetInputManager : RulesetInputManager<TestAction>
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

        public class TestReplayFrame : ReplayFrame
        {
            public Vector2 Position;

            public List<TestAction> Actions = new List<TestAction>();

            public TestReplayFrame(double time, Vector2 position, params TestAction[] actions)
                : base(time)
            {
                Position = position;
                Actions.AddRange(actions);
            }
        }

        public enum TestAction
        {
            Down,
        }

        internal class TestReplayRecorder : ReplayRecorder<TestAction>
        {
            public TestReplayRecorder(Replay target)
                : base(target)
            {
            }

            protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<TestAction> actions, ReplayFrame previousFrame)
                => new TestReplayFrame(Time.Current, mousePosition, actions.ToArray());
        }
    }
}
