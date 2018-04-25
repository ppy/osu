// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class ScrollableTimeline : CompositeDrawable
    {
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly ScrollingTimelineContainer timelineContainer;

        public ScrollableTimeline()
        {
            Masking = true;
            CornerRadius = 5;

            OsuCheckbox hitObjectsCheckbox;
            OsuCheckbox hitSoundsCheckbox;
            OsuCheckbox waveformCheckbox;
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
                                    Padding = new MarginPadding { Horizontal = 15 },
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 4),
                                    Children = new[]
                                    {
                                        hitObjectsCheckbox = new OsuCheckbox { LabelText = "Hitobjects" },
                                        hitSoundsCheckbox = new OsuCheckbox { LabelText = "Hitsounds" },
                                        waveformCheckbox = new OsuCheckbox { LabelText = "Waveform" }
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
                                new Container<TimelineButton>
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Masking = true,
                                    Children = new[]
                                    {
                                        new TimelineButton
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            Height = 0.5f,
                                            Icon = FontAwesome.fa_search_plus,
                                            Action = () => timelineContainer.Zoom++
                                        },
                                        new TimelineButton
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            RelativeSizeAxes = Axes.Y,
                                            Height = 0.5f,
                                            Icon = FontAwesome.fa_search_minus,
                                            Action = () => timelineContainer.Zoom--
                                        },
                                    }
                                }
                            }
                        },
                        timelineContainer = new ScrollingTimelineContainer { RelativeSizeAxes = Axes.Y }
                    }
                }
            };

            hitObjectsCheckbox.Current.Value = true;
            hitSoundsCheckbox.Current.Value = true;
            waveformCheckbox.Current.Value = true;

            timelineContainer.Beatmap.BindTo(Beatmap);
            timelineContainer.WaveformVisible.BindTo(waveformCheckbox.Current);
        }

        protected override void Update()
        {
            base.Update();

            timelineContainer.Size = new Vector2(DrawSize.X - timelineContainer.DrawPosition.X, 1);
        }
    }
}
