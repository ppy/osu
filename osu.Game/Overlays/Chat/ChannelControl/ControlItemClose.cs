// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.Chat.ChannelControl
{
    public class ControlItemClose : OsuAnimatedButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            Alpha = 0f;
            Size = new Vector2(20);
            Add(new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.75f),
                Icon = FontAwesome.Solid.TimesCircle,
                RelativeSizeAxes = Axes.Both,
                Colour = osuColour.Red1,
            });
        }
    }
}
