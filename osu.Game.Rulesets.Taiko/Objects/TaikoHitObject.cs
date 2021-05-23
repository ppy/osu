// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public abstract class TaikoHitObject : HitObject
    {
        public readonly Bindable<HitType> TypeBindable = new Bindable<HitType>();

        /// <summary>
        /// The <see cref="HitType"/> that actuates this <see cref="Hit"/>.
        /// </summary>
        public HitType Type
        {
            get => TypeBindable.Value;
            set => TypeBindable.Value = value;
        }

        /// <summary>
        /// Default size of a drawable taiko hit object.
        /// </summary>
        public const float DEFAULT_SIZE = 0.45f;

        public override Judgement CreateJudgement() => new TaikoJudgement();

        protected override HitWindows CreateHitWindows() => new TaikoHitWindows();

        protected TaikoHitObject()
        {
            SamplesBindable.BindCollectionChanged((_, __) => UpdateTypeFromSamples());
            TypeBindable.BindValueChanged(_ => UpdateSamplesFromType());
        }

        protected virtual void UpdateSamplesFromType()
        {
        }

        protected virtual void UpdateTypeFromSamples()
        {
        }
    }
}
