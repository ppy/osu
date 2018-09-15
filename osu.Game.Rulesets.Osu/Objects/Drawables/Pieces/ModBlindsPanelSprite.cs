// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class ModBlindsPanelSprite : Sprite
    {
        public ModBlindsPanelSprite()
        {
            RelativeSizeAxes = Axes.None;
            Anchor = Anchor.TopLeft;
        }

        protected override void Update()
        {
            Height = Parent?.DrawHeight ?? 0;
            if (Height == 0 || Texture is null)
                Width = 0;
            else
                Width = Texture.Width / (float)Texture.Height * Height;
        }
    }
}
