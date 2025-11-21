// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Overlays;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    public partial class MatchmakingBackgroundScreen : BackgroundScreen
    {
        public MatchmakingBackgroundScreen()
        {
            InternalChild = new Content
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        public partial class Content : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(TextureStore textures, OverlayColourProvider colourProvider)
            {
                InternalChild = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get("Backgrounds/bg1"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    Colour = colourProvider.Dark2
                };
            }
        }
    }
}
