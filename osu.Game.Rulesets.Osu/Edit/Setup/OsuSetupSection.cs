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
                    Label = "堆栈指数",
                    Description = "游玩模式下, osu! 会自动将处于同一位置下的物件堆起来。 增加此值会使物件在一段时间内更容易重叠在一起。",
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
