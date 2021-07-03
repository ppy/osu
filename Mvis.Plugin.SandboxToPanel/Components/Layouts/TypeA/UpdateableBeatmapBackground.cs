using Mvis.Plugin.Sandbox.Components.MusicHelpers;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.Sandbox.Components.Layouts.TypeA
{
    public class UpdateableBeatmapBackground : CurrentBeatmapProvider
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
                Colour = Color4.LightGray
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

        private class BeatmapName : CompositeDrawable
        {
            [Resolved(canBeNull: true)]
            private SandboxConfigManager config { get; set; }

            private readonly Bindable<int> radius = new Bindable<int>(350);

            private readonly WorkingBeatmap beatmap;

            public BeatmapName(WorkingBeatmap beatmap = null)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;
                RelativePositionAxes = Axes.Y;

                if (beatmap == null)
                    return;

                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 26, weight: FontWeight.SemiBold),
                            Text = new RomanisableString(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold),
                            Text = new RomanisableString(getShortTitle(beatmap.Metadata.TitleUnicode ?? string.Empty), getShortTitle(beatmap.Metadata.Title))
                        }
                    }
                }.WithEffect(new BlurEffect
                {
                    Colour = Color4.Black.Opacity(0.8f),
                    DrawOriginal = true,
                    PadExtent = true,
                    Sigma = new Vector2(5)
                }));

                config?.BindWith(SandboxSetting.Radius, radius);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                radius.BindValueChanged(r =>
                {
                    if (beatmap != null)
                        Scale = new Vector2(r.NewValue / 350f);
                }, true);
            }

            /// <summary>
            /// Trims additional info in brackets in beatmap title (if exists).
            /// </summary>
            /// <param name="longTitle">The title to trim.</param>
            /// <returns></returns>
            private static string getShortTitle(string longTitle)
            {
                var newTitle = longTitle;

                for (int i = 0; i < title_chars.Length; i++)
                {
                    if (newTitle.Contains(title_chars[i]))
                    {
                        var charIndex = newTitle.IndexOf(title_chars[i]);

                        if (charIndex != 0)
                            newTitle = newTitle.Substring(0, charIndex);
                    }
                }

                if (newTitle.EndsWith(" "))
                    newTitle = newTitle[0..^1];

                return newTitle;
            }

            private static readonly char[] title_chars = new[]
            {
                '(',
                '-',
                '~'
            };
        }
    }
}
