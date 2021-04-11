using System;
using Mvis.Plugin.RulesetPanel.Objects.Helpers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Collections.Interface;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Objects
{
    public class UpdateableBeatmapBackground : CurrentBeatmapProvider
    {
        private const int animation_duration = 500;

        private readonly Container backgroundContainer;
        private readonly Container nameContainer;
        private readonly MusicIntensityController intensityController;

        private BeatmapCover.Cover background;
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
            LoadComponentAsync(new BeatmapCover.Cover(beatmap.NewValue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0
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
            private readonly OsuSpriteText titleText;
            private ILocalisedBindableString title;
            private readonly WorkingBeatmap beatmap;

            [Resolved]
            private LocalisationManager localisation { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                title = localisation.GetLocalisedString(new RomanisableString(beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title));
                title.BindValueChanged(v =>
                {
                    titleText.Text = getShortTitle(v.NewValue);
                }, true);
            }

            public BeatmapName(WorkingBeatmap beatmap = null)
            {
                RelativeSizeAxes = Axes.Both;
                RelativePositionAxes = Axes.Y;

                if (beatmap == null)
                    return;

                this.beatmap = beatmap;

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
                            Text = new RomanisableString(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist),
                            Shadow = false
                        },
                        titleText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold),
                            Shadow = false
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

                if (newTitle.EndsWith(" ", StringComparison.Ordinal))
                    newTitle = newTitle.Substring(0, newTitle.Length - 1);

                return newTitle;
            }

            private static readonly char[] title_chars =
            {
                '(',
                '-',
                '~'
            };
        }
    }
}
