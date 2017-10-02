// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Menus;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using OpenTK;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Edit
{
    internal class Editor : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg4");

        internal override bool ShowOverlays => false;

        private readonly Box bottomBackground;

        public Editor()
        {
            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 40,
                Children = new Drawable[]
                {
                    new EditorMenuBar
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            });

            SummaryTimeline summaryTimeline;
            Add(new Container
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = 60,
                Children = new Drawable[]
                {
                    bottomBackground = new Box { RelativeSizeAxes = Axes.Both },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 5, Bottom = 5, Left = 10, Right = 10 },
                        Child = new FillFlowContainer
                        {
                            Name = "Bottom bar",
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10, 0),
                            Children = new[]
                            {
                                summaryTimeline = new SummaryTimeline
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.65f
                                }
                            }
                        }
                    }
                }
            });

            summaryTimeline.Beatmap.BindTo(Beatmap);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bottomBackground.Colour = colours.Gray2;
        }

        protected override void OnResuming(Screen last)
        {
            Beatmap.Value.Track?.Stop();
            base.OnResuming(last);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Background.FadeColour(Color4.DarkGray, 500);
            Beatmap.Value.Track?.Stop();
        }

        protected override bool OnExiting(Screen next)
        {
            Background.FadeColour(Color4.White, 500);
            Beatmap.Value.Track?.Start();
            return base.OnExiting(next);
        }
    }
}
