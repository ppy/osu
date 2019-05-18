// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Changelog.Components;
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
            var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);
            bool complete = false;

            req.Success += res =>
            {
                build = res;
                complete = true;
            };

            req.Failure += _ => complete = true;

            api.Queue(req);

            while (!complete && cancellation?.IsCancellationRequested != true)
                Task.Delay(1);

            Children = new Drawable[]
            {
                new ChangelogBuildWithNavigation(build) { SelectBuild = SelectBuild },
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
                        // do we need .ToUniversalTime() here?
                        // also, this should be a temporary solution to weekdays in >localized< date strings
                        Text = Build.CreatedAt.Date.ToLongDateString().Replace(Build.CreatedAt.ToString("dddd") + ", ", ""),
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 14),
                        Colour = OsuColour.FromHex(@"FD5"),
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = 5 },
                    });
                }

                TooltipIconButton left, right;

                fill.AddRange(new[]
                {
                    left = new TooltipIconButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.ChevronLeft,
                        Size = new Vector2(24),
                        TooltipText = Build.Versions?.Previous?.DisplayVersion,
                        IsEnabled = Build.Versions?.Previous != null,
                        Action = () => { SelectBuild?.Invoke(Build.Versions.Previous); },
                    },
                    right = new TooltipIconButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.ChevronRight,
                        Size = new Vector2(24),
                        TooltipText = Build.Versions?.Next?.DisplayVersion,
                        IsEnabled = Build.Versions?.Next != null,
                        Action = () => { SelectBuild?.Invoke(Build.Versions.Next); },
                    },
                });

                fill.SetLayoutPosition(left, -1);
                fill.SetLayoutPosition(right, 1);

                return fill;
            }
        }
    }
}
