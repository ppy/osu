// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinBlueprint : SelectionBlueprint<ISerialisableDrawable>
    {
        private Container box = null!;

        private AnchorOriginVisualiser anchorOriginVisualiser = null!;

        private OsuSpriteText label = null!;

        private Drawable drawable => (Drawable)Item;

        protected override bool ShouldBeAlive => drawable.IsAlive && Item.IsPresent;

        private Quad drawableQuad;

        public override Quad ScreenSpaceDrawQuad => drawableQuad;
        public override Quad SelectionQuad => drawable.ScreenSpaceDrawQuad;

        public override bool Contains(Vector2 screenSpacePos) => drawableQuad.Contains(screenSpacePos);

        public override Vector2 ScreenSpaceSelectionPoint => drawable.ToScreenSpace(drawable.OriginPosition);

        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos) =>
            drawableQuad.Contains(screenSpacePos);

        public SkinBlueprint(ISerialisableDrawable component)
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
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 3,
                            BorderThickness = SelectionBox.BORDER_RADIUS / 2,
                            BorderColour = ColourInfo.GradientVertical(colours.Pink4.Darken(0.4f), colours.Pink4),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Blending = BlendingParameters.Additive,
                                    Alpha = 0.2f,
                                    Colour = ColourInfo.GradientVertical(colours.Pink2, colours.Pink4),
                                    AlwaysPresent = true,
                                },
                            }
                        },
                        label = new OsuSpriteText
                        {
                            Text = Item.GetType().Name,
                            Font = OsuFont.Default.With(size: 10, weight: FontWeight.Bold),
                            Alpha = 0,
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
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateSelectedState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateSelectedState();
            base.OnHoverLost(e);
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
            anchorOriginVisualiser.FadeTo(IsSelected ? 1 : 0, 200, Easing.OutQuint);
            label.FadeTo(IsSelected || IsHovered ? 1 : 0, 200, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            drawableQuad = drawable.ToScreenSpace(
                drawable.DrawRectangle);

            var localSpaceQuad = ToLocalSpace(drawableQuad);

            float cos = MathF.Cos(MathUtils.DegreesToRadians(drawable.Rotation));
            float sin = MathF.Sin(MathUtils.DegreesToRadians(drawable.Rotation));

            float offsetX = drawable.Scale.X > 0 ? -SkinSelectionHandler.INFLATE_SIZE : SkinSelectionHandler.INFLATE_SIZE;
            float offsetY = drawable.Scale.Y > 0 ? -SkinSelectionHandler.INFLATE_SIZE : SkinSelectionHandler.INFLATE_SIZE;

            float rotatedX = cos * offsetX - sin * offsetY;
            float rotatedY = sin * offsetX + cos * offsetY;

            box.Position = new Vector2(localSpaceQuad.TopLeft[0] + rotatedX, localSpaceQuad.TopLeft[1] + rotatedY);
            box.Size = new Vector2(localSpaceQuad.Width + MathF.Abs(SkinSelectionHandler.INFLATE_SIZE) * 2,
                localSpaceQuad.Height + MathF.Abs(SkinSelectionHandler.INFLATE_SIZE) * 2);
            box.Rotation = drawable.Rotation;
            box.Scale = new Vector2(MathF.Sign(drawable.Scale.X), MathF.Sign(drawable.Scale.Y));
        }
    }

    internal partial class AnchorOriginVisualiser : CompositeDrawable
    {
        private readonly Drawable drawable;

        private Drawable originBox = null!;

        private Drawable anchorBox = null!;
        private Drawable anchorLine = null!;

        public AnchorOriginVisualiser(Drawable drawable)
        {
            this.drawable = drawable;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Color4 anchorColour = colours.Red1;
            Color4 originColour = colours.Red3;

            InternalChildren = new[]
            {
                anchorLine = new Circle
                {
                    Height = 3f,
                    Origin = Anchor.CentreLeft,
                    Colour = ColourInfo.GradientHorizontal(originColour.Opacity(0.5f), originColour),
                },
                originBox = new Circle
                {
                    Colour = originColour,
                    Origin = Anchor.Centre,
                    Size = new Vector2(7),
                },
                anchorBox = new Circle
                {
                    Colour = anchorColour,
                    Origin = Anchor.Centre,
                    Size = new Vector2(10),
                },
            };
        }

        private Vector2? anchorPosition;
        private Vector2? originPositionInDrawableSpace;

        protected override void Update()
        {
            base.Update();

            if (drawable.Parent == null)
                return;

            var newAnchor = drawable.Parent!.ToSpaceOfOtherDrawable(drawable.AnchorPosition, this);
            anchorPosition = tweenPosition(anchorPosition ?? newAnchor, newAnchor);
            anchorBox.Position = anchorPosition.Value;

            // for the origin, tween in the drawable's local space to avoid unwanted tweening when the drawable is being dragged.
            originPositionInDrawableSpace = originPositionInDrawableSpace != null ? tweenPosition(originPositionInDrawableSpace.Value, drawable.OriginPosition) : drawable.OriginPosition;
            originBox.Position = drawable.ToSpaceOfOtherDrawable(originPositionInDrawableSpace.Value, this);

            var point1 = ToLocalSpace(anchorBox.ScreenSpaceDrawQuad.Centre);
            var point2 = ToLocalSpace(originBox.ScreenSpaceDrawQuad.Centre);

            anchorLine.Position = point1;
            anchorLine.Width = (point2 - point1).Length;
            anchorLine.Rotation = MathHelper.RadiansToDegrees(MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X));
        }

        private Vector2 tweenPosition(Vector2 oldPosition, Vector2 newPosition)
            => new Vector2(
                (float)Interpolation.DampContinuously(oldPosition.X, newPosition.X, 25, Clock.ElapsedFrameTime),
                (float)Interpolation.DampContinuously(oldPosition.Y, newPosition.Y, 25, Clock.ElapsedFrameTime)
            );
    }
}
