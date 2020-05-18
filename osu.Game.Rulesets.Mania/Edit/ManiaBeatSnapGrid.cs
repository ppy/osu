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
        private const double visible_range = 1500;

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
            foreach (var grid in grids)
            {
                foreach (var line in grid.Objects.OfType<DrawableGridLine>())
                {
                    double lineTime = line.HitObject.StartTime;

                    if (lineTime >= minTime && lineTime <= maxTime)
                        line.Colour = Color4.White;
                    else
                    {
                        double timeSeparation = lineTime < minTime ? minTime - lineTime : lineTime - maxTime;
                        line.Colour = OsuColour.Gray((float)Math.Max(0, 1 - timeSeparation / visible_range));
                    }
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
