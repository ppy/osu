// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Stream : OsuHitObject, IHasStreamPath, IHasMultipleComboInformation
    {
        public double EndTime => StreamPath.ControlPoints.Count > 0 ? StartTime + StreamPath.ControlPoints.Last().Time : StartTime;

        [JsonIgnore]
        public double Duration
        {
            get => EndTime - StartTime;
            set
            {
                if (StreamPath.ControlPoints.Count > 0)
                    StreamPath.ControlPoints.Last().Time = value;
            }
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
                updateState();
            }
        }

        public override bool NewCombo
        {
            get => base.NewCombo;
            set
            {
                base.NewCombo = value;
                updateState();
            }
        }

        public BindableList<StreamHitCircleState> HitCircleStates { get; set; } = new BindableList<StreamHitCircleState>();

        public IEnumerable<IHasComboInformation> ComboObjects => HitCircleStates;

        public Stream()
        {
            SamplesBindable.CollectionChanged += (_, _) => UpdateNestedSamples();
            StreamPath.Version.ValueChanged += _ => updateState();
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            foreach (var state in HitCircleStates)
            {
                var h = new StreamHitCircle();
                h.StartTimeBindable.BindTo(state.StartTimeBindable);
                h.PositionBindable.BindTo(state.PositionBindable);
                h.NewComboBindable.BindTo(state.NewComboBindable);
                h.ComboOffsetBindable.BindTo(state.ComboOffsetBindable);
                h.IndexInCurrentComboBindable.BindTo(state.IndexInCurrentComboBindable);
                h.ComboIndexBindable.BindTo(state.ComboIndexBindable);
                h.ComboIndexWithOffsetsBindable.BindTo(state.ComboIndexWithOffsetsBindable);
                h.LastInComboBindable.BindTo(state.LastInComboBindable);

                AddNested(h);
            }

            UpdateNestedSamples();
        }

        public IEnumerable<HitCircle> ToHitCircles()
        {
            return HitCircleStates.Select(state => new HitCircle
            {
                Samples = state.Samples.Select(o => o.With()).ToList(),
                SampleControlPoint = (SampleControlPoint)state.SampleControlPoint.DeepClone(),
                StartTime = state.StartTime,
                Position = state.Position,
                NewCombo = state.NewCombo,
                ComboOffset = state.ComboOffset
            }).ToList(); // Exhaust the states first because the states could change while adding hit circles to the beatmap
        }

        private void updateState()
        {
            var positions = StreamPath.GetStreamPath();
            int i = 0; // index in stream
            int j = 0; // index in stream control segment
            int k = 1; // stream control point

            foreach ((Vector2 pos, double time) in positions)
            {
                var samplePoint = (SampleControlPoint)SampleControlPoint.DeepClone();
                samplePoint.Time = StartTime + time;

                if (i == HitCircleStates.Count)
                    HitCircleStates.Add(createHitCircleState());

                var state = HitCircleStates[i];
                state.StartTime = StartTime + time;
                state.Position = Position + pos;
                state.NewCombo = j == 0 && i != positions.Count - 1 && NewCombo;
                i++;

                if (k >= StreamPath.ControlPoints.Count || ++j < StreamPath.ControlPoints[k].Count) continue;

                j = 0;
                k++;
            }

            // Remove additional states
            for (int l = HitCircleStates.Count - 1; l >= i; l--)
            {
                HitCircleStates.RemoveAt(l);
            }
        }

        protected void UpdateNestedSamples()
        {
            int i = 0;

            foreach (var hitCircle in NestedHitObjects.OfType<StreamHitCircle>())
            {
                var state = getNestedState(i++);

                hitCircle.Samples = state.Samples;
                hitCircle.SampleControlPoint = state.SampleControlPoint;
            }
        }

        private StreamHitCircleState getNestedState(int i)
        {
            return i < HitCircleStates.Count ? HitCircleStates[i] : createHitCircleState();
        }

        private StreamHitCircleState createHitCircleState()
        {
            return new StreamHitCircleState(Samples.Select(o => o.With()).ToList(), (SampleControlPoint)SampleControlPoint.DeepClone());
        }

        public override Judgement CreateJudgement() => new OsuIgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
