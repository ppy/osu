// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;
using osuTK;

namespace osu.Game.Overlays.Changelog
{
    public partial class ChangelogSingleBuild : ChangelogContent
    {
        private APIChangelogBuild build;

        public ChangelogSingleBuild(APIChangelogBuild build)
        {
            this.build = build;
        }

        [BackgroundDependencyLoader]
        private void load(CancellationToken? cancellation, IAPIProvider api, OverlayColourProvider colourProvider)
        {
            bool complete = false;

            var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);
            req.Success += res =>
            {
                build = res;
                complete = true;
            };
            req.Failure += _ => complete = true;

            api.PerformAsync(req);

            while (!complete)
            {
                if (cancellation?.IsCancellationRequested == true)
                {
                    req.Cancel();
                    return;
                }

                Thread.Sleep(10);
            }

            if (build != null)
            {
                CommentsContainer comments;

                Children = new Drawable[]
                {
                    new ChangelogBuildWithNavigation(build) { SelectBuild = SelectBuild },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Colour = colourProvider.Background6,
                        Margin = new MarginPadding { Top = 30 },
                    },
                    new ChangelogSupporterPromo
                    {
                        Alpha = api.LocalUser.Value.IsSupporter ? 0 : 1,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Colour = colourProvider.Background6,
                        Alpha = api.LocalUser.Value.IsSupporter ? 0 : 1,
                    },
                    comments = new CommentsContainer()
                };

                comments.ShowComments(CommentableType.Build, build.Id);
            }
        }

        public partial class ChangelogBuildWithNavigation : ChangelogBuild
        {
            public ChangelogBuildWithNavigation(APIChangelogBuild build)
                : base(build)
            {
            }

            private OsuSpriteText date;

            protected override FillFlowContainer CreateHeader()
            {
                var fill = base.CreateHeader();

                var nestedFill = (FillFlowContainer)fill.Child;

                var buildDisplay = (OsuHoverContainer)nestedFill.Child;

                buildDisplay.Scale = new Vector2(1.25f);
                buildDisplay.Action = null;

                fill.Add(date = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = Build.CreatedAt.Date.ToLocalisableString("dd MMMM yyyy"),
                    Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 14),
                    Margin = new MarginPadding { Top = 5 },
                    Scale = new Vector2(1.25f),
                });

                nestedFill.Insert(-1, new NavigationIconButton(Build.Versions?.Previous)
                {
                    Icon = FontAwesome.Solid.ChevronLeft,
                    SelectBuild = b => SelectBuild(b)
                });
                nestedFill.Insert(1, new NavigationIconButton(Build.Versions?.Next)
                {
                    Icon = FontAwesome.Solid.ChevronRight,
                    SelectBuild = b => SelectBuild(b)
                });

                return fill;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                date.Colour = colourProvider.Light1;
            }
        }

        private partial class NavigationIconButton : IconButton
        {
            public Action<APIChangelogBuild> SelectBuild;

            public NavigationIconButton(APIChangelogBuild build)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                if (build == null) return;

                TooltipText = build.DisplayVersion;

                Action = () =>
                {
                    SelectBuild?.Invoke(build);
                    Enabled.Value = false;
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                HoverColour = colours.GreyVioletLight.Opacity(0.6f);
                FlashColour = colours.GreyVioletLighter;
            }
        }
    }
}
