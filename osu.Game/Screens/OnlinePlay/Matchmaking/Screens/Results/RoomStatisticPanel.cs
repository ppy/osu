// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Results
{
    public partial class RoomStatisticPanel : CompositeDrawable
    {
        private readonly Color4 backgroundColour = Color4.SaddleBrown;

        private readonly string text;
        private readonly MultiplayerRoomUser user;

        public RoomStatisticPanel(string text, MultiplayerRoomUser user)
        {
            this.text = text;
            this.user = user;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new CircularContainer
            {
                AutoSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding(10),
                        Text = $"{text}: {user.User?.Username}"
                    }
                }
            };
        }
    }
}
