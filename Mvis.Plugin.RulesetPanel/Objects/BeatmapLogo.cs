using Mvis.Plugin.RulesetPanel.Config;
using Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers;
using Mvis.Plugin.RulesetPanel.UI.Objects.Helpers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Objects
{
    public class BeatmapLogo : CurrentBeatmapProvider
    {
        [Resolved]
        private RulesetPanelConfigManager config { get; set; }

        private readonly Bindable<bool> useCustomColour = new Bindable<bool>();
        private readonly Bindable<int> red = new Bindable<int>(0);
        private readonly Bindable<int> green = new Bindable<int>(0);
        private readonly Bindable<int> blue = new Bindable<int>(0);
        private readonly Bindable<int> radius = new Bindable<int>(350);

        private CircularProgress progressGlow;
        private GlowEffect glow;
        private MusicVisualizer visualizer;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                visualizer = new MusicVisualizer(),
                new UpdateableBeatmapBackground
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                (progressGlow = new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    InnerRadius = 0.02f,
                }).WithEffect(glow = new GlowEffect
                {
                    Colour = Color4.White,
                    Strength = 5,
                    PadExtent = true
                }),
            };

            config?.BindWith(RulesetPanelSetting.Red, red);
            config?.BindWith(RulesetPanelSetting.Green, green);
            config?.BindWith(RulesetPanelSetting.Blue, blue);
            config?.BindWith(RulesetPanelSetting.UseCustomColour, useCustomColour);
            config?.BindWith(RulesetPanelSetting.Radius, radius);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            radius.BindValueChanged(r =>
            {
                Size = new Vector2(r.NewValue);
                visualizer.Size = new Vector2(r.NewValue - 2);
            }, true);

            red.BindValueChanged(_ => updateColour());
            green.BindValueChanged(_ => updateColour());
            blue.BindValueChanged(_ => updateColour());
            useCustomColour.BindValueChanged(_ => updateColour(), true);
        }

        private void updateColour()
        {
            progressGlow.Colour = glow.Colour = visualizer.Colour =
                useCustomColour.Value ? new Color4(red.Value / 255f, green.Value / 255f, blue.Value / 255f, 1) : Color4.White;
        }

        protected override void Update()
        {
            base.Update();

            var track = Beatmap.Value?.Track;
            progressGlow.Current.Value = (track == null || track.Length == 0) ? 0 : (track.CurrentTime / track.Length);
        }
    }
}
