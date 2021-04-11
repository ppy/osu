using System.Collections.Generic;
using Mvis.Plugin.RulesetPanel.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers.Bars
{
    public class BasicBar : Container
    {
        protected virtual IEnumerable<Drawable> ColourReceptors => new[] { box };

        [Resolved]
        private RulesetPanelConfigManager config { get; set; }

        private readonly Bindable<bool> useCustomColour = new Bindable<bool>();
        private readonly Bindable<int> red = new Bindable<int>(0);
        private readonly Bindable<int> green = new Bindable<int>(0);
        private readonly Bindable<int> blue = new Bindable<int>(0);

        public BasicBar()
        {
            Child = CreateContent();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            config.BindWith(RulesetPanelSetting.Red, red);
            config.BindWith(RulesetPanelSetting.Green, green);
            config.BindWith(RulesetPanelSetting.Blue, blue);
            config.BindWith(RulesetPanelSetting.UseCustomColour, useCustomColour);

            red.BindValueChanged(_ => updateColour());
            green.BindValueChanged(_ => updateColour());
            blue.BindValueChanged(_ => updateColour());
            useCustomColour.BindValueChanged(_ => updateColour(), true);
        }

        private void updateColour()
        {
            if (!useCustomColour.Value)
            {
                foreach (var r in ColourReceptors)
                    r.Colour = Color4.White;

                return;
            }

            foreach (var r in ColourReceptors)
                r.FadeColour(new Colour4(red.Value / 255f, green.Value / 255f, blue.Value / 255f, 1));
        }

        private Box box;

        protected virtual Drawable CreateContent() => box = new Box
        {
            EdgeSmoothness = Vector2.One,
            RelativeSizeAxes = Axes.Both,
            Colour = Color4.White,
        };

        public virtual void SetValue(float amplitudeValue, float valueMultiplier, int softness)
        {
            var newHeight = ValueFormula(amplitudeValue, valueMultiplier);

            // Don't allow resize if new height less than current
            if (newHeight <= Height)
                return;

            this.ResizeHeightTo(newHeight).Then().ResizeHeightTo(0, softness);
        }

        protected virtual float ValueFormula(float amplitudeValue, float valueMultiplier) => amplitudeValue * valueMultiplier;
    }
}
