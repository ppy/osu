// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public partial class CardDetailsOverlayContainer
    {
        private partial class UserTagSection : CompositeDrawable
        {
            public IEnumerable<UserTag> Tags
            {
                set
                {
                    Debug.Assert(LoadState >= LoadState.Ready);

                    tagFlow.ChildrenEnumerable = value.Select(tag => new DrawableUserTag(tag));
                    this.FadeTo(tagFlow.Children.Count > 0 ? 1 : 0);
                }
            }

            private FillFlowContainer<DrawableUserTag> tagFlow = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding(10);

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children =
                    [
                        new OsuSpriteText
                        {
                            Text = "User Tags",
                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                        },
                        tagFlow = new FillFlowContainer<DrawableUserTag>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(4)
                        }
                    ]
                };
            }
        }

        private partial class DrawableUserTag(UserTag tag) : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 3;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children =
                    [
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Alpha = tag.GroupName != null ? 1 : 0,
                            Children =
                            [
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colour.Gray6,
                                },
                                new OsuSpriteText
                                {
                                    Text = tag.GroupName ?? "",
                                    Padding = new MarginPadding { Left = 5, Right = 3 },
                                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                }
                            ]
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Children =
                            [
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colour.Gray2,
                                },
                                new OsuSpriteText
                                {
                                    Text = tag.DisplayName,
                                    Padding = new MarginPadding { Left = 5, Right = 3 },
                                    Font = OsuFont.GetFont(size: 12),
                                }
                            ]
                        },
                    ]
                };
            }
        }
    }
}
