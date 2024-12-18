// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles
{
    public partial class HitCircleSelectionBlueprint : OsuSelectionBlueprint<HitCircle>
    {
        protected new DrawableHitCircle DrawableObject => (DrawableHitCircle)base.DrawableObject;

        protected readonly HitCirclePiece CirclePiece;
        private readonly HitCircleOverlapMarker marker;
        private readonly Bindable<bool> showHitMarkers = new Bindable<bool>();

        public HitCircleSelectionBlueprint(HitCircle circle)
            : base(circle)
        {
            InternalChildren = new Drawable[]
            {
                marker = new HitCircleOverlapMarker(),
                CirclePiece = new HitCirclePiece(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.EditorShowHitMarkers, showHitMarkers);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showHitMarkers.BindValueChanged(_ =>
            {
                if (!showHitMarkers.Value)
                    DrawableObject.RestoreHitAnimations();
            });
        }

        protected override void Update()
        {
            base.Update();

            CirclePiece.UpdateFrom(HitObject);
            marker.UpdateFrom(HitObject);

            if (showHitMarkers.Value)
                DrawableObject.SuppressHitAnimations();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.HitArea.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => CirclePiece.ScreenSpaceDrawQuad;
    }
}
