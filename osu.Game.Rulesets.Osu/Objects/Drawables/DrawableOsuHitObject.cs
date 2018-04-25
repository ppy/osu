// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using System.Linq;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject>
    {
        public override bool IsPresent => base.IsPresent || State.Value == ArmedState.Idle && Time.Current >= HitObject.StartTime - HitObject.TimePreempt;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
            Alpha = 0;
        }

        protected sealed override void UpdateState(ArmedState state)
        {
            double transformTime = HitObject.StartTime - HitObject.TimePreempt;

            base.ApplyTransformsAt(transformTime, true);
            base.ClearTransformsAfter(transformTime, true);

            using (BeginAbsoluteSequence(transformTime, true))
            {
                UpdatePreemptState();

                using (BeginDelayedSequence(HitObject.TimePreempt + (Judgements.FirstOrDefault()?.TimeOffset ?? 0), true))
                    UpdateCurrentState(state);
            }
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            if (HitObject is IHasComboInformation combo)
                AccentColour = skin.GetValue<SkinConfiguration, Color4>(s => s.ComboColours.Count > 0 ? s.ComboColours[combo.ComboIndex % s.ComboColours.Count] : (Color4?)null) ?? Color4.White;
        }

        protected virtual void UpdatePreemptState() => this.FadeIn(HitObject.TimeFadein);

        protected virtual void UpdateCurrentState(ArmedState state)
        {
        }

        // Todo: At some point we need to move these to DrawableHitObject after ensuring that all other Rulesets apply
        // transforms in the same way and don't rely on them not being cleared
        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null) { }
        public override void ApplyTransformsAt(double time, bool propagateChildren = false) { }

        private OsuInputManager osuActionInputManager;
        internal OsuInputManager OsuActionInputManager => osuActionInputManager ?? (osuActionInputManager = GetContainingInputManager() as OsuInputManager);
    }

    public enum ComboResult
    {
        [Description(@"")]
        None,
        [Description(@"Good")]
        Good,
        [Description(@"Amazing")]
        Perfect
    }
}
