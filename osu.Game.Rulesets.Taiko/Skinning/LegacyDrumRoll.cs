// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class LegacyDrumRoll : Container, IHasAccentColour
    {
        protected override Container<Drawable> Content => content;

        private Container content;

        private LegacyCirclePiece headCircle;

        private Sprite body;

        private Sprite end;

        public LegacyDrumRoll()
        {
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        headCircle = new LegacyCirclePiece
                        {
                            Depth = float.MinValue,
                            RelativeSizeAxes = Axes.Y,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                        },
                        body = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = skin.GetTexture("taiko-roll-middle"),
                        },
                        end = new Sprite
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.Both,
                            Texture = skin.GetTexture("taiko-roll-end"),
                            FillMode = FillMode.Fit,
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAccentColour();
        }

        protected override void Update()
        {
            base.Update();

            var padding = Content.DrawHeight * Content.Width / 2;

            Content.Padding = new MarginPadding
            {
                Left = padding,
                Right = padding,
            };

            Width = Parent.DrawSize.X + DrawHeight;
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
            headCircle.AccentColour = accentColour;
            body.Colour = accentColour;
            end.Colour = accentColour;
        }
    }
}
