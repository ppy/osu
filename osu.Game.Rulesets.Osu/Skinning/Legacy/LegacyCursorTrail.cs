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
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacyCursorTrail : CursorTrail
    {
        private readonly ISkin skin;
        private const double disjoint_trail_time_separation = 1000 / 60.0;

        public bool DisjointTrail { get; private set; }
        private double lastTrailTime;

        private IBindable<float> cursorSize = null!;

        private Vector2? currentPosition;

        public LegacyCursorTrail(ISkin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, ISkinSource skinSource)
        {
            cursorSize = config.GetBindable<float>(OsuSetting.GameplayCursorSize).GetBoundCopy();

            Texture = skin.GetTexture("cursortrail");

            // Cursor and cursor trail components are sourced from potentially different skin sources.
            // Stable always chooses cursor trail disjoint behaviour based on the cursor texture lookup source, so we need to fetch where that occurred.
            // See https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/Graphics/Skinning/SkinManager.cs#L269
            var cursorProvider = skinSource.FindProvider(s => s.GetTexture("cursor") != null);
            DisjointTrail = cursorProvider?.GetTexture("cursormiddle") == null;

            if (DisjointTrail)
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
        }

        protected override double FadeDuration => DisjointTrail ? 150 : 500;
        protected override float FadeExponent => 1;

        protected override bool InterpolateMovements => !DisjointTrail;

        protected override float IntervalMultiplier => 1 / Math.Max(cursorSize.Value, 1);
        protected override bool AvoidDrawingNearCursor => !DisjointTrail;

        protected override void Update()
        {
            base.Update();

            if (!DisjointTrail || !currentPosition.HasValue)
                return;

            if (Time.Current - lastTrailTime >= disjoint_trail_time_separation)
            {
                lastTrailTime = Time.Current;
                AddTrail(currentPosition.Value);
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!DisjointTrail)
                return base.OnMouseMove(e);

            currentPosition = e.ScreenSpaceMousePosition;

            // Intentionally block the base call as we're adding the trails ourselves.
            return false;
        }
    }
}
