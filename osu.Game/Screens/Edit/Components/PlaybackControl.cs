// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Components
{
    public partial class PlaybackControl : BottomBarContainer
    {
        private IconButton playButton = null!;
        private Bindable<bool> adjustPitch = new Bindable<bool>(false);

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private readonly Bindable<EditorScreenMode> currentScreenMode = new Bindable<EditorScreenMode>();
        private readonly BindableNumber<double> tempoAdjustment = new BindableDouble(1);

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, Editor? editor)
        {
            Background.Colour = colourProvider.Background4;

            Children = new Drawable[]
            {
                playButton = new IconButton
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Scale = new Vector2(1.2f),
                    IconScale = new Vector2(1.2f),
                    Icon = FontAwesome.Regular.PlayCircle,
                    Action = togglePause,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Left = 45, },
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = EditorStrings.PlaybackSpeed,
                        },
                        new PlaybackTabControl
                        {
                            Current = tempoAdjustment,
                            RelativeSizeAxes = Axes.X,
                            Height = 16,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Margin = new MarginPadding { Right = 5 },
                                    Text = EditorStrings.AdjustPitch,
                                },
                                new SwitchButton
                                {
                                    Current = adjustPitch,
                                },
                            }
                        },
                    }
                }
            };

            Track.BindValueChanged(tr => tr.NewValue?.AddAdjustment(adjustPitch.Value ? AdjustableProperty.Frequency : AdjustableProperty.Tempo, tempoAdjustment), true);

            adjustPitch.BindValueChanged(state =>
            {
                if (Track.Value is var track)
                {
                    if (state.NewValue)
                    {
                        track.RemoveAdjustment(AdjustableProperty.Tempo, tempoAdjustment);
                        track.AddAdjustment(AdjustableProperty.Frequency, tempoAdjustment);
                    }
                    else
                    {
                        track.RemoveAdjustment(AdjustableProperty.Frequency, tempoAdjustment);
                        track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjustment);
                    }
                }
            });

            if (editor != null)
                currentScreenMode.BindTo(editor.Mode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentScreenMode.BindValueChanged(_ => adjustPitch.Value = (currentScreenMode.Value == EditorScreenMode.Timing));
        }

        protected override void Dispose(bool isDisposing)
        {
            Track.Value?.RemoveAdjustment(adjustPitch.Value ? AdjustableProperty.Frequency : AdjustableProperty.Tempo, tempoAdjustment);

            base.Dispose(isDisposing);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return false;

            switch (e.Key)
            {
                case Key.Space:
                    togglePause();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        private void togglePause()
        {
            if (editorClock.IsRunning)
                editorClock.Stop();
            else
                editorClock.Start();
        }

        private static readonly IconUsage play_icon = FontAwesome.Regular.PlayCircle;
        private static readonly IconUsage pause_icon = FontAwesome.Regular.PauseCircle;

        protected override void Update()
        {
            base.Update();

            playButton.Icon = editorClock.IsRunning ? pause_icon : play_icon;
        }

        private partial class PlaybackTabControl : OsuTabControl<double>
        {
            private static readonly double[] tempo_values = { 0.25, 0.5, 0.75, 1 };

            protected override TabItem<double> CreateTabItem(double value) => new PlaybackTabItem(value);

            protected override Dropdown<double> CreateDropdown() => null!;

            public PlaybackTabControl()
            {
                RelativeSizeAxes = Axes.Both;
                TabContainer.Spacing = Vector2.Zero;

                tempo_values.ForEach(AddItem);

                Current.Value = tempo_values.Last();
            }

            public partial class PlaybackTabItem : TabItem<double>
            {
                private const float fade_duration = 200;

                private readonly OsuSpriteText text;
                private readonly OsuSpriteText textBold;

                public PlaybackTabItem(double value)
                    : base(value)
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
                            Font = OsuFont.GetFont(size: 14)
                        },
                        textBold = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Text = $"{value:0%}",
                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                            Alpha = 0,
                        },
                    };
                }

                private Color4 hoveredColour;
                private Color4 normalColour;

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    text.Colour = normalColour = colourProvider.Light3;
                    textBold.Colour = hoveredColour = colourProvider.Content1;
                }

                protected override bool OnHover(HoverEvent e)
                {
                    updateState();
                    return false;
                }

                protected override void OnHoverLost(HoverLostEvent e) => updateState();
                protected override void OnActivated() => updateState();
                protected override void OnDeactivated() => updateState();

                private void updateState()
                {
                    text.FadeColour(Active.Value || IsHovered ? hoveredColour : normalColour, fade_duration, Easing.OutQuint);
                    text.FadeTo(Active.Value ? 0 : 1, fade_duration, Easing.OutQuint);
                    textBold.FadeTo(Active.Value ? 1 : 0, fade_duration, Easing.OutQuint);
                }
            }
        }
    }
}
