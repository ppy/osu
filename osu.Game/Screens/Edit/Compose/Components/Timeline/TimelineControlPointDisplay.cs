using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineControlPointDisplay : TimelinePart
    {
        protected override void LoadBeatmap(WorkingBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            ControlPointInfo cpi = beatmap.Beatmap.ControlPointInfo;

            cpi.TimingPoints.ForEach(addTimingPoint);

            // Consider all non-timing points as the same type
            cpi.SamplePoints.Select(c => (ControlPoint)c)
               .Concat(cpi.EffectPoints)
               .Concat(cpi.DifficultyPoints)
               .Distinct()
               // Non-timing points should not be added where there are timing points
               .Where(c => cpi.TimingPointAt(c.Time).Time != c.Time)
               .ForEach(addNonTimingPoint);
        }

        private void addTimingPoint(ControlPoint controlPoint) => Add(new TimingPointVisualisation(controlPoint));
        private void addNonTimingPoint(ControlPoint controlPoint) => Add(new NonTimingPointVisualisation(controlPoint));

        public TimelineControlPointDisplay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            //Bind to changes to control points
        }

        private class TimingPointVisualisation : ControlPointVisualisation
        {
            public TimingPointVisualisation(ControlPoint controlPoint)
                : base(controlPoint)
            {
                Anchor = Anchor.BottomLeft;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.YellowDark;
        }

        private class NonTimingPointVisualisation : ControlPointVisualisation
        {
            public NonTimingPointVisualisation(ControlPoint controlPoint)
                : base(controlPoint)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Green;
        }

        private abstract class ControlPointVisualisation : PointVisualisation
        {
            protected ControlPointVisualisation(ControlPoint controlPoint)
                : base(controlPoint.Time)
            {
                Origin = Anchor.Centre;
            }
        }
    }
}
