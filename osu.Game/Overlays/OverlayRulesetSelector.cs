// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Overlays
{
    public class OverlayRulesetSelector : RulesetSelector
    {
        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                foreach (var i in TabContainer.Children.OfType<IHasAccentColour>())
                    i.AccentColour = value;
            }
        }

        protected OverlayColourScheme ColourScheme { get; }

        public OverlayRulesetSelector(OverlayColourScheme colourScheme)
        {
            ColourScheme = colourScheme;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.ForOverlayElement(ColourScheme, 1, 0.7f);
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new OverlayRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(25, 0),
        };
    }
}
