// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogSingleBuild : ChangelogContent
    {
        private APIChangelogBuild build;

        public ChangelogSingleBuild(APIChangelogBuild build)
        {
            this.build = build;
        }

        [BackgroundDependencyLoader]
        private void load(CancellationToken? cancellation, IAPIProvider api)
        {
            bool complete = false;

            var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);
            req.Success += res =>
            {
                build = res;
                complete = true;
            };
            req.Failure += _ => complete = true;

            // This is done on a separate thread to support cancellation below
            Task.Run(() =>
            {
                try
                {
                    req.Perform(api);
                }
                catch
                {
                    complete = true;
                }
            });

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
                Children = new Drawable[]
                {
                    new ChangelogBuildWithNavigation(build) { SelectBuild = SelectBuild },
                    new Comments(build)
                };
        }

        public class ChangelogBuildWithNavigation : ChangelogBuild
        {
            public ChangelogBuildWithNavigation(APIChangelogBuild build)
                : base(build)
            {
            }

            protected override FillFlowContainer CreateHeader()
            {
                var fill = base.CreateHeader();

                foreach (var existing in fill.Children.OfType<OsuHoverContainer>())
                {
                    existing.Scale = new Vector2(1.25f);
                    existing.Action = null;

                    existing.Add(new OsuSpriteText
                    {
                        Text = Build.CreatedAt.Date.ToString("dd MMM yyyy"),
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 14),
                        Colour = OsuColour.FromHex(@"FD5"),
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = 5 },
                    });
                }

                fill.Insert(-1, new NavigationIconButton(Build.Versions?.Previous)
                {
                    Icon = FontAwesome.Solid.ChevronLeft,
                    SelectBuild = b => SelectBuild(b)
                });
                fill.Insert(1, new NavigationIconButton(Build.Versions?.Next)
                {
                    Icon = FontAwesome.Solid.ChevronRight,
                    SelectBuild = b => SelectBuild(b)
                });

                return fill;
            }
        }

        private class NavigationIconButton : IconButton
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
