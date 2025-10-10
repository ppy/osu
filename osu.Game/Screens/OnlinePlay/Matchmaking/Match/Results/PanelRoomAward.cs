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
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results
{
    public partial class PanelRoomAward : CompositeDrawable
    {
        private readonly string text;
        private readonly int userId;

        public PanelRoomAward(string text, int userId)
        {
            this.text = text;
            this.userId = userId;

            Height = 40;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache, OverlayColourProvider colourProvider)
        {
            // Should be cached by this point.
            APIUser user = userLookupCache.GetUserAsync(userId).GetResultSafely()!;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background2,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Padding = new MarginPadding(10),
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new MatchmakingAvatar(user)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = text
                            }
                        }
                    },
                }
            };
        }
    }
}
