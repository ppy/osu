// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class MessagePlaceholder : Placeholder
    {
        private readonly string message;

        public MessagePlaceholder(string message)
        {
            Direction = FillDirection.Horizontal;
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    Icon = FontAwesome.fa_exclamation_circle,
                    Size = new Vector2(26),
                    Margin = new MarginPadding { Right = 10 },
                },
                new OsuSpriteText
                {
                    Text = this.message = message,
                    TextSize = 22,
                },
            };
        }

        public override bool Equals(Placeholder other) => (other as MessagePlaceholder)?.message == message;
    }
}
