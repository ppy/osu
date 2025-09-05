// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Results
{
    public partial class RoomStatisticPanel : CompositeDrawable
    {
        private readonly Color4 backgroundColour = Color4.SaddleBrown;

        private readonly string text;
        private readonly int userId;

        public RoomStatisticPanel(string text, int userId)
        {
            this.text = text;
            this.userId = userId;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache)
        {
            // Should be cached by this point.
            APIUser? user = userLookupCache.GetUserAsync(userId).GetResultSafely();

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
                        Text = $"{text}: {user?.Username}"
                    }
                }
            };
        }
    }
}
