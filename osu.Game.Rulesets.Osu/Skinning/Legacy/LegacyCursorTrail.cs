// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyCursorTrail : CursorTrail
    {
        private readonly ISkin skin;
        private const double disjoint_trail_time_separation = 1000 / 60.0;

        private bool disjointTrail;
        private double lastTrailTime;
        private IBindable<float> cursorSize;

        private Vector2? currentPosition;

        public LegacyCursorTrail(ISkin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Texture = skin.GetTexture("cursortrail");
            disjointTrail = skin.GetTexture("cursormiddle") == null;

            if (disjointTrail)
            {
                bool centre = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.CursorCentre)?.Value ?? true;

                TrailOrigin = centre ? Anchor.Centre : Anchor.TopLeft;
                Blending = BlendingParameters.Inherit;
            }
            else
            {
                Blending = BlendingParameters.Additive;
            }

            if (Texture != null)
            {
                // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                Texture.ScaleAdjust *= 1.6f;
            }

            cursorSize = config.GetBindable<float>(OsuSetting.GameplayCursorSize).GetBoundCopy();
        }

        protected override double FadeDuration => disjointTrail ? 150 : 500;
        protected override float FadeExponent => 1;

        protected override bool InterpolateMovements => !disjointTrail;

        protected override float IntervalMultiplier => 1 / Math.Max(cursorSize.Value, 1);
        protected override bool AvoidDrawingNearCursor => !disjointTrail;

        protected override void Update()
        {
            base.Update();

            if (!disjointTrail || !currentPosition.HasValue)
                return;

            if (Time.Current - lastTrailTime >= disjoint_trail_time_separation)
            {
                lastTrailTime = Time.Current;
                AddTrail(currentPosition.Value);
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!disjointTrail)
                return base.OnMouseMove(e);

            currentPosition = e.ScreenSpaceMousePosition;

            // Intentionally block the base call as we're adding the trails ourselves.
            return false;
        }
    }
}
