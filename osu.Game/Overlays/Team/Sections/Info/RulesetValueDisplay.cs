// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.Team.Sections.Info
{
    public partial class RulesetValueDisplay : CompositeDrawable
    {
        private readonly OsuSpriteText title;
        private readonly ConstrainedIconContainer iconContainer;
        private readonly OsuSpriteText rulesetName;

        private RulesetInfo? ruleset;

        public LocalisableString Title
        {
            set => title.Text = value;
        }

        public RulesetInfo? Ruleset
        {
            get => ruleset;
            set
            {
                if (ruleset != null && ruleset.Equals(value))
                    return;

                ruleset = value;
                Scheduler.AddOnce(updateRuleset);
            }
        }

        public RulesetValueDisplay()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    title = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 12),
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(2, 0),
                        Children = new Drawable[]
                        {
                            iconContainer = new ConstrainedIconContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(10),
                            },
                            rulesetName = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(size: 12),
                            }
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            title.Colour = colourProvider.Content1;
            rulesetName.Colour = colourProvider.Content2;
        }

        private void updateRuleset()
        {
            if (ruleset == null)
                return;

            rulesetName.Text = ruleset.Name;
            iconContainer.Icon = ruleset.CreateInstance().CreateIcon();
        }
    }
}
