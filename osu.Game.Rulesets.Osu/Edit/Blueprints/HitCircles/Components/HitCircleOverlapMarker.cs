// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;
using osuTK;

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
            skin.SourceChanged += updateColour;
        }

        [Resolved]
        private ISkinSource skin { get; set; }

        private HitCircle hitObject;

        public override void UpdateFrom(HitCircle hitObject)
        {
            base.UpdateFrom(hitObject);

            if (this.hitObject != hitObject)
            {
                if (this.hitObject != null)
                    this.hitObject.DefaultsApplied -= onDefaultsApplied;

                this.hitObject = hitObject;

                if (this.hitObject != null)
                    this.hitObject.DefaultsApplied += onDefaultsApplied;

                updateColour();
            }

            Scale = new Vector2(hitObject.Scale);

            double editorTime = editorClock.CurrentTime;
            double hitObjectTime = hitObject.StartTime;
            bool hasReachedObject = editorTime >= hitObjectTime;

            if (hasReachedObject && showHitMarkers.Value)
            {
                float alpha = Interpolation.ValueAt(editorTime, 0, 1f, hitObjectTime, hitObjectTime + FADE_OUT_EXTENSION, Easing.In);
                float ringScale = Math.Clamp(Interpolation.ValueAt(editorTime, 0, 1f, hitObjectTime, hitObjectTime + FADE_OUT_EXTENSION / 2, Easing.OutQuint), 0, 1);

                ring.Scale = new Vector2(1 + 0.1f * ringScale);
                content.Alpha = 0.9f * (1 - alpha);
            }
            else
                content.Alpha = 0;
        }

        private void onDefaultsApplied(HitObject _) => updateColour();

        private void updateColour()
        {
            if (hitObject is IHasComboInformation combo)
                ring.BorderColour = combo.GetComboColour(skin);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skin != null)
                skin.SourceChanged -= updateColour;

            if (hitObject != null)
                hitObject.DefaultsApplied -= onDefaultsApplied;
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
