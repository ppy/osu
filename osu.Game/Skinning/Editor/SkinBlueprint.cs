// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprint : SelectionBlueprint<SkinnableHUDComponent>
    {
        /// <summary>
        /// The <see cref="DrawableHitObject"/> which this <see cref="OverlaySelectionBlueprint"/> applies to.
        /// </summary>
        public readonly SkinnableHUDComponent Component;

        private Container box;
        private Drawable drawable => Component.Drawable;

        /// <summary>
        /// Whether the blueprint should be shown even when the <see cref="Component"/> is not alive.
        /// </summary>
        protected virtual bool AlwaysShowWhenSelected => false;

        protected override bool ShouldBeAlive => (Component.IsAlive && Component.IsPresent) || (AlwaysShowWhenSelected && State == SelectionState.Selected);

        public SkinBlueprint(SkinnableHUDComponent component)
            : base(component)
        {
            Component = component;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                box = new Container
                {
                    Colour = colours.Yellow,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.2f,
                            AlwaysPresent = true,
                        },
                    }
                },
            };
        }

        private Quad drawableQuad;

        public override Quad ScreenSpaceDrawQuad => drawableQuad;

        protected override void Update()
        {
            base.Update();

            drawableQuad = drawable.ScreenSpaceDrawQuad;
            var quad = ToLocalSpace(drawable.ScreenSpaceDrawQuad);

            box.Position = quad.TopLeft;
            box.Size = quad.Size;
            box.Rotation = Component.Rotation;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => drawable.ReceivePositionalInputAt(screenSpacePos);

        public override Vector2 ScreenSpaceSelectionPoint => Component.ToScreenSpace(Vector2.Zero);

        public override Quad SelectionQuad => drawable.ScreenSpaceDrawQuad;

        public override Vector2 GetInstantDelta(Vector2 screenSpacePosition) => Component.Parent.ToLocalSpace(screenSpacePosition) - Component.Position;
    }
}
