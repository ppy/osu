// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaBeatSnapGrid : CompositeDrawable
    {
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

        private readonly List<Grid> grids = new List<Grid>();

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var stage in composer.Playfield.Stages)
            {
                var grid = new Grid(stage);
                grids.Add(grid);

                AddInternal(grid);
            }

            beatDivisor.BindValueChanged(_ => createLines(), true);
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

        public void SetRange(double minTime, double maxTime) => Schedule(() =>
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

                foreach (var l in linesDuring)
                    l.Colour = OsuColour.Gray(0.5f);

                for (int i = 0; i < linesBefore.Count; i++)
                {
                    int offset = (linesBefore.Count - i - 1) / beatDivisor.Value;
                    linesBefore[i].Colour = OsuColour.Gray(0.5f / (offset + 1));
                }

                for (int i = 0; i < linesAfter.Count; i++)
                {
                    int offset = i / beatDivisor.Value;
                    linesAfter[i].Colour = OsuColour.Gray(0.5f / (offset + 1));
                }
            }
        });

        private class Grid : ScrollingHitObjectContainer
        {
            [Resolved]
            private IManiaHitObjectComposer composer { get; set; }

            private readonly Stage stage;

            public Grid(Stage stage)
            {
                this.stage = stage;

                RelativeSizeAxes = Axes.None;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Clock = composer.Playfield.Clock;
            }

            protected override void Update()
            {
                base.Update();

                var parentQuad = Parent.ToLocalSpace(stage.HitObjectContainer.ScreenSpaceDrawQuad);
                Position = parentQuad.TopLeft;
                Size = parentQuad.Size;
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
