// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneReplayRecording : OsuTestScene
    {
        private readonly TestRulesetInputManager playbackManager;

        private readonly TestRulesetInputManager recordingManager;

        [Cached]
        private GameplayState gameplayState = new GameplayState(new Beatmap(), new OsuRuleset(), Array.Empty<Mod>());

        public TestSceneReplayRecording()
        {
            Replay replay = new Replay();

            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        recordingManager = new TestRulesetInputManager(new TestSceneModSettings.TestRulesetInfo(), 0, SimultaneousBindingMode.Unique)
                        {
                            Recorder = new TestReplayRecorder(new Score
                            {
                                Replay = replay,
                                ScoreInfo = { BeatmapInfo = gameplayState.Beatmap.BeatmapInfo }
                            })
                            {
                                ScreenSpaceToGamefield = pos => recordingManager?.ToLocalSpace(pos) ?? Vector2.Zero,
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
                                    new TestConsumer()
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
                                GamefieldToScreenSpace = pos => playbackManager?.ToScreenSpace(pos) ?? Vector2.Zero,
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
                                    new TestConsumer()
                                }
                            },
                        }
                    }
                }
            });
        }

        protected override void Update()
        {
            base.Update();

            playbackManager.ReplayInputHandler.SetFrameFromTime(Time.Current - 500);
        }
    }

    public class TestFramedReplayInputHandler : FramedReplayInputHandler<TestReplayFrame>
    {
        public TestFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            inputs.Add(new MousePositionAbsoluteInput { Position = GamefieldToScreenSpace(CurrentFrame?.Position ?? Vector2.Zero) });
            inputs.Add(new ReplayState<TestAction> { PressedActions = CurrentFrame?.Actions ?? new List<TestAction>() });
        }
    }

    public class TestConsumer : CompositeDrawable, IKeyBindingHandler<TestAction>
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent.ReceivePositionalInputAt(screenSpacePos);

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
            Position = e.MousePosition;
            return base.OnMouseMove(e);
        }

        public bool OnPressed(KeyBindingPressEvent<TestAction> e)
        {
            box.Colour = Color4.White;
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<TestAction> e)
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
            public override IEnumerable<IKeyBinding> DefaultKeyBindings => new[]
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
        public TestReplayRecorder(Score target)
            : base(target)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<TestAction> actions, ReplayFrame previousFrame) =>
            new TestReplayFrame(Time.Current, mousePosition, actions.ToArray());
    }
}
