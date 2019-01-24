// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Match;
using osuTK;

namespace osu.Game.Screens.Multi
{
    [Cached]
    public class Multiplayer : OsuScreen, IOnlineComponent
    {
        private readonly MultiplayerWaveContainer waves;

        public override bool AllowBeatmapRulesetChange => currentSubScreen?.AllowBeatmapRulesetChange ?? base.AllowBeatmapRulesetChange;

        private readonly OsuButton createButton;
        private readonly LoungeSubScreen loungeSubScreen;

        private OsuScreen currentSubScreen;

        [Cached(Type = typeof(IRoomManager))]
        private RoomManager roomManager;

        [Resolved]
        private APIAccess api { get; set; }

        public Multiplayer()
        {
            Child = waves = new MultiplayerWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
            };

            waves.AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"3e3a44"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = OsuColour.FromHex(@"3c3842"),
                            ColourDark = OsuColour.FromHex(@"393540"),
                            TriangleScale = 5,
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT },
                    Child = loungeSubScreen = new LoungeSubScreen(Push),
                },
                new Header(loungeSubScreen),
                createButton = new HeaderButton
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(150, Header.HEIGHT - 20),
                    Margin = new MarginPadding
                    {
                        Top = 10,
                        Right = 10,
                    },
                    Text = "Create room",
                    Action = () => loungeSubScreen.Push(new Room
                    {
                        Name = { Value = $"{api.LocalUser}'s awesome room" }
                    }),
                },
                roomManager = new RoomManager()
            });

            screenAdded(loungeSubScreen);
            loungeSubScreen.Exited += _ => Exit();
        }

        private readonly IBindable<bool> isIdle = new BindableBool();

        [BackgroundDependencyLoader(true)]
        private void load(IdleTracker idleTracker)
        {
            api.Register(this);

            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            isIdle.BindValueChanged(updatePollingRate, true);
        }

        private void updatePollingRate(bool idle)
        {
            roomManager.TimeBetweenPolls = !IsCurrentScreen || !(currentSubScreen is LoungeSubScreen) ? 0 : (idle ? 120000 : 15000);
            Logger.Log($"Polling adjusted to {roomManager.TimeBetweenPolls}");
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            if (state != APIState.Online)
                forcefullyExit();
        }

        private void forcefullyExit()
        {
            // This is temporary since we don't currently have a way to force screens to be exited
            if (IsCurrentScreen)
                Exit();
            else
            {
                MakeCurrent();
                Schedule(forcefullyExit);
            }
        }

        protected override void OnEntering(Screen last)
        {
            Content.FadeIn();

            base.OnEntering(last);
            waves.Show();
        }

        protected override bool OnExiting(Screen next)
        {
            waves.Hide();

            Content.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            cancelLooping();
            loungeSubScreen.MakeCurrent();
            updatePollingRate(isIdle.Value);

            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            Content.FadeIn(250);
            Content.ScaleTo(1, 250, Easing.OutSine);

            updatePollingRate(isIdle.Value);
        }

        protected override void OnSuspending(Screen next)
        {
            Content.ScaleTo(1.1f, 250, Easing.InSine);
            Content.FadeOut(250);

            cancelLooping();
            roomManager.TimeBetweenPolls = 0;

            base.OnSuspending(next);
        }

        private void cancelLooping()
        {
            var track = Beatmap.Value.Track;
            if (track != null)
                track.Looping = false;
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            // the wave overlay transition takes longer than expected to run.
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
            base.LogoExiting(logo);
        }

        protected override void Update()
        {
            base.Update();

            if (!IsCurrentScreen) return;

            if (currentSubScreen is MatchSubScreen)
            {
                var track = Beatmap.Value.Track;
                if (track != null)
                {
                    track.Looping = true;

                    if (!track.IsRunning)
                    {
                        Game.Audio.AddItemToList(track);
                        track.Seek(Beatmap.Value.Metadata.PreviewTime);
                        track.Start();
                    }
                }

                createButton.Hide();
            }
            else if (currentSubScreen is LoungeSubScreen)
                createButton.Show();
        }

        private void screenAdded(Screen newScreen)
        {
            currentSubScreen = (OsuScreen)newScreen;
            updatePollingRate(isIdle.Value);

            newScreen.ModePushed += screenAdded;
            newScreen.Exited += screenRemoved;
        }

        private void screenRemoved(Screen newScreen)
        {
            if (currentSubScreen is MatchSubScreen)
                cancelLooping();

            currentSubScreen = (OsuScreen)newScreen;
            updatePollingRate(isIdle.Value);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            api?.Unregister(this);
        }

        private class MultiplayerWaveContainer : WaveContainer
        {
            protected override bool StartHidden => true;

            public MultiplayerWaveContainer()
            {
                FirstWaveColour = OsuColour.FromHex(@"654d8c");
                SecondWaveColour = OsuColour.FromHex(@"554075");
                ThirdWaveColour = OsuColour.FromHex(@"44325e");
                FourthWaveColour = OsuColour.FromHex(@"392850");
            }
        }
    }
}
