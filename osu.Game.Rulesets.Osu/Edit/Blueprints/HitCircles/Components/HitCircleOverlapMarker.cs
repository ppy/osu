// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components
{
    public partial class HitCircleOverlapMarker : BlueprintPiece<HitCircle>
    {
        /// <summary>
        /// Hit objects are intentionally made to fade out at a constant slower rate than in gameplay.
        /// This allows a mapper to gain better historical context and use recent hitobjects as reference / snap points.
        /// </summary>
        public const double FADE_OUT_EXTENSION = 700;

        private readonly RingPiece ring;

        private readonly Container content;

        [Resolved]
        private EditorClock editorClock { get; set; }

        private Bindable<bool> showHitMarkers;

        public HitCircleOverlapMarker()
        {
            Origin = Anchor.Centre;

            Size = OsuHitObject.OBJECT_DIMENSIONS;

            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    },
                    ring = new RingPiece
                    {
                        BorderThickness = 4,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            showHitMarkers = config.GetBindable<bool>(OsuSetting.EditorShowHitMarkers);
        }

        [Resolved]
        private ISkinSource skin { get; set; }

        public override void UpdateFrom(HitCircle hitObject)
        {
            base.UpdateFrom(hitObject);

            Scale = new Vector2(hitObject.Scale);

            double editorTime = editorClock.CurrentTime;
            double hitObjectTime = hitObject.StartTime;
            bool hasReachedObject = editorTime >= hitObjectTime;

            if (hasReachedObject && showHitMarkers.Value)
            {
                float alpha = Interpolation.ValueAt(editorTime, 0, 1f, hitObjectTime, hitObjectTime + FADE_OUT_EXTENSION, Easing.In);
                float ringScale = MathHelper.Clamp(Interpolation.ValueAt(editorTime, 0, 1f, hitObjectTime, hitObjectTime + FADE_OUT_EXTENSION / 2, Easing.OutQuint), 0, 1);

                ring.Scale = new Vector2(1 + 0.1f * ringScale);
                content.Alpha = 0.9f * (1 - alpha);

                // TODO: should only update colour on skin/combo/object change.
                if (hitObject is IHasComboInformation combo && content.Alpha > 0)
                    ring.BorderColour = combo.GetComboColour(skin);
            }
            else
                content.Alpha = 0;
        }

        public override void Show()
        {
            // intentional no op so SelectionBlueprint Selection/Deselection logic doesn't touch us.
        }

        public override void Hide()
        {
            // intentional no op so SelectionBlueprint Selection/Deselection logic doesn't touch us.
        }
    }
}
