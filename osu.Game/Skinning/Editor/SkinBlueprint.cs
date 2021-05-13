// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprint : SelectionBlueprint<ISkinnableComponent>
    {
        private Container box;

        private Container outlineBox;

        private Drawable drawable => (Drawable)Item;

        /// <summary>
        /// Whether the blueprint should be shown even when the <see cref="SelectionBlueprint{T}.Item"/> is not alive.
        /// </summary>
        protected virtual bool AlwaysShowWhenSelected => true;

        protected override bool ShouldBeAlive => drawable.IsAlive && Item.IsPresent;

        [Resolved]
        private OsuColour colours { get; set; }

        public SkinBlueprint(ISkinnableComponent component)
            : base(component)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                box = new Container
                {
                    Children = new Drawable[]
                    {
                        outlineBox = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderThickness = 3,
                            BorderColour = Color4.White,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0f,
                                    AlwaysPresent = true,
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Text = Item.GetType().Name,
                            Font = OsuFont.Default.With(size: 10, weight: FontWeight.Bold),
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.TopRight,
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateSelectedState();
            box.FadeInFromZero(200, Easing.OutQuint);
        }

        protected override void OnSelected()
        {
            // base logic hides selected blueprints when not selected, but skin blueprints don't do that.
            updateSelectedState();
        }

        protected override void OnDeselected()
        {
            // base logic hides selected blueprints when not selected, but skin blueprints don't do that.
            updateSelectedState();
        }

        private void updateSelectedState()
        {
            outlineBox.FadeColour(colours.Pink.Opacity(IsSelected ? 1 : 0.5f), 200, Easing.OutQuint);
            outlineBox.Child.FadeTo(IsSelected ? 0.2f : 0, 200, Easing.OutQuint);
        }

        private Quad drawableQuad;

        public override Quad ScreenSpaceDrawQuad => drawableQuad;

        protected override void Update()
        {
            base.Update();

            drawableQuad = drawable.ScreenSpaceDrawQuad;
            var quad = ToLocalSpace(drawable.ScreenSpaceDrawQuad);

            box.Position = drawable.ToSpaceOfOtherDrawable(Vector2.Zero, this);
            box.Size = quad.Size;
            box.Rotation = drawable.Rotation;
            box.Scale = new Vector2(MathF.Sign(drawable.Scale.X), MathF.Sign(drawable.Scale.Y));
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => drawable.ReceivePositionalInputAt(screenSpacePos);

        public override Vector2 ScreenSpaceSelectionPoint => drawable.ScreenSpaceDrawQuad.Centre;

        public override Quad SelectionQuad => drawable.ScreenSpaceDrawQuad;
    }
}
