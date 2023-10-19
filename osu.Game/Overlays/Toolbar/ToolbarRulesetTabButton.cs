// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
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
            protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds(HoverSampleSet.Button);

            public bool Active
            {
                set
                {
                    if (value)
                    {
                        IconContainer.Colour = Color4.White;
                        IconContainer.EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Colour = new Color4(255, 194, 224, 100),
                            Radius = 15,
                            Roundness = 15,
                        };
                    }
                    else
                    {
                        IconContainer.Colour = new Color4(255, 194, 224, 255);
                        IconContainer.EdgeEffect = new EdgeEffectParameters();
                    }
                }
            }

            protected override bool OnClick(ClickEvent e)
            {
                Parent!.TriggerClick();
                return base.OnClick(e);
            }
        }
    }
}
