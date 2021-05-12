using System;
using Mvis.Plugin.RulesetPanel.Config;
using Mvis.Plugin.RulesetPanel.UI.Objects.Helpers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers
{
    public class MusicVisualizer : MusicAmplitudesProvider
    {
        [Resolved(canBeNull: true)]
        private RulesetPanelConfigManager config { get; set; }

        private readonly Bindable<int> visuals = new Bindable<int>(3);
        private readonly Bindable<double> barWidth = new Bindable<double>(1.0);
        private readonly Bindable<int> totalBarCount = new Bindable<int>(3500);
        private readonly Bindable<int> rotation = new Bindable<int>(0);
        private readonly Bindable<int> decay = new Bindable<int>(200);
        private readonly Bindable<int> multiplier = new Bindable<int>(400);
        private readonly Bindable<BarType> type = new Bindable<BarType>(BarType.Fall);
        private readonly Bindable<bool> symmetry = new Bindable<bool>(true);

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            config?.BindWith(RulesetPanelSetting.VisualizerAmount, visuals);
            config?.BindWith(RulesetPanelSetting.BarWidth, barWidth);
            config?.BindWith(RulesetPanelSetting.BarsPerVisual, totalBarCount);
            config?.BindWith(RulesetPanelSetting.Rotation, rotation);
            config?.BindWith(RulesetPanelSetting.BarType, type);
            config?.BindWith(RulesetPanelSetting.Decay, decay);
            config?.BindWith(RulesetPanelSetting.Multiplier, multiplier);
            config?.BindWith(RulesetPanelSetting.Symmetry, symmetry);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rotation.BindValueChanged(e => Rotation = e.NewValue + (symmetry.Value ? 180f / visuals.Value : 0));
            totalBarCount.BindValueChanged(_ => updateBarCount());
            visuals.BindValueChanged(_ => updateVisuals());
            symmetry.BindValueChanged(_ => updateVisuals());
            type.BindValueChanged(_ => updateVisuals(), true);
        }

        private void updateVisuals()
        {
            Clear();

            var degree = 360f / trueVisualsCount;

            for (int i = 0; i < trueVisualsCount; i++)
            {
                Add(createVisualizer().With(v =>
                {
                    v.Anchor = Anchor.Centre;
                    v.Origin = Anchor.Centre;
                    v.RelativeSizeAxes = Axes.Both;
                    v.Rotation = i * degree;
                    v.DegreeValue.Value = degree;
                    v.BarWidth.BindTo(barWidth);
                    v.Decay.BindTo(decay);
                    v.HeightMultiplier.BindTo(multiplier);

                    if (symmetry.Value)
                        v.Reversed.Value = i % 2 == 0;
                }));
            }

            updateBarCount();
            rotation.TriggerChange();
        }

        private MusicVisualizerDrawable createVisualizer()
        {
            switch (type.Value)
            {
                default:
                case BarType.Basic:
                    return new BasicMusicVisualizerDrawable();

                case BarType.Rounded:
                    return new RoundedMusicVisualizerDrawable();

                case BarType.Fall:
                    return new FallMusicVisualizerDrawable();

                case BarType.Dots:
                    return new DotsMusicVisualizerDrawable();
            }
        }

        private void updateBarCount()
        {
            var barsPerVis = (int)Math.Round((float)totalBarCount.Value / trueVisualsCount);

            foreach (var c in Children)
                ((MusicVisualizerDrawable)c).BarCount.Value = barsPerVis;
        }

        protected override void OnAmplitudesUpdate(float[] amplitudes)
        {
            foreach (var c in Children)
                ((MusicVisualizerDrawable)c).SetAmplitudes(amplitudes);
        }

        private int trueVisualsCount => visuals.Value * (symmetry.Value ? 2 : 1);
    }
}
