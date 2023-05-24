// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Edit.Setup;

namespace osu.Game.Rulesets.Osu.Edit.Setup
{
    public partial class OsuSetupSection : RulesetSetupSection
    {
        private LabelledSliderBar<float> stackLeniency;

        public OsuSetupSection()
            : base(new OsuRuleset().RulesetInfo)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new[]
            {
                stackLeniency = new LabelledSliderBar<float>
                {
                    Label = "Stack Leniency",
                    Description = "In play mode, osu! automatically stacks notes which occur at the same location. Increasing this value means it is more likely to snap notes of further time-distance.",
                    Current = new BindableFloat(Beatmap.BeatmapInfo.StackLeniency)
                    {
                        Default = 0.7f,
                        MinValue = 0,
                        MaxValue = 1,
                        Precision = 0.1f
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            stackLeniency.Current.BindValueChanged(_ => updateBeatmap());
        }

        private void updateBeatmap()
        {
            Beatmap.BeatmapInfo.StackLeniency = stackLeniency.Current.Value;
            Beatmap.SaveState();
        }
    }
}
