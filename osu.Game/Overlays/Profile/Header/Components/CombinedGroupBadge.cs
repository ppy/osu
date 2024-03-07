// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class CombinedGroupBadge : Container, IHasTooltip
    {
        public LocalisableString TooltipText { get; }

        public int DotSize { get; set; } = 8;

        private readonly APIUserGroup[] groups;

        public CombinedGroupBadge(APIUserGroup[] groups)
        {
            this.groups = groups;

            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 8;

            TooltipText = string.Join(", ", groups.Select(g => g.Name));
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider? colourProvider, RulesetStore rulesets)
        {
            FillFlowContainer innerContainer;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider?.Background6 ?? Colour4.Black
                },
                innerContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Padding = new MarginPadding { Vertical = 4, Horizontal = 4 },
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2)
                }
            });

            foreach (var group in groups)
            {
                innerContainer.Add(new CircularContainer
                {
                    Size = new Vector2(DotSize),
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex(group.Colour ?? Colour4.White.ToHex())
                    }
                });
            }
        }
    }
}
