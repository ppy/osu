// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineTickDisplay : TimelinePart
    {
        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        public TimelineTickDisplay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatDivisor.BindValueChanged(_ => createLines(), true);
        }

        private void createLines()
        {
            Clear();

            for (var i = 0; i < beatmap.ControlPointInfo.TimingPoints.Count; i++)
            {
                var point = beatmap.ControlPointInfo.TimingPoints[i];
                var until = i + 1 < beatmap.ControlPointInfo.TimingPoints.Count ? beatmap.ControlPointInfo.TimingPoints[i + 1].Time : working.Value.Track.Length;

                int beat = 0;

                for (double t = point.Time; t < until; t += point.BeatLength / beatDivisor.Value)
                {
                    var indexInBeat = beat % beatDivisor.Value;

                    if (indexInBeat == 0)
                    {
                        Add(new PointVisualisation(t)
                        {
                            Colour = BindableBeatDivisor.GetColourFor(1, colours),
                            Origin = Anchor.TopCentre,
                        });
                    }
                    else
                    {
                        var divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beat, beatDivisor.Value);
                        var colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                        var height = 0.1f - (float)divisor / BindableBeatDivisor.VALID_DIVISORS.Last() * 0.08f;

                        Add(new PointVisualisation(t)
                        {
                            Colour = colour,
                            Height = height,
                            Origin = Anchor.TopCentre,
                        });

                        Add(new PointVisualisation(t)
                        {
                            Colour = colour,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomCentre,
                            Height = height,
                        });
                    }

                    beat++;
                }
            }
        }
    }
}
