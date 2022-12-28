// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacyCursorTrail : CursorTrail
    {
        private readonly ISkin skin;
        private readonly BindableBool forceLong = new BindableBool();
        private const double disjoint_trail_time_separation = 1000 / 60.0;

        private bool disjointTrail;
        private double lastTrailTime;

        private IBindable<float> cursorSize = null!;

        private Vector2? currentPosition;

        public LegacyCursorTrail(ISkin skin)
        {
            this.skin = skin;
        }

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuRulesetConfigManager rulesetConfig)
        {
            cursorSize = config.GetBindable<float>(OsuSetting.GameplayCursorSize).GetBoundCopy();

            Texture = skin.GetTexture("cursortrail");
            rulesetConfig.BindWith(OsuRulesetSetting.CursorTrailForceLong, forceLong);
            forceLong.BindValueChanged(_ => updateDisjoint(), true);

            if (Texture != null)
            {
                // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                Texture.ScaleAdjust *= 1.6f;
            }

            cursorSize = config.GetBindable<float>(OsuSetting.GameplayCursorSize).GetBoundCopy();
        }

        private void updateDisjoint()
        {
            ResetTime();
            disjointTrail = skin.GetTexture("cursormiddle") == null && !forceLong.Value;

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
