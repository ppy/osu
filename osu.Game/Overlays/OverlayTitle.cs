// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays
{
    public abstract class OverlayTitle : CompositeDrawable
    {
        private readonly OsuSpriteText title;
        private readonly Container icon;

        protected string Title
        {
            set => title.Text = value;
        }

        protected string IconTexture
        {
            set => icon.Child = new OverlayTitleIcon(value);
        }

        protected OverlayTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10, 0),
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    icon = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = 5 }, // compensates for osu-web sprites having around 5px of whitespace on each side
                        Size = new Vector2(30)
                    },
                    title = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular),
                        Margin = new MarginPadding { Vertical = 17.5f } // 15px padding + 2.5px line-height difference compensation
                    }
                }
            };
        }

        private class OverlayTitleIcon : Sprite
        {
            private readonly string textureName;

            public OverlayTitleIcon(string textureName)
            {
                this.textureName = textureName;

                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                FillMode = FillMode.Fit;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(textureName);
            }
        }
    }
}
