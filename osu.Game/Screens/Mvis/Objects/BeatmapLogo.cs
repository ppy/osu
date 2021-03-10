using System.Collections.Generic;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Mvis.Objects.Helpers;
using osu.Game.Screens.Mvis.Objects.MusicVisualizers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Objects
{
    public class BeatmapLogo : CurrentBeatmapProvider
    {
        private const int radius = 350;

        [Resolved(canBeNull: true)]
        private MConfigManager config { get; set; }

        private readonly Bindable<bool> useOsuLogoVisuals = new Bindable<bool>();
        private readonly Bindable<int> visuals = new Bindable<int>(3);
        private readonly Bindable<double> barWidth = new Bindable<double>(3.0);
        private readonly Bindable<int> barCount = new Bindable<int>(120);
        private readonly Bindable<int> rotation = new Bindable<int>(0);

        private readonly Bindable<bool> useCustomColour = new Bindable<bool>();
        private readonly Bindable<int> red = new Bindable<int>(0);
        private readonly Bindable<int> green = new Bindable<int>(0);
        private readonly Bindable<int> blue = new Bindable<int>(0);

        private CircularProgress progressGlow;
        private GlowEffect glow;
        private Container placeholder;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            Size = new Vector2(radius);

            InternalChildren = new Drawable[]
            {
                visualisation = new MenuLogoVisualisation
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.White,
                    Alpha = 0,
                },
                placeholder = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                updateableBg = new UpdateableBeatmapBackground
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

            config.BindWith(MSetting.MvisUseOsuLogoVisualisation, useOsuLogoVisuals);
            config?.BindWith(MSetting.MvisVisualizerAmount, visuals);
            config?.BindWith(MSetting.MvisBarWidth, barWidth);
            config?.BindWith(MSetting.MvisBarsPerVisual, barCount);
            config?.BindWith(MSetting.MvisRotation, rotation);

            config?.BindWith(MSetting.MvisRed, red);
            config?.BindWith(MSetting.MvisGreen, green);
            config?.BindWith(MSetting.MvisBlue, blue);
            config?.BindWith(MSetting.MvisUseCustomColour, useCustomColour);

            useOsuLogoVisuals.ValueChanged += _ => updateVisuals();
            barCount.BindValueChanged(_ => updateVisuals());
            visuals.BindValueChanged(_ => updateVisuals(), true);
            rotation.BindValueChanged(e => placeholder.Rotation = e.NewValue, true);

            red.BindValueChanged(_ => updateColour());
            green.BindValueChanged(_ => updateColour());
            blue.BindValueChanged(_ => updateColour());
            useCustomColour.BindValueChanged(_ => updateColour(), true);
        }

        private void updateColour()
        {
            if (!useCustomColour.Value)
            {
                progressGlow.Colour = Color4.White;
                glow.Colour = Color4.White;
                return;
            }

            progressGlow.FadeColour(new Colour4(red.Value / 255f, green.Value / 255f, blue.Value / 255f, 1));
            glow.Colour = new Colour4(red.Value / 255f, green.Value / 255f, blue.Value / 255f, 1);
        }

        private CancellationTokenSource cancellationTokenSource;
        private MenuLogoVisualisation visualisation;
        private UpdateableBeatmapBackground updateableBg;

        private void updateVisuals()
        {
            var drawableVisuals = new List<MusicCircularVisualizer>();

            var degree = 360f / visuals.Value;

            switch (useOsuLogoVisuals.Value)
            {
                case true:
                    placeholder.FadeOut(500, Easing.OutQuint);
                    visualisation.FadeIn(500, Easing.OutQuint);
                    break;

                case false:
                    placeholder.FadeIn(500, Easing.OutQuint);
                    visualisation.FadeOut(500, Easing.OutQuint);
                    break;
            }

            for (int i = 0; i < visuals.Value; i++)
            {
                drawableVisuals.Add(new MusicCircularVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = degree,
                    Rotation = i * degree,
                    BarWidth = (float)barWidth.Value,
                    BarsCount = barCount.Value,
                    CircleSize = radius,
                });
            }

            LoadComponentsAsync(drawableVisuals, loaded =>
            {
                placeholder.Clear();
                placeholder.AddRange(loaded);
            }, (cancellationTokenSource = new CancellationTokenSource()).Token);
        }

        public override void StopResponseOnBeatmapChanges()
        {
            updateableBg?.StopResponseOnBeatmapChanges();
        }

        public override void ResponseOnBeatmapChanges()
        {
            updateableBg?.ResponseOnBeatmapChanges();
        }

        protected override void Update()
        {
            base.Update();

            var track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : null;
            float progress = (track == null || track.IsDummyDevice) ? 0 : (float)(track.CurrentTime / track.Length);

            if (float.IsNaN(progress))
                progress = 0;

            progressGlow.Current.Value = progress;
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationTokenSource?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
