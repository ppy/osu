// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components
{
    public class HitCircleOverlapMarker : BlueprintPiece<HitCircle>
    {
        private readonly Circle circle;
        private readonly RingPiece ring;

        [Resolved]
        private EditorClock editorClock { get; set; }

        public HitCircleOverlapMarker()
        {
            Origin = Anchor.Centre;

            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            InternalChildren = new Drawable[]
            {
                circle = new Circle
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
            };
        }

        [Resolved]
        private ISkinSource skin { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            circle.BorderColour = colours.Yellow;
        }

        public override void UpdateFrom(HitCircle hitObject)
        {
            base.UpdateFrom(hitObject);

            Scale = new Vector2(hitObject.Scale);

            if ((hitObject is IHasComboInformation combo))
                ring.BorderColour = combo.GetComboColour(skin);

            bool hasReachedObject = editorClock.CurrentTime >= hitObject.StartTime;
            float interpolation = Interpolation.ValueAt(editorClock.CurrentTime, 0, 1f, hitObject.StartTime, hitObject.StartTime + DrawableOsuEditorRuleset.EDITOR_HIT_OBJECT_FADE_OUT_EXTENSION, Easing.In);
            float interpolation2 = MathHelper.Clamp(Interpolation.ValueAt(editorClock.CurrentTime, 0, 1f, hitObject.StartTime, hitObject.StartTime + DrawableOsuEditorRuleset.EDITOR_HIT_OBJECT_FADE_OUT_EXTENSION / 2, Easing.OutQuint), 0, 1);
            float interpolation3 = Interpolation.ValueAt(editorClock.CurrentTime, 0, 1f, hitObject.StartTime, hitObject.StartTime + DrawableOsuEditorRuleset.EDITOR_HIT_OBJECT_FADE_OUT_EXTENSION);

            if (hasReachedObject)
            {
                circle.Scale = new Vector2(1 - 0.05f * interpolation3);
                ring.Scale = new Vector2(1 + 0.1f * interpolation2);
                Alpha = 0.9f * (1 - (interpolation));
            }
            else
                Alpha = 0;
        }

        public override void Hide()
        {
            // intentional no op so we are not hidden when not selected.
        }
    }
}
