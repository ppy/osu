// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class ManiaSelectionBlueprint : OverlaySelectionBlueprint
    {
        public new DrawableManiaHitObject DrawableObject => (DrawableManiaHitObject)base.DrawableObject;

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [Resolved]
        private IManiaHitObjectComposer composer { get; set; }

        public ManiaSelectionBlueprint(DrawableHitObject drawableObject)
            : base(drawableObject)
        {
            RelativeSizeAxes = Axes.None;
        }

        protected override void Update()
        {
            base.Update();

            Position = Parent.ToLocalSpace(DrawableObject.ToScreenSpace(Vector2.Zero));
        }

        public override void Show()
        {
            DrawableObject.AlwaysAlive = true;
            base.Show();
        }

        public override void Hide()
        {
            DrawableObject.AlwaysAlive = false;
            base.Hide();
        }

        public override Vector2 GetInstantDelta(Vector2 screenSpacePosition)
        {
            var baseDelta = base.GetInstantDelta(screenSpacePosition);

            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
            {
                // The parent of DrawableObject is the scrolling hitobject container (SHOC).
                // In the coordinate-space of the SHOC, the screen-space position at the hit target is equal to the height of the SHOC,
                // but this is not what we want as it means a slight movement downwards results in a delta greater than the height of the SHOC.
                // To get around this issue, the height of the SHOC is subtracted from the delta.
                //
                // Ideally this should be a _negative_ value in the case described above, however this code gives a _positive_ delta.
                // This is intentional as the delta is added to the hitobject's position (see: ManiaSelectionHandler) and a negative delta would move them towards the top of the screen instead,
                // which would cause the delta to get increasingly larger as additional movements are performed.
                return new Vector2(baseDelta.X, baseDelta.Y - DrawableObject.Parent.DrawHeight);
            }

            return baseDelta;
        }
    }
}
