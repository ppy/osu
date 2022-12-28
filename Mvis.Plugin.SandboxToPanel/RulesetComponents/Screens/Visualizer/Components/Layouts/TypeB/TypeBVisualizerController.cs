using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.MusicHelpers;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Visualizers;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Visualizers.Linear;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

#nullable disable

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts.TypeB
{
    public partial class TypeBVisualizerController : MusicAmplitudesProvider
    {
        private readonly Bindable<double> barWidth = new Bindable<double>();
        private readonly Bindable<int> barCount = new Bindable<int>();
        private readonly Bindable<int> multiplier = new Bindable<int>();
        private readonly Bindable<int> decay = new Bindable<int>();
        private readonly Bindable<int> smoothness = new Bindable<int>();
        private readonly Bindable<LinearBarType> type = new Bindable<LinearBarType>();
        private readonly Bindable<string> colour = new Bindable<string>("#ffffff");
        private readonly Bindable<string> progressColour = new Bindable<string>("#ffffff");
        private readonly Bindable<string> textColour = new Bindable<string>("#ffffff");

        private OsuSpriteText text;
        private Box progress;
        private Container<LinearMusicVisualizerDrawable> visualizerContainer;

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager config)
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding(50);
            Child = new FillFlowContainer
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Width = 500,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    visualizerContainer = new Container<LinearMusicVisualizerDrawable>
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 200
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                                Alpha = 0.3f
                            },
                            progress = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0
                            }
                        }
                    },
                    text = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 30)
                    }
                }
            };

            config.BindWith(SandboxRulesetSetting.BarCountB, barCount);
            config.BindWith(SandboxRulesetSetting.BarWidthB, barWidth);
            config.BindWith(SandboxRulesetSetting.MultiplierB, multiplier);
            config.BindWith(SandboxRulesetSetting.DecayB, decay);
            config.BindWith(SandboxRulesetSetting.SmoothnessB, smoothness);
            config.BindWith(SandboxRulesetSetting.LinearBarType, type);

            config?.BindWith(SandboxRulesetSetting.TypeBColour, colour);
            config?.BindWith(SandboxRulesetSetting.TypeBProgressColour, progressColour);
            config?.BindWith(SandboxRulesetSetting.TypeBTextColour, textColour);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.BindValueChanged(b =>
            {
                text.Text = $"{b.NewValue.Metadata.Artist} - {b.NewValue.Metadata.Title}";
            }, true);

            type.BindValueChanged(t =>
            {
                LinearMusicVisualizerDrawable drawable;

                switch (t.NewValue)
                {
                    default:
                    case LinearBarType.Basic:
                        drawable = new BasicLinearMusicVisualizerDrawable();
                        break;

                    case LinearBarType.Rounded:
                        drawable = new RoundedLinearMusicVisualizerDrawable();
                        break;
                }

                visualizerContainer.Child = drawable.With(d =>
                {
                    d.BarAnchorBindable.Value = BarAnchor.Bottom;
                    d.BarWidth.BindTo(barWidth);
                    d.BarCount.BindTo(barCount);
                    d.HeightMultiplier.BindTo(multiplier);
                    d.Decay.BindTo(decay);
                    d.Smoothness.BindTo(smoothness);
                });
            }, true);

            colour.BindValueChanged(c => visualizerContainer.Colour = Colour4.FromHex(c.NewValue), true);
            progressColour.BindValueChanged(c => progress.Colour = Colour4.FromHex(c.NewValue), true);
            textColour.BindValueChanged(c => text.Colour = Colour4.FromHex(c.NewValue), true);
        }

        protected override void Update()
        {
            base.Update();

            var track = Beatmap.Value?.Track;
            progress.Width = (float)((track == null || track.Length == 0) ? 0 : (track.CurrentTime / track.Length));
        }

        protected override void OnAmplitudesUpdate(float[] amplitudes)
        {
            visualizerContainer.Child?.SetAmplitudes(amplitudes);
        }
    }
}
