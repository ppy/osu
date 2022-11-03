// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Game.Audio;
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
                streamPath.ControlPoints.AddRange(value.ControlPoints.Select(c => new StreamControlPoint(c.Time, c.Count, c.Ratio)));
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
                AddNested(new HitCircle
                {
                    StartTime = StartTime + time,
                    Position = Position + pos
                });
            }

            UpdateNestedSamples();
        }

        private void updateNestedPositions()
        {
        }

        protected void UpdateNestedSamples()
        {
            int i = 0;

            foreach (var hitCircle in NestedHitObjects.OfType<HitCircle>())
            {
                hitCircle.Samples = i < HitCircleSamples.Count ? HitCircleSamples[i] : Samples;
                i++;
            }
        }
    }
}
