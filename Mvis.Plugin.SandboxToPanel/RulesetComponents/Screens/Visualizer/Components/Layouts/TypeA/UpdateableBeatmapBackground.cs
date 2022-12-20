using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.MusicHelpers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts.TypeA
{
    public partial class UpdateableBeatmapBackground : CurrentBeatmapProvider
    {
        private const int animation_duration = 500;

        private readonly Container backgroundContainer;
        private readonly Container nameContainer;
        private readonly MusicIntensityController intensityController;

        private BeatmapBackground background;
        private BeatmapName name;

        public UpdateableBeatmapBackground()
        {
            AddRangeInternal(new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        backgroundContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1.2f),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        nameContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                    }
                },
                intensityController = new MusicIntensityController()
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            intensityController.Intensity.BindValueChanged(intensity =>
            {
                var adjustedIntensity = intensity.NewValue / 150;

                if (adjustedIntensity > 0.2f)
                    adjustedIntensity = 0.2f;

                var sizeDelta = 1.2f - adjustedIntensity;

                if (sizeDelta > backgroundContainer.Size.X)
                    return;

                backgroundContainer.ResizeTo(sizeDelta, 10, Easing.OutQuint).Then().ResizeTo(1.2f, 1500, Easing.OutQuint);
            }, true);
        }

        protected override void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            LoadComponentAsync(new BeatmapBackground(beatmap.NewValue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
                Colour = Color4.DarkGray
            }, newBackground =>
            {
                background?.FadeOut(animation_duration, Easing.OutQuint);
                background?.RotateTo(360, animation_duration, Easing.OutQuint);
                background?.Expire();

                background = newBackground;
                backgroundContainer.Add(newBackground);
                newBackground.RotateTo(360, animation_duration, Easing.OutQuint);
                newBackground.FadeIn(animation_duration, Easing.OutQuint);
            });

            LoadComponentAsync(new BeatmapName(beatmap.NewValue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Y = -1.2f,
                Depth = -float.MaxValue,
            }, newName =>
            {
                name?.MoveToY(1.2f, animation_duration, Easing.Out);
                name?.Expire();

                name = newName;
                nameContainer.Add(newName);
                newName.MoveToY(0, animation_duration, Easing.OutQuint);
            });
        }

        private partial class BeatmapName : CompositeDrawable
        {
            [Resolved(canBeNull: true)]
            private SandboxRulesetConfigManager config { get; set; }

            private readonly Bindable<int> radius = new Bindable<int>(350);
            private readonly Bindable<string> colour = new Bindable<string>("#ffffff");

            private readonly WorkingBeatmap beatmap;
            private TextFlowContainer artist;
            private TextFlowContainer title;

            public BeatmapName(WorkingBeatmap beatmap = null)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                RelativePositionAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 30 };

                if (beatmap == null)
                    return;

                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        artist = new TextFlowContainer(t =>
                        {
                            t.Font = OsuFont.GetFont(size: 28, weight: FontWeight.Bold);
                        })
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = beatmap.Metadata.Artist
                        },
                        title = new TextFlowContainer(t =>
                        {
                            t.Font = OsuFont.GetFont(size: 22, weight: FontWeight.SemiBold);
                        })
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = beatmap.Metadata.Title
                        }
                    }
                }.WithEffect(new BlurEffect
                {
                    Colour = Color4.Black.Opacity(0.7f),
                    DrawOriginal = true,
                    PadExtent = true,
                    Sigma = new Vector2(5)
                }));

                config?.BindWith(SandboxRulesetSetting.Radius, radius);
                config?.BindWith(SandboxRulesetSetting.TypeATextColour, colour);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                radius.BindValueChanged(r =>
                {
                    if (beatmap != null)
                        Scale = new Vector2(r.NewValue / 350f);
                }, true);

                colour.BindValueChanged(c => artist.Colour = title.Colour = Colour4.FromHex(c.NewValue), true);
            }
        }
    }
}
