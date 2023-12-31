// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarRulesetTabButton : TabItem<RulesetInfo>
    {
        private readonly RulesetButton ruleset;

        public ToolbarRulesetTabButton(RulesetInfo value)
            : base(value)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Child = ruleset = new RulesetButton
            {
                Active = false,
                TooltipMainText = value.CreateInstance().Description
            };
            var rInstance = value.CreateInstance();

            ruleset.TooltipMain = rInstance.Description;
            ruleset.TooltipSub = ToolbarStrings.PlaySomeRuleset(rInstance.Description);
            ruleset.SetIcon(rInstance.CreateIcon());
        }

        protected override void OnActivated() => ruleset.Active = true;

        protected override void OnDeactivated() => ruleset.Active = false;

        private partial class RulesetButton : ToolbarButton
        {
            private Color4 rulesetcolour;

            protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds();

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public RulesetButton()
            {
                Padding = new MarginPadding(3)
                {
                    Bottom = 5
                };
            }
            public string TooltipMainText { get; set; } = "";

            public bool Active
            {
                set => Scheduler.AddOnce(() =>
                {
                    if (value)
                    {
                        string tooltipMainText = TooltipMainText;
                        if (tooltipMainText == "osu!")
                        {
                            rulesetcolour = colours.PinkLight;
                        }
                        else if (tooltipMainText == "osu!mania")
                        {
                            rulesetcolour = colours.Purple0;
                        }
                        else if (tooltipMainText == "osu!catch")
                        {
                            rulesetcolour = colours.Blue0;
                        }
                        else
                        {
                            rulesetcolour = colours.GreenLightest;
                        }
                        IconContainer.Colour = rulesetcolour;
                    }
                    else
                    {
                        IconContainer.Colour = colours.GrayF;
                        IconContainer.EdgeEffect = new EdgeEffectParameters();
                    }
                });
            }

            protected override bool OnClick(ClickEvent e)
            {
                Parent!.TriggerClick();
                return base.OnClick(e);
            }
        }
    }
}
