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
    public class SkinBlueprint : SelectionBlueprint<ISkinnableDrawable>
    {
        private Container box;

        private Container outlineBox;

        private AnchorOriginVisualiser anchorOriginVisualiser;

        private Drawable drawable => (Drawable)Item;

        protected override bool ShouldBeAlive => drawable.IsAlive && Item.IsPresent;

        [Resolved]
        private OsuColour colours { get; set; }

        public SkinBlueprint(ISkinnableDrawable component)
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
                        },
                    },
                },
                anchorOriginVisualiser = new AnchorOriginVisualiser(drawable)
                {
                    Alpha = 0,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateSelectedState();
            this.FadeInFromZero(200, Easing.OutQuint);
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

            anchorOriginVisualiser.FadeTo(IsSelected ? 1 : 0, 200, Easing.OutQuint);
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

        public override Vector2 ScreenSpaceSelectionPoint => drawable.ToScreenSpace(drawable.OriginPosition);

        public override Quad SelectionQuad => drawable.ScreenSpaceDrawQuad;
    }

    internal class AnchorOriginVisualiser : CompositeDrawable
    {
        private readonly Drawable drawable;

        private readonly Box originBox;

        private readonly Box anchorBox;
        private readonly Box anchorLine;

        public AnchorOriginVisualiser(Drawable drawable)
        {
            this.drawable = drawable;

            InternalChildren = new Drawable[]
            {
                anchorLine = new Box
                {
                    Height = 2,
                    Origin = Anchor.CentreLeft,
                    Colour = Color4.Yellow,
                    EdgeSmoothness = Vector2.One
                },
                originBox = new Box
                {
                    Colour = Color4.Red,
                    Origin = Anchor.Centre,
                    Size = new Vector2(5),
                },
                anchorBox = new Box
                {
                    Colour = Color4.Red,
                    Origin = Anchor.Centre,
                    Size = new Vector2(5),
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            if (drawable.Parent == null)
                return;

            originBox.Position = drawable.ToSpaceOfOtherDrawable(drawable.OriginPosition, this);
            anchorBox.Position = drawable.Parent.ToSpaceOfOtherDrawable(drawable.AnchorPosition, this);

            var point1 = ToLocalSpace(anchorBox.ScreenSpaceDrawQuad.Centre);
            var point2 = ToLocalSpace(originBox.ScreenSpaceDrawQuad.Centre);

            anchorLine.Position = point1;
            anchorLine.Width = (point2 - point1).Length;
            anchorLine.Rotation = MathHelper.RadiansToDegrees(MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X));
        }
    }
}
