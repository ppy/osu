// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class ManiaSelectionBlueprint : SelectionBlueprint
    {
        public Vector2 ScreenSpaceDragPosition { get; private set; }
        public Vector2 DragPosition { get; private set; }

        protected new DrawableManiaHitObject HitObject => (DrawableManiaHitObject)base.HitObject;

        protected IClock EditorClock { get; private set; }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [Resolved]
        private IManiaHitObjectComposer composer { get; set; }

        public ManiaSelectionBlueprint(DrawableHitObject hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;
        }

        [BackgroundDependencyLoader]
        private void load(IAdjustableClock clock)
        {
            EditorClock = clock;
        }

        protected override void Update()
        {
            base.Update();

            Position = Parent.ToLocalSpace(HitObject.ToScreenSpace(Vector2.Zero));
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            ScreenSpaceDragPosition = e.ScreenSpaceMousePosition;
            DragPosition = HitObject.ToLocalSpace(e.ScreenSpaceMousePosition);

            return base.OnMouseDown(e);
        }

        protected override bool OnDrag(DragEvent e)
        {
            var result = base.OnDrag(e);

            ScreenSpaceDragPosition = e.ScreenSpaceMousePosition;
            DragPosition = HitObject.ToLocalSpace(e.ScreenSpaceMousePosition);

            return result;
        }

        public override void Show()
        {
            HitObject.AlwaysAlive = true;
            base.Show();
        }

        public override void Hide()
        {
            HitObject.AlwaysAlive = false;
            base.Hide();
        }
    }
}
