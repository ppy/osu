using Mvis.Plugin.Sandbox.Components.MusicHelpers;
using Mvis.Plugin.Sandbox.Components.Visualizers;
using Mvis.Plugin.Sandbox.Components.Visualizers.Linear;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace Mvis.Plugin.Sandbox.Components.Layouts.TypeB
{
    public class TypeBVisualizerController : MusicAmplitudesProvider
    {
        private readonly Bindable<double> barWidth = new Bindable<double>();
        private readonly Bindable<int> barCount = new Bindable<int>();
        private readonly Bindable<int> multiplier = new Bindable<int>();
        private readonly Bindable<int> decay = new Bindable<int>();
        private readonly Bindable<int> smoothness = new Bindable<int>();
        private readonly Bindable<LinearBarType> type = new Bindable<LinearBarType>();
        private readonly BindableBool showBeatmapInfo = new BindableBool();

        private OsuSpriteText text;
        private Box progress;
        private Container<LinearMusicVisualizerDrawable> visualizerContainer;

        [BackgroundDependencyLoader]
        private void load(SandboxConfigManager config)
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
                                Width = 0,
                                Colour = Color4.White
                            }
                        }
                    },
                    text = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 30)
                    }
                }
            };

            config.BindWith(SandboxSetting.BarCountB, barCount);
            config.BindWith(SandboxSetting.BarWidthB, barWidth);
            config.BindWith(SandboxSetting.MultiplierB, multiplier);
            config.BindWith(SandboxSetting.DecayB, decay);
            config.BindWith(SandboxSetting.SmoothnessB, smoothness);
            config.BindWith(SandboxSetting.LinearBarType, type);
            config.BindWith(SandboxSetting.ShowBeatmapInfo, showBeatmapInfo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.BindValueChanged(b =>
            {
                text.Text = new RomanisableString(
                    $"{b.NewValue.Metadata.ArtistUnicode ?? b.NewValue.Metadata.Artist} - {b.NewValue.Metadata.TitleUnicode ?? b.NewValue.Metadata.Title}",
                    $"{b.NewValue.Metadata.Artist} - {b.NewValue.Metadata.Title}");
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

            showBeatmapInfo.BindValueChanged(v =>
            {
                if (v.NewValue) text.FadeIn(300, Easing.OutQuint);
                else text.FadeOut(300, Easing.OutQuint);
            }, true);
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
