// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Online.Leaderboards
{
    public class MessagePlaceholder : Placeholder
    {
        private readonly string message;

        public MessagePlaceholder(string message)
        {
            AddIcon(FontAwesome.Solid.ExclamationCircle, cp =>
            {
                cp.Font = cp.Font.With(size: TEXT_SIZE);
                cp.Padding = new MarginPadding { Right = 10 };
            });

            AddText(this.message = message);
        }

        public override bool Equals(Placeholder other) => (other as MessagePlaceholder)?.message == message;
    }
}
