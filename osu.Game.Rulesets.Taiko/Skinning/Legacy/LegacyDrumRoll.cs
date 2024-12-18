// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacyDrumRoll : CompositeDrawable, IHasAccentColour
    {
        public override Quad ScreenSpaceDrawQuad
        {
            get
            {
                // the reason why this calculation is so involved is that the head & tail sprites have different sizes/radii.
                // therefore naively taking the SSDQs of them and making a quad out of them results in a trapezoid shape and not a box.
                var headCentre = headCircle.ScreenSpaceDrawQuad.Centre;
                var tailCentre = (tailCircle.ScreenSpaceDrawQuad.TopLeft + tailCircle.ScreenSpaceDrawQuad.BottomLeft) / 2;

                float headRadius = headCircle.ScreenSpaceDrawQuad.Height / 2;
                float tailRadius = tailCircle.ScreenSpaceDrawQuad.Height / 2;
                float radius = Math.Max(headRadius, tailRadius);

                var rectangle = new RectangleF(headCentre.X, headCentre.Y, tailCentre.X - headCentre.X, 0).Inflate(radius);
                return new Quad(rectangle.TopLeft, rectangle.TopRight, rectangle.BottomLeft, rectangle.BottomRight);
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => ScreenSpaceDrawQuad.Contains(screenSpacePos);

        private LegacyCirclePiece headCircle = null!;

        private Sprite body = null!;

        private Sprite tailCircle = null!;

        public LegacyDrumRoll()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                tailCircle = new Sprite
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Texture = skin.GetTexture("taiko-roll-end", WrapMode.ClampToEdge, WrapMode.ClampToEdge),
                    FillMode = FillMode.Fit,
                },
                body = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = skin.GetTexture("taiko-roll-middle", WrapMode.ClampToEdge, WrapMode.ClampToEdge),
                },
                headCircle = new LegacyCirclePiece
                {
                    RelativeSizeAxes = Axes.Y,
                },
            };

            AccentColour = colours.YellowDark;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAccentColour();
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (value == accentColour)
                    return;

                accentColour = value;
                if (IsLoaded)
                    updateAccentColour();
            }
        }

        private void updateAccentColour()
        {
            var colour = LegacyColourCompatibility.DisallowZeroAlpha(accentColour);

            headCircle.AccentColour = colour;
            body.Colour = colour;
            tailCircle.Colour = colour;
        }
    }
}
