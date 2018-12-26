// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
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
    public class Multiplayer : OsuScreen
    {
        private readonly MultiplayerWaveContainer waves;

        public override bool AllowBeatmapRulesetChange => currentScreen?.AllowBeatmapRulesetChange ?? base.AllowBeatmapRulesetChange;

        private readonly OsuButton createButton;
        private readonly LoungeSubScreen loungeSubScreen;

        private OsuScreen currentScreen;

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

            var track = Beatmap.Value.Track;
            if (track != null)
                track.Looping = false;

            loungeSubScreen.MakeCurrent();

            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            Content.FadeIn(250);
            Content.ScaleTo(1, 250, Easing.OutSine);
        }

        protected override void OnSuspending(Screen next)
        {
            Content.ScaleTo(1.1f, 250, Easing.InSine);
            Content.FadeOut(250);

            base.OnSuspending(next);
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

            if (currentScreen is MatchSubScreen)
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
            else if (currentScreen is LoungeSubScreen)
                createButton.Show();
        }

        private void screenAdded(Screen newScreen)
        {
            currentScreen = (OsuScreen)newScreen;

            newScreen.ModePushed += screenAdded;
            newScreen.Exited += screenRemoved;
        }

        private void screenRemoved(Screen newScreen)
        {
            if (currentScreen is MatchSubScreen)
            {
                var track = Beatmap.Value.Track;
                if (track != null)
                    track.Looping = false;
            }

            currentScreen = (OsuScreen)newScreen;
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
