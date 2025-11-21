// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    /// <summary>
    /// A circular player avatar used in matchmaking displays.
    /// Is part of a <see cref="PlayerPanel"/> but can also be used in isolation for a more ambient/decorative user display.
    /// </summary>
    public partial class MatchmakingAvatar : CompositeDrawable
    {
        public static readonly Vector2 SIZE = new Vector2(30);

        private readonly APIUser user;
        private readonly bool isOwnUser;

        public MatchmakingAvatar(APIUser user, bool isOwnUser = false)
        {
            this.user = user;
            this.isOwnUser = isOwnUser;

            Size = SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            if (isOwnUser)
            {
                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Child = new FastCircle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colour.Yellow,
                    }
                });
            }

            AddInternal(new Container
            {
                Padding = new MarginPadding(2),
                RelativeSizeAxes = Axes.Both,
                Child = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.LightSlateGray,
                        },
                        new ClickableAvatar(user, true)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                }
            });
        }
    }
}
