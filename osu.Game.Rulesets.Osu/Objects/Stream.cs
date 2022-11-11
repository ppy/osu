// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Stream : OsuHitObject, IHasPath
    {
        public double EndTime => StreamPath.ControlPoints.Count > 0 ? StreamPath.ControlPoints.Last().Time : StartTime;

        [JsonIgnore]
        public double Duration
        {
            get => EndTime - StartTime;
            set => throw new System.NotSupportedException($"Adjust via {nameof(StreamPath)} instead"); // can be implemented if/when needed.
        }

        private readonly StreamPath streamPath = new StreamPath();

        public StreamPath StreamPath
        {
            get => streamPath;
            set
            {
                streamPath.Path = value.Path;
                streamPath.ControlPoints.Clear();
                streamPath.ControlPoints.AddRange(value.ControlPoints.Select(c => new StreamControlPoint(c.Time, c.Count, c.Acceleration, c.Exponential)));
            }
        }

        public SliderPath Path => StreamPath.Path;

        public double Distance => StreamPath.Path.Distance;

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                updateNestedPositions();
            }
        }

        public IList<IList<HitSampleInfo>> HitCircleSamples { get; set; } = new List<IList<HitSampleInfo>>();

        public Stream()
        {
            SamplesBindable.CollectionChanged += (_, _) => UpdateNestedSamples();
            StreamPath.Version.ValueChanged += _ => updateNestedPositions();
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            // Generate nested hit circles
            foreach ((Vector2 pos, double time) in StreamPath.GetStreamPath())
            {
                AddNested(new StreamHitCircle
                {
                    StartTime = StartTime + time,
                    Position = Position + pos
                });
            }

            UpdateNestedSamples();
        }

        public IEnumerable<HitCircle> ToHitCircles()
        {
            var positions = StreamPath.GetStreamPath();
            int i = 0;
            int j = 0;
            int k = 1;

            foreach ((Vector2 pos, double time) in positions)
            {
                var samplePoint = (SampleControlPoint)SampleControlPoint.DeepClone();
                samplePoint.Time = time;

                yield return new HitCircle
                {
                    StartTime = StartTime + time,
                    Position = Position + pos,
                    NewCombo = j == 0 && i != positions.Count - 1 && NewCombo,
                    SampleControlPoint = samplePoint,
                    Samples = getNestedSample(i++).Select(o => o.With()).ToList()
                };

                if (k >= StreamPath.ControlPoints.Count || ++j < StreamPath.ControlPoints[k].Count) continue;

                j = 0;
                k++;
            }
        }

        private void updateNestedPositions()
        {
        }

        protected void UpdateNestedSamples()
        {
            int i = 0;

            foreach (var hitCircle in NestedHitObjects.OfType<StreamHitCircle>())
            {
                hitCircle.Samples = getNestedSample(i++);
            }
        }

        private IList<HitSampleInfo> getNestedSample(int i)
        {
            return i < HitCircleSamples.Count ? HitCircleSamples[i] : Samples;
        }
    }
}
