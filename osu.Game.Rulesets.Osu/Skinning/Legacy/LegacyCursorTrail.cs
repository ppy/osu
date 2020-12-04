// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyCursorTrail : CursorTrail
    {
        private const double disjoint_trail_time_separation = 1000 / 60.0;

        private bool disjointTrail;
        private double lastTrailTime;
        private IBindable<float> cursorSize;

        public LegacyCursorTrail()
        {
            Blending = BlendingParameters.Additive;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuConfigManager config)
        {
            Texture = skin.GetTexture("cursortrail");
            disjointTrail = skin.GetTexture("cursormiddle") == null;

            if (Texture != null)
            {
                // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                Texture.ScaleAdjust *= 1.6f;
            }

            cursorSize = config.GetBindable<float>(OsuSetting.GameplayCursorSize).GetBoundCopy();
        }

        protected override double FadeDuration => disjointTrail ? 150 : 500;

        protected override bool InterpolateMovements => !disjointTrail;

        protected override float IntervalMultiplier => 1 / Math.Max(cursorSize.Value, 1);

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!disjointTrail)
                return base.OnMouseMove(e);

            if (Time.Current - lastTrailTime >= disjoint_trail_time_separation)
            {
                lastTrailTime = Time.Current;
                return base.OnMouseMove(e);
            }

            return false;
        }
    }
}
