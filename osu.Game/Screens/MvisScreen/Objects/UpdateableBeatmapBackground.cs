using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osuTK;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Effects;
using osu.Game.Screens.Mvis.UI.Objects.Helpers;
using osu.Framework.Localisation;

namespace osu.Game.Screens.Mvis.UI.Objects
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
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.25f)
                        },
                        nameContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
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
            public BeatmapName(WorkingBeatmap beatmap = null)
            {
                RelativeSizeAxes = Axes.Both;
                RelativePositionAxes = Axes.Y;

                if (beatmap == null)
                    return;

                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 26, weight: FontWeight.SemiBold),
                            Text = new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)),
                            Shadow = false,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold),
                            Text = new LocalisedString((beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title)),
                            Shadow = false,
                        }
                    }
                }.WithEffect(new BlurEffect
                {
                    Colour = Color4.Black.Opacity(0.7f),
                    DrawOriginal = true,
                    Sigma = new Vector2(5)
                }));
            }

            /// <summary>
            /// Trims additional info in brackets in beatmap title (if exists).
            /// </summary>
            /// <param name="longTitle">The title to trim.</param>
            /// <returns></returns>
            private string getShortTitle(string longTitle)
            {
                if (!longTitle.Contains("("))
                    return longTitle;

                var bracketIndex = longTitle.IndexOf('(');
                return longTitle.Substring(0, bracketIndex);
            }
        }
    }
}
