// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components
{
    public class PlaybackContainer : BottomBarContainer
    {
        private readonly IconButton playButton;

        public PlaybackContainer()
        {
            PlaybackTabControl tabs;

            Children = new Drawable[]
            {
                playButton = new IconButton
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1.4f),
                    IconScale = new Vector2(1.4f),
                    Icon = FontAwesome.fa_play_circle_o,
                    Action = playPause,
                    Padding = new MarginPadding { Left = 20 }
                },
                new OsuSpriteText
                {
                    Origin = Anchor.BottomLeft,
                    Text = "Playback Speed",
                    RelativePositionAxes = Axes.Y,
                    Y = 0.5f,
                    Padding = new MarginPadding { Left = 45 }
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Padding = new MarginPadding { Left = 45 },
                    Child = tabs = new PlaybackTabControl(),
                }
            };

            tabs.AddItem(0.25);
            tabs.AddItem(0.75);
            tabs.AddItem(1);

            tabs.Current.ValueChanged += newValue => Track.Tempo.Value = newValue;
        }

        private void playPause()
        {
            if (Track.IsRunning)
                Track.Stop();
            else
                Track.Start();
        }

        protected override void Update()
        {
            base.Update();

            playButton.Icon = Track.IsRunning ? FontAwesome.fa_pause_circle_o : FontAwesome.fa_play_circle_o;
        }

        private class PlaybackTabControl : OsuTabControl<double>
        {
            protected override TabItem<double> CreateTabItem(double value) => new PlaybackTabItem(value);

            protected override Dropdown<double> CreateDropdown() => null;

            public PlaybackTabControl()
            {
                RelativeSizeAxes = Axes.Both;
                TabContainer.Spacing = new Vector2(20, 0);
            }

            public class PlaybackTabItem : TabItem<double>
            {
                private const float fade_duration = 100;

                private readonly OsuSpriteText text;
                private readonly OsuSpriteText textBold;

                public PlaybackTabItem(double value) : base(value)
                {
                    AutoSizeAxes = Axes.X;
                    RelativeSizeAxes = Axes.Y;

                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Text = $"{value:P0}",
                            TextSize = 14,
                        },
                        textBold = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Text = $"{value:P0}",
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                    };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    text.Colour = colours.Gray5;
                }

                protected override bool OnHover(InputState state)
                {
                    if (!Active)
                        toBold();
                    return true;
                }

                protected override void OnHoverLost(InputState state)
                {
                    if (!Active)
                        toNormal();
                }

                private void toBold()
                {
                    text.FadeOut(fade_duration);
                    textBold.FadeIn(fade_duration);
                }

                private void toNormal()
                {
                    text.FadeIn(fade_duration);
                    textBold.FadeOut(fade_duration);
                }

                protected override void OnActivated() => toBold();

                protected override void OnDeactivated() => toNormal();
            }
        }
    }
}
