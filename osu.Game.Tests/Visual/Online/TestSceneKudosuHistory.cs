// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Profile.Sections.Kudosu;
using System.Collections.Generic;
using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneKudosuHistory : OsuTestScene
    {
        private readonly Box background;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        public TestSceneKudosuHistory()
        {
            FillFlowContainer<DrawableKudosuHistoryItem> content;

            AddRange(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                content = new FillFlowContainer<DrawableKudosuHistoryItem>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.7f,
                    AutoSizeAxes = Axes.Y,
                }
            });

            items.ForEach(t => content.Add(new DrawableKudosuHistoryItem(t)));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Background4;
        }

        private readonly IEnumerable<APIKudosuHistory> items = new[]
        {
            new APIKudosuHistory
            {
                Amount = 10,
                CreatedAt = new DateTimeOffset(new DateTime(2011, 11, 11)),
                Source = KudosuSource.DenyKudosu,
                Action = KudosuAction.Reset,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 1",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username1",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 5,
                CreatedAt = new DateTimeOffset(new DateTime(2012, 10, 11)),
                Source = KudosuSource.Forum,
                Action = KudosuAction.Give,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 2",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username2",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 8,
                CreatedAt = new DateTimeOffset(new DateTime(2013, 9, 11)),
                Source = KudosuSource.Forum,
                Action = KudosuAction.Reset,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 3",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username3",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 7,
                CreatedAt = new DateTimeOffset(new DateTime(2014, 8, 11)),
                Source = KudosuSource.Forum,
                Action = KudosuAction.Revoke,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 4",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username4",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 100,
                CreatedAt = new DateTimeOffset(new DateTime(2015, 7, 11)),
                Source = KudosuSource.Vote,
                Action = KudosuAction.Give,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 5",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username5",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 20,
                CreatedAt = new DateTimeOffset(new DateTime(2016, 6, 11)),
                Source = KudosuSource.Vote,
                Action = KudosuAction.Reset,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 6",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username6",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 11,
                CreatedAt = new DateTimeOffset(new DateTime(2016, 6, 11)),
                Source = KudosuSource.AllowKudosu,
                Action = KudosuAction.Give,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 7",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username7",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 24,
                CreatedAt = new DateTimeOffset(new DateTime(2014, 6, 11)),
                Source = KudosuSource.Delete,
                Action = KudosuAction.Reset,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 8",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username8",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 12,
                CreatedAt = new DateTimeOffset(new DateTime(2016, 6, 11)),
                Source = KudosuSource.Restore,
                Action = KudosuAction.Give,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 9",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username9",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 2,
                CreatedAt = new DateTimeOffset(new DateTime(2012, 6, 11)),
                Source = KudosuSource.Recalculate,
                Action = KudosuAction.Give,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 10",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username10",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            },
            new APIKudosuHistory
            {
                Amount = 32,
                CreatedAt = new DateTimeOffset(new DateTime(2019, 8, 11)),
                Source = KudosuSource.Recalculate,
                Action = KudosuAction.Reset,
                Post = new APIKudosuHistory.ModdingPost
                {
                    Title = @"Random post 11",
                    Url = @"https://osu.ppy.sh/b/1234",
                },
                Giver = new APIKudosuHistory.KudosuGiver
                {
                    Username = @"Username11",
                    Url = @"https://osu.ppy.sh/u/1234"
                }
            }
        };
    }
}
