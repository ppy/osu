// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaBeatSnapGrid : Component
    {
        /// <summary>
        /// The brightness of bar lines one beat around the time range from <see cref="SetRange"/>.
        /// </summary>
        private const float first_beat_brightness = 0.5f;

        [Resolved]
        private IManiaHitObjectComposer composer { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        private readonly List<ScrollingHitObjectContainer> grids = new List<ScrollingHitObjectContainer>();

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var stage in composer.Playfield.Stages)
            {
                foreach (var column in stage.Columns)
                {
                    var lineContainer = new ScrollingHitObjectContainer();

                    grids.Add(lineContainer);
                    column.UnderlayElements.Add(lineContainer);
                }
            }

            beatDivisor.BindValueChanged(_ => createLines(), true);
        }

        public override void Hide()
        {
            base.Hide();
            foreach (var grid in grids)
                grid.Hide();
        }

        public override void Show()
        {
            base.Show();
            foreach (var grid in grids)
                grid.Show();
        }

        private void createLines()
        {
            foreach (var grid in grids)
                grid.Clear();

            for (int i = 0; i < beatmap.ControlPointInfo.TimingPoints.Count; i++)
            {
                var point = beatmap.ControlPointInfo.TimingPoints[i];
                var until = i + 1 < beatmap.ControlPointInfo.TimingPoints.Count ? beatmap.ControlPointInfo.TimingPoints[i + 1].Time : working.Value.Track.Length;

                int beat = 0;

                for (double t = point.Time; t < until; t += point.BeatLength / beatDivisor.Value)
                {
                    var indexInBeat = beat % beatDivisor.Value;
                    Color4 colour;

                    if (indexInBeat == 0)
                        colour = BindableBeatDivisor.GetColourFor(1, colours);
                    else
                    {
                        var divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beat, beatDivisor.Value);
                        colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                    }

                    foreach (var grid in grids)
                        grid.Add(new DrawableGridLine(t, colour));

                    beat++;
                }
            }
        }

        public (Vector2 position, double time)? GetSnappedPosition(Vector2 position)
        {
            float minDist = float.PositiveInfinity;
            DrawableGridLine minDistLine = null;
            Vector2 minDistLinePosition = Vector2.Zero;

            foreach (var grid in grids)
            {
                foreach (var line in grid.AliveObjects.OfType<DrawableGridLine>())
                {
                    Vector2 linePos = line.ToSpaceOfOtherDrawable(line.OriginPosition, this);
                    float d = Vector2.Distance(position, linePos);

                    if (d < minDist)
                    {
                        minDist = d;
                        minDistLine = line;
                        minDistLinePosition = linePos;
                    }
                }
            }

            if (minDistLine == null)
                return null;

            float noteOffset = (scrollingInfo.Direction.Value == ScrollingDirection.Up ? 1 : -1) * DefaultNotePiece.NOTE_HEIGHT / 2;
            return (new Vector2(position.X, minDistLinePosition.Y + noteOffset), minDistLine.HitObject.StartTime);
        }

        public void SetRange(double minTime, double maxTime)
        {
            if (LoadState >= LoadState.Ready)
                setRange(minTime, maxTime);
            else
                Schedule(() => setRange(minTime, maxTime));
        }

        private void setRange(double minTime, double maxTime)
        {
            var linesBefore = new List<DrawableGridLine>();
            var linesDuring = new List<DrawableGridLine>();
            var linesAfter = new List<DrawableGridLine>();

            foreach (var grid in grids)
            {
                linesBefore.Clear();
                linesDuring.Clear();
                linesAfter.Clear();

                foreach (var line in grid.Objects.OfType<DrawableGridLine>())
                {
                    if (line.HitObject.StartTime < minTime)
                        linesBefore.Add(line);
                    else if (line.HitObject.StartTime <= maxTime)
                        linesDuring.Add(line);
                    else
                        linesAfter.Add(line);
                }

                // Snapping will always happen on one of the two lines around minTime (the "target" line).
                // One of those lines may exist in linesBefore and the other may exist in linesAfter, depending on whether such a line exists, and the target changes when the mid-point is crossed.
                // For display purposes, one complete beat is shown at the maximum brightness such that the target line should always be bright.
                bool targetLineIsLastLineBefore = false;

                if (linesBefore.Count > 0 && linesAfter.Count > 0)
                    targetLineIsLastLineBefore = Math.Abs(linesBefore[^1].HitObject.StartTime - minTime) <= Math.Abs(linesAfter[0].HitObject.StartTime - minTime);
                else if (linesBefore.Count > 0)
                    targetLineIsLastLineBefore = true;

                if (targetLineIsLastLineBefore)
                {
                    // Move the last line before to linesDuring
                    linesDuring.Insert(0, linesBefore[^1]);
                    linesBefore.RemoveAt(linesBefore.Count - 1);
                }
                else if (linesAfter.Count > 0) // = false does not guarantee that a line after exists (maybe at the bottom of the screen)
                {
                    // Move the first line after to linesDuring
                    linesDuring.Insert(0, linesAfter[0]);
                    linesAfter.RemoveAt(0);
                }

                // Grays are used rather than transparency since the lines appear on a coloured mania playfield.

                foreach (var l in linesDuring)
                    l.Colour = OsuColour.Gray(first_beat_brightness);

                for (int i = 0; i < linesBefore.Count; i++)
                {
                    int offset = (linesBefore.Count - i - 1) / beatDivisor.Value;
                    linesBefore[i].Colour = OsuColour.Gray(first_beat_brightness / (offset + 1));
                }

                for (int i = 0; i < linesAfter.Count; i++)
                {
                    int offset = i / beatDivisor.Value;
                    linesAfter[i].Colour = OsuColour.Gray(first_beat_brightness / (offset + 1));
                }
            }
        }

        private class DrawableGridLine : DrawableHitObject
        {
            [Resolved]
            private IScrollingInfo scrollingInfo { get; set; }

            private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

            public DrawableGridLine(double startTime, Color4 colour)
                : base(new HitObject { StartTime = startTime })
            {
                RelativeSizeAxes = Axes.X;
                Height = 2;

                AddInternal(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour
                });
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                direction.BindTo(scrollingInfo.Direction);
                direction.BindValueChanged(onDirectionChanged, true);
            }

            private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
            {
                Origin = Anchor = direction.NewValue == ScrollingDirection.Up
                    ? Anchor.TopLeft
                    : Anchor.BottomLeft;
            }

            protected override void UpdateStateTransforms(ArmedState state)
            {
                using (BeginAbsoluteSequence(HitObject.StartTime + 1000))
                    this.FadeOut();
            }
        }
    }
}
