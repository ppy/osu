// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods.Objects
{
    public class StrictTrackingSlider : Slider
    {
        public StrictTrackingSlider(Slider original)
        {
            StartTime = original.StartTime;
            Samples = original.Samples;
            Path = original.Path;
            NodeSamples = original.NodeSamples;
            RepeatCount = original.RepeatCount;
            Position = original.Position;
            NewCombo = original.NewCombo;
            ComboOffset = original.ComboOffset;
            LegacyLastTickOffset = original.LegacyLastTickOffset;
            TickDistanceMultiplier = original.TickDistanceMultiplier;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            var sliderEvents = SliderEventGenerator.Generate(StartTime, SpanDuration, Velocity, TickDistance, Path.Distance, this.SpanCount(), LegacyLastTickOffset, cancellationToken);

            foreach (var e in sliderEvents)
            {
                switch (e.Type)
                {
                    case SliderEventType.Head:
                        AddNested(HeadCircle = new SliderHeadCircle
                        {
                            StartTime = e.Time,
                            Position = Position,
                            StackHeight = StackHeight,
                        });
                        break;

                    case SliderEventType.LegacyLastTick:
                        AddNested(TailCircle = new StrictTrackingSliderTailCircle(this)
                        {
                            RepeatIndex = e.SpanIndex,
                            StartTime = e.Time,
                            Position = EndPosition,
                            StackHeight = StackHeight
                        });
                        break;

                    case SliderEventType.Repeat:
                        AddNested(new SliderRepeat(this)
                        {
                            RepeatIndex = e.SpanIndex,
                            StartTime = StartTime + (e.SpanIndex + 1) * SpanDuration,
                            Position = Position + Path.PositionAt(e.PathProgress),
                            StackHeight = StackHeight,
                            Scale = Scale,
                        });
                        break;
                }
            }

            UpdateNestedSamples();
        }
    }
}
