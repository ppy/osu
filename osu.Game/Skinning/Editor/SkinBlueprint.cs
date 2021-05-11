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
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprint : SelectionBlueprint<ISkinnableComponent>
    {
        private Container box;

        private Drawable drawable => (Drawable)Item;

        /// <summary>
        /// Whether the blueprint should be shown even when the <see cref="SelectionBlueprint{T}.Item"/> is not alive.
        /// </summary>
        protected virtual bool AlwaysShowWhenSelected => false;

        protected override bool ShouldBeAlive => (drawable.IsAlive && Item.IsPresent) || (AlwaysShowWhenSelected && State == SelectionState.Selected);

        public SkinBlueprint(ISkinnableComponent component)
            : base(component)
        {
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
            box.Rotation = drawable.Rotation;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => drawable.ReceivePositionalInputAt(screenSpacePos);

        public override Vector2 ScreenSpaceSelectionPoint => drawable.ScreenSpaceDrawQuad.Centre;

        public override Quad SelectionQuad => drawable.ScreenSpaceDrawQuad;
    }
}
