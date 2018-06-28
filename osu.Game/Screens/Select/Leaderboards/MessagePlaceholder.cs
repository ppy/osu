// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class MessagePlaceholder : Placeholder
    {
        private readonly string message;

        public MessagePlaceholder(string message)
        {
            AddIcon(FontAwesome.fa_exclamation_circle, cp =>
            {
                cp.TextSize = TEXT_SIZE;
                cp.Padding = new MarginPadding { Right = 10 };
            });

            AddText(this.message = message);
        }

        public override bool Equals(Placeholder other) => (other as MessagePlaceholder)?.message == message;
    }
}
