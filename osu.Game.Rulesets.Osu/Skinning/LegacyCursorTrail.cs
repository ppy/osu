// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class LegacyCursorTrail : CursorTrail
    {
        private const double disjoint_trail_time_separation = 1000 / 60.0;

        private bool disjointTrail;
        private double lastTrailTime;

        public LegacyCursorTrail()
        {
            Blending = BlendingParameters.Additive;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Texture = skin.GetTexture("cursortrail");
            disjointTrail = skin.GetTexture("cursormiddle") == null;

            if (Texture != null)
            {
                // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                Texture.ScaleAdjust *= 1.6f;
            }
        }

        protected override double FadeDuration => disjointTrail ? 150 : 500;

        protected override bool InterpolateMovements => !disjointTrail;

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
