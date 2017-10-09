// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class ScrollableTimeline : CompositeDrawable
    {
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly Container timelineContainer;
        private readonly BeatmapWaveformGraph waveform;

        public ScrollableTimeline()
        {
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("111")
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex("222")
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 160,
                                    Padding = new MarginPadding { Horizontal = 25 },
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 4),
                                    Children = new[]
                                    {
                                        new OsuCheckbox { LabelText = "Hit Objects" },
                                        new OsuCheckbox { LabelText = "Hit Sounds" },
                                        new OsuCheckbox { LabelText = "Waveform" }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex("333")
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Horizontal = 15 },
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 30),
                                    Children = new[]
                                    {
                                        new SpriteIcon
                                        {
                                            Size = new Vector2(18),
                                            Icon = FontAwesome.fa_search_plus,
                                            Colour = OsuColour.FromHex("555")
                                        },
                                        new SpriteIcon
                                        {
                                            Size = new Vector2(18),
                                            Icon = FontAwesome.fa_search_minus,
                                            Colour = OsuColour.FromHex("555")
                                        },
                                    }
                                }
                            }
                        },
                        timelineContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                waveform = new BeatmapWaveformGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex("081a84")
                                    // Resolution = 0.33f,
                                }
                            }
                        }
                    }
                }
            };

            waveform.Beatmap.BindTo(Beatmap);
        }

        protected override bool OnWheel(InputState state)
        {
            if (!state.Keyboard.ControlPressed)
                return false;

            waveform.ScaleTo(new Vector2(MathHelper.Clamp(waveform.Scale.X + state.Mouse.WheelDelta, 1, 30), 1), 150, Easing.OutQuint);
            return true;
        }

        protected override void Update()
        {
            base.Update();

            timelineContainer.Size = new Vector2(DrawSize.X - timelineContainer.DrawPosition.X, 1);
        }
    }
}
