// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class BasicSearchTextBox : SearchTextBox
    {
        public BasicSearchTextBox()
        {
            Add(new SpriteIcon
            {
                Icon = FontAwesome.Solid.Search,
                Origin = Anchor.CentreRight,
                Anchor = Anchor.CentreRight,
                Margin = new MarginPadding { Right = 10 },
                Size = new Vector2(20),
            });

            TextFlow.Padding = new MarginPadding { Right = 35 };
        }
    }
}
