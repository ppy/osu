// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class StreamHitCircleState : IHasComboInformation, IHasPosition
    {
        public StreamHitCircleState(IList<HitSampleInfo> samples, SampleControlPoint sampleControlPoint)
        {
            Samples = samples;
            SampleControlPoint = sampleControlPoint;
        }

        public IList<HitSampleInfo> Samples { get; set; }
        public SampleControlPoint SampleControlPoint { get; set; }

        public Bindable<double> StartTimeBindable = new BindableDouble();

        public double StartTime
        {
            get => StartTimeBindable.Value;
            set => StartTimeBindable.Value = value;
        }

        public Bindable<Vector2> PositionBindable { get; } = new Bindable<Vector2>();

        public Vector2 Position
        {
            get => PositionBindable.Value;
            set => PositionBindable.Value = value;
        }

        public float X => Position.X;
        public float Y => Position.Y;

        public Bindable<bool> NewComboBindable { get; } = new Bindable<bool>();

        public bool NewCombo
        {
            get => NewComboBindable.Value;
            set => NewComboBindable.Value = value;
        }

        public Bindable<int> ComboOffsetBindable { get; } = new Bindable<int>();

        public int ComboOffset
        {
            get => ComboOffsetBindable.Value;
            set => ComboOffsetBindable.Value = value;
        }

        public Bindable<int> IndexInCurrentComboBindable { get; } = new Bindable<int>();

        public int IndexInCurrentCombo
        {
            get => IndexInCurrentComboBindable.Value;
            set => IndexInCurrentComboBindable.Value = value;
        }

        public Bindable<int> ComboIndexBindable { get; } = new Bindable<int>();

        public virtual int ComboIndex
        {
            get => ComboIndexBindable.Value;
            set => ComboIndexBindable.Value = value;
        }

        public Bindable<int> ComboIndexWithOffsetsBindable { get; } = new Bindable<int>();

        public int ComboIndexWithOffsets
        {
            get => ComboIndexWithOffsetsBindable.Value;
            set => ComboIndexWithOffsetsBindable.Value = value;
        }

        public Bindable<bool> LastInComboBindable => new Bindable<bool>();

        public bool LastInCombo
        {
            get => LastInComboBindable.Value;
            set => LastInComboBindable.Value = value;
        }
    }
}
