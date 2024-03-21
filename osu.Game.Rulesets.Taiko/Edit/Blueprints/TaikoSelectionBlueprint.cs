// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public partial class TaikoSelectionBlueprint : HitObjectSelectionBlueprint
    {
        public TaikoSelectionBlueprint(HitObject hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;

            AddInternal(new HitPiece
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.TopLeft
            });
        }

        protected override void Update()
        {
            base.Update();

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            topLeft = Vector2.ComponentMin(topLeft, Parent!.ToLocalSpace(DrawableObject.ScreenSpaceDrawQuad.TopLeft));
            bottomRight = Vector2.ComponentMax(bottomRight, Parent!.ToLocalSpace(DrawableObject.ScreenSpaceDrawQuad.BottomRight));

            Size = bottomRight - topLeft;
            Position = topLeft;
        }
    }
}
