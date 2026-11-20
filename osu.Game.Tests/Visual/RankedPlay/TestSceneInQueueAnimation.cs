// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneInQueueAnimation : MultiplayerTestScene
    {
        private readonly Bindable<ScreenQueue.MatchmakingScreenState> currentState = new Bindable<ScreenQueue.MatchmakingScreenState>();

        private Sample? enqueueSample;

        private SampleChannel? waitingLoopChannel;
        private ScheduledDelegate? startLoopPlaybackDelegate;
        private DrawableSample waitingLoop = null!;

        [Resolved]
        private TextureStore textures { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create", () =>
            {
                enqueueSample = audio.Samples.Get(@"Multiplayer/Matchmaking/enqueue");

                currentState.BindTo(QueueController.CurrentState);
                currentState.BindValueChanged(e =>
                {
                    startLoopPlaybackDelegate?.Cancel();
                    stopWaitingLoopPlayback();

                    if (e.NewValue == ScreenQueue.MatchmakingScreenState.Queueing)
                    {
                        enqueueSample?.Play();
                        startLoopPlaybackDelegate = Scheduler.AddDelayed(startWaitingLoopPlayback, 2000);
                    }
                });

                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        waitingLoop = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/waiting-loop")),
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get("Backgrounds/bg1"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                            Colour = colourProvider.Dark2
                        },
                        new ScreenQueue.InQueueAnimation
                        {
                            State = { Value = Visibility.Visible },
                        },
                        new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(100),
                            Masking = true,
                            Child = new ClickableAvatar
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("set queue state to idle", () => QueueController.CurrentState.Value = ScreenQueue.MatchmakingScreenState.Idle);
            AddStep("set queue state to queueing", () => QueueController.CurrentState.Value = ScreenQueue.MatchmakingScreenState.Queueing);
        }

        private void startWaitingLoopPlayback()
        {
            stopWaitingLoopPlayback();

            waitingLoopChannel = waitingLoop.GetChannel();
            if (waitingLoopChannel == null)
                return;

            waitingLoopChannel.Looping = true;
            waitingLoopChannel?.Play();

            waitingLoop.VolumeTo(1)
                       .Delay(2000)
                       .VolumeTo(0, 12000);
        }

        private void stopWaitingLoopPlayback()
        {
            waitingLoopChannel?.Stop();
            waitingLoopChannel?.Dispose();
        }
    }
}
