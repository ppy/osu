// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary
{
    /// <summary>
    /// The timeline that sits at the bottom of the editor.
    /// </summary>
    public class SummaryTimeline : BottomBarContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IAdjustableClock adjustableClock)
        {
            TimelinePart markerPart, controlPointPart, bookmarkPart, breakPart;

            Children = new Drawable[]
            {
                markerPart = new MarkerPart(adjustableClock) { RelativeSizeAxes = Axes.Both },
                controlPointPart = new ControlPointPart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.35f
                },
                bookmarkPart = new BookmarkPart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.35f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray5,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(5)
                        },
                        new Box
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 1,
                            EdgeSmoothness = new Vector2(0, 1),
                        },
                        new Circle
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(5)
                        },
                    }
                },
                breakPart = new BreakPart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.25f
                }
            };

            markerPart.Beatmap.BindTo(Beatmap);
            controlPointPart.Beatmap.BindTo(Beatmap);
            bookmarkPart.Beatmap.BindTo(Beatmap);
            breakPart.Beatmap.BindTo(Beatmap);
        }
    }
}
