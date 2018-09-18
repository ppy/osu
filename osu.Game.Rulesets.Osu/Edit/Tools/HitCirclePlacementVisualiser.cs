// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.States;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Tools
{
    public class HitCirclePlacementVisualiser : PlacementVisualiser
    {
        public new HitCircle HitObject => (HitCircle)base.HitObject;

        public HitCirclePlacementVisualiser()
            : base(new HitCircle())
        {
            Alpha = 0.75f;

            InternalChild = new DrawableHitCircle(HitObject);
        }

        protected override bool OnClick(InputState state)
        {
            Finish();
            return base.OnClick(state);
        }

        private class DrawableHitCircle : Objects.Drawables.DrawableHitCircle
        {
            public DrawableHitCircle(HitCircle h)
                : base(h)
            {
                Position = Vector2.Zero;
                Alpha = 1;
            }

            protected override void UpdatePreemptState()
            {
            }

            protected override void UpdateCurrentState(ArmedState state)
            {
            }
        }
    }
}
