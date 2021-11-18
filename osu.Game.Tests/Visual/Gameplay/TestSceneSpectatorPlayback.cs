// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Replays;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectatorPlayback : OsuManualInputManagerTestScene
    {
        protected override bool UseOnlineAPI => true;

        private TestRulesetInputManager playbackManager;
        private TestRulesetInputManager recordingManager;

        private Replay replay;

        private readonly IBindableList<int> users = new BindableList<int>();

        private TestReplayRecorder recorder;

        private readonly ManualClock manualClock = new ManualClock();

        private OsuSpriteText latencyDisplay;

        private TestFramedReplayInputHandler replayHandler;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [Cached]
        private GameplayState gameplayState = new GameplayState(new Beatmap(), new OsuRuleset(), Array.Empty<Mod>());

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            replay = new Replay();

            users.BindTo(spectatorClient.PlayingUsers);
            users.BindCollectionChanged((obj, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(args.NewItems != null);

                        foreach (int user in args.NewItems)
                        {
                            if (user == api.LocalUser.Value.Id)
                                spectatorClient.WatchUser(user);
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(args.OldItems != null);

                        foreach (int user in args.OldItems)
                        {
                            if (user == api.LocalUser.Value.Id)
                                spectatorClient.StopWatchingUser(user);
                        }

                        break;
                }
            }, true);

            spectatorClient.OnNewFrames += onNewFrames;

            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        recordingManager = new TestRulesetInputManager(new TestSceneModSettings.TestRulesetInfo(), 0, SimultaneousBindingMode.Unique)
                        {
                            Recorder = recorder = new TestReplayRecorder
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
                                        Text = "Sending",
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
                            Clock = new FramedClock(manualClock),
                            ReplayInputHandler = replayHandler = new TestFramedReplayInputHandler(replay)
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
                                        Text = "Receiving",
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

            Add(latencyDisplay = new OsuSpriteText());
        });

        private void onNewFrames(int userId, FrameDataBundle frames)
        {
            Logger.Log($"Received {frames.Frames.Count()} new frames ({string.Join(',', frames.Frames.Select(f => ((int)f.Time).ToString()))})");

            foreach (var legacyFrame in frames.Frames)
            {
                var frame = new TestReplayFrame();
                frame.FromLegacy(legacyFrame, null);
                replay.Frames.Add(frame);
            }
        }

        [Test]
        public void TestBasic()
        {
        }

        private double latency = SpectatorClient.TIME_BETWEEN_SENDS;

        protected override void Update()
        {
            base.Update();

            if (latencyDisplay == null) return;

            // propagate initial time value
            if (manualClock.CurrentTime == 0)
            {
                manualClock.CurrentTime = Time.Current;
                return;
            }

            if (!replayHandler.HasFrames)
                return;

            var lastFrame = replay.Frames.LastOrDefault();

            // this isn't perfect as we basically can't be aware of the rate-of-send here (the streamer is not sending data when not being moved).
            // in gameplay playback, the case where NextFrame is null would pause gameplay and handle this correctly; it's strictly a test limitation / best effort implementation.
            if (lastFrame != null)
                latency = Math.Max(latency, Time.Current - lastFrame.Time);

            latencyDisplay.Text = $"latency: {latency:N1}";

            double proposedTime = Time.Current - latency + Time.Elapsed;

            // this will either advance by one or zero frames.
            double? time = replayHandler.SetFrameFromTime(proposedTime);

            if (time == null)
                return;

            manualClock.CurrentTime = time.Value;
        }

        [TearDownSteps]
        public void TearDown()
        {
            AddStep("stop recorder", () =>
            {
                recorder.Expire();
                spectatorClient.OnNewFrames -= onNewFrames;
            });
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

            public bool OnPressed(KeyBindingPressEvent<TestAction> e)
            {
                if (e.Repeat)
                    return false;

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

        public class TestReplayFrame : ReplayFrame, IConvertibleReplayFrame
        {
            public Vector2 Position;

            public List<TestAction> Actions = new List<TestAction>();

            public TestReplayFrame(double time, Vector2 position, params TestAction[] actions)
                : base(time)
            {
                Position = position;
                Actions.AddRange(actions);
            }

            public TestReplayFrame()
            {
            }

            public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame lastFrame = null)
            {
                Position = currentFrame.Position;
                Time = currentFrame.Time;
                if (currentFrame.MouseLeft)
                    Actions.Add(TestAction.Down);
            }

            public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
            {
                ReplayButtonState state = ReplayButtonState.None;

                if (Actions.Contains(TestAction.Down))
                    state |= ReplayButtonState.Left1;

                return new LegacyReplayFrame(Time, Position.X, Position.Y, state);
            }
        }

        public enum TestAction
        {
            Down,
        }

        internal class TestReplayRecorder : ReplayRecorder<TestAction>
        {
            public TestReplayRecorder()
                : base(new Score { ScoreInfo = { BeatmapInfo = new BeatmapInfo() } })
            {
            }

            protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<TestAction> actions, ReplayFrame previousFrame)
            {
                return new TestReplayFrame(Time.Current, mousePosition, actions.ToArray());
            }
        }
    }
}
