// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Users
{
    public partial class RecentlyFavouriters : Container
    {
        private readonly Bindable<IReadOnlyList<APIUser>> users = new Bindable<IReadOnlyList<APIUser>>(new List<APIUser>());
        private readonly BindableInt allFavoritesCountBindable = new BindableInt();

        private readonly FillFlowContainer avatarFlow;
        private readonly OsuSpriteText text;
        private readonly Box backgroundBox;

        public RecentlyFavouriters()
        {
            AutoSizeAxes = Axes.Both;

            avatarFlow = new FillFlowContainer
            {
                Direction = FillDirection.Full,
                Spacing = new Vector2(2, 2),
                Padding = new MarginPadding(6),
            };

            Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    backgroundBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.8f,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(6f),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(2f),
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Icon = FontAwesome.Solid.Heart,
                                        Shadow = true,
                                        Size = new Vector2(12),
                                    },
                                    text = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                    },
                                }
                            },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            backgroundBox.Colour = colourProvider.Background4;
        }

        protected override bool OnHover(HoverEvent e)
        {
            backgroundBox.FadeTo(0.6f, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundBox.FadeTo(0.8f, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            users.BindValueChanged(_ => rebuild(), true);
            allFavoritesCountBindable.BindValueChanged(_ => rebuild(), true);

            FinishTransforms(true);
        }

        public void SetData(IReadOnlyList<APIUser> recentUsers, int allFavoritesCount)
        {
            users.Value = recentUsers;
            allFavoritesCountBindable.Value = allFavoritesCount;
        }

        private void rebuild()
        {
            int visibleCount = users.Value?.Count ?? 0;
            int totalCount = allFavoritesCountBindable.Value;

            text.Text = totalCount.ToString();

            avatarFlow.Clear();

            if (users.Value == null || users.Value.Count == 0)
            {
                avatarFlow.Add(new OsuSpriteText
                {
                    Text = "No recent favouriters",
                    Font = OsuFont.GetFont(size: 12),
                    Alpha = 0.8f
                });

                return;
            }

            var avatars = users.Value
                               .Take(50)
                               .Select(u => new ClickableAvatar(u, true).With(avatar =>
                               {
                                   avatar.Size = new Vector2(30);
                                   avatar.CornerRadius = 5;
                                   avatar.Masking = true;
                               }))
                               .ToArray();

            avatarFlow.AddRange(avatars);

            if (visibleCount < totalCount)
            {
                avatarFlow.Add(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Child = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 4 },
                        Text = $"+ {totalCount - visibleCount} others!",
                        Font = OsuFont.GetFont(size: 12),
                        Alpha = 0.85f
                    }
                });
            }

            float width = visibleCount <= 10 ? visibleCount * 34 : 330;
            avatarFlow.Width = width;
        }
    }
}
