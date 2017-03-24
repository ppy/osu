// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A circle piece which is used to visualise Bash objects.
    /// </summary>
    public class BashCirclePiece : CirclePiece
    {
        private Sprite icon;

        protected override Framework.Graphics.Drawable CreateIcon()
        {
            return icon ?? (icon = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            AccentColour = colours.YellowDark;

            icon.Texture = textures.Get(@"Play/Taiko/bash-hit-inner");
        }
    }
}