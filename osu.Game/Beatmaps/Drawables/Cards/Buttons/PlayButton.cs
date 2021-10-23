// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class PlayButton : OsuHoverContainer
    {
        public PlayButton()
        {
            Anchor = Origin = Anchor.Centre;

            Child = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Solid.Play,
                Size = new Vector2(14)
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HoverColour = colours.Yellow;
        }
    }
}
