// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract partial class DrawableManiaHitObject : DrawableHitObject<ManiaHitObject>
    {
        /// <summary>
        /// The <see cref="ManiaAction"/> which causes this <see cref="DrawableManiaHitObject{TObject}"/> to be hit.
        /// </summary>
        protected readonly IBindable<ManiaAction> Action = new Bindable<ManiaAction>();

        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();
        private readonly Bindable<bool> configTimingBasedNoteColouring = new Bindable<bool>();

        /// <summary>
        /// If timing based note colouring is used, the snap divisor of the note. 0 otherwise.
        /// </summary>
        public IBindable<int> SnapDivisor => snapDivisor;

        private readonly Bindable<int> snapDivisor = new Bindable<int>();

        [Resolved(canBeNull: true)]
        private ManiaPlayfield? playfield { get; set; }

        [Resolved(canBeNull: true)]
        private IBeatmap? beatmap { get; set; }

        protected override float SamplePlaybackPosition
        {
            get
            {
                if (playfield == null)
                    return base.SamplePlaybackPosition;

                return (float)HitObject.Column / playfield.TotalColumns;
            }
        }

        /// <summary>
        /// Whether this <see cref="DrawableManiaHitObject"/> can be hit, given a time value.
        /// If non-null, judgements will be ignored whilst the function returns false.
        /// </summary>
        public Func<DrawableHitObject, double, bool>? CheckHittable;

        protected DrawableManiaHitObject(ManiaHitObject hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(true)]
        private void load(ManiaRulesetConfigManager? config, IBindable<ManiaAction>? action, IScrollingInfo scrollingInfo)
        {
            config?.BindWith(ManiaRulesetSetting.TimingBasedNoteColouring, configTimingBasedNoteColouring);

            if (action != null)
                Action.BindTo(action);

            Direction.BindTo(scrollingInfo.Direction);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            configTimingBasedNoteColouring.BindValueChanged(_ => updateSnapColour());
            StartTimeBindable.BindValueChanged(_ => updateSnapColour(), true);
            Direction.BindValueChanged(OnDirectionChanged, true);
        }

        protected override void OnApply()
        {
            base.OnApply();
            updateSnapColour();
        }

        protected virtual void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            Anchor = Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        private void updateSnapColour()
        {
            if (beatmap == null || HitObject == null) return;

            int divisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(HitObject.StartTime);

            snapDivisor.Value = configTimingBasedNoteColouring.Value ? divisor : 0;
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(150, Easing.In);
                    break;

                case ArmedState.Hit:
                    this.FadeOut();
                    break;
            }
        }

        /// <summary>
        /// Causes this <see cref="DrawableManiaHitObject"/> to get missed, disregarding all conditions in implementations of <see cref="DrawableHitObject.CheckForResult"/>.
        /// </summary>
        public virtual void MissForcefully() => ApplyMinResult();
    }

    public abstract partial class DrawableManiaHitObject<TObject> : DrawableManiaHitObject
        where TObject : ManiaHitObject
    {
        public new TObject HitObject => (TObject)base.HitObject;

        protected DrawableManiaHitObject(TObject hitObject)
            : base(hitObject)
        {
        }
    }
}
