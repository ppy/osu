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
    public abstract class ManiaSelectionBlueprint : OverlaySelectionBlueprint
    {
        public new DrawableManiaHitObject DrawableObject => (DrawableManiaHitObject)base.DrawableObject;

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        protected ManiaSelectionBlueprint(DrawableHitObject drawableObject)
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
    }
}
