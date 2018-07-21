// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.States;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components
{
    public class PlaybackControl : BottomBarContainer
    {
        private IconButton playButton;

        private IAdjustableClock adjustableClock;

        [BackgroundDependencyLoader]
        private void load(IAdjustableClock adjustableClock)
        {
            this.adjustableClock = adjustableClock;

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
                    Action = togglePause,
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

            tabs.Current.ValueChanged += newValue => Beatmap.Value.Track.Tempo.Value = newValue;
        }

        private void togglePause()
        {
            if (adjustableClock.IsRunning)
                adjustableClock.Stop();
            else
                adjustableClock.Start();
        }

        protected override void Update()
        {
            base.Update();

            playButton.Icon = adjustableClock.IsRunning ? FontAwesome.fa_pause_circle_o : FontAwesome.fa_play_circle_o;
        }

        private class PlaybackTabControl : OsuTabControl<double>
        {
            private static readonly double[] tempo_values = { 0.5, 0.75, 1 };

            protected override TabItem<double> CreateTabItem(double value) => new PlaybackTabItem(value);

            protected override Dropdown<double> CreateDropdown() => null;

            public PlaybackTabControl()
            {
                RelativeSizeAxes = Axes.Both;
                TabContainer.Spacing = Vector2.Zero;

                tempo_values.ForEach(AddItem);
            }

            public class PlaybackTabItem : TabItem<double>
            {
                private const float fade_duration = 200;

                private readonly OsuSpriteText text;
                private readonly OsuSpriteText textBold;

                public PlaybackTabItem(double value) : base(value)
                {
                    RelativeSizeAxes = Axes.Both;

                    Width = 1f / tempo_values.Length;

                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Text = $"{value:0%}",
                            TextSize = 14,
                        },
                        textBold = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Text = $"{value:0%}",
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                            Alpha = 0,
                        },
                    };
                }

                private Color4 hoveredColour;
                private Color4 normalColour;

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    text.Colour = normalColour = colours.YellowDarker;
                    textBold.Colour = hoveredColour = colours.Yellow;
                }

                protected override bool OnHover(InputState state)
                {
                    updateState();
                    return true;
                }

                protected override void OnHoverLost(InputState state) => updateState();
                protected override void OnActivated() => updateState();
                protected override void OnDeactivated() => updateState();

                private void updateState()
                {
                    text.FadeColour(Active || IsHovered ? hoveredColour : normalColour, fade_duration, Easing.OutQuint);
                    text.FadeTo(Active ? 0 : 1, fade_duration, Easing.OutQuint);
                    textBold.FadeTo(Active ? 1 : 0, fade_duration, Easing.OutQuint);
                }
            }
        }
    }
}
