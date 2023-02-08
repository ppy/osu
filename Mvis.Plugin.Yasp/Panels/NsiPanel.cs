using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin;
using osu.Game.Screens.LLin.Misc;
using osuTK;
using Color4 = osuTK.Graphics.Color4;

#nullable disable

namespace Mvis.Plugin.Yasp.Panels
{
    public partial class NsiPanel : CompositeDrawable, IPanel
    {
        private CoverContainer beatmapBackground;
        private Container metadataContainer;

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();
        private readonly Bindable<bool> displayUnicode = new BindableBool();
        private Container coverAnimContainer;
        private Box bgBox;
        private OsuSpriteText artistDisplay;
        private OsuSpriteText sourceDisplay;
        private OsuTextFlowContainer titleDisplay;

        [Resolved]
        private IImplementLLin player { get; set; }

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            frameworkConfig.BindWith(FrameworkSetting.ShowUnicode, displayUnicode);

            var shear = new Vector2(0.25f, 0);

            RelativeSizeAxes = Axes.Both;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    Colour = Color4.Black.Opacity(0.1f),
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                coverAnimContainer = new Container
                {
                    Name = "Cover Anim Container",
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Depth = -1,
                    X = -1
                },
                metadataContainer = new Container
                {
                    Name = "Metadata Container",
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 50, Top = 50 },
                    Width = 0.45f,

                    X = 1,

                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5),

                            Shear = shear * 0.7f,

                            Children = new[]
                            {
                                artistDisplay = new OsuSpriteText
                                {
                                    Text = "Artist display",
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 30)
                                },
                                sourceDisplay = new OsuSpriteText
                                {
                                    Text = "Source display",
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 30)
                                }
                            }
                        },
                        titleDisplay = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 70))
                        {
                            Text = "Title display",
                            TextAnchor = Anchor.BottomRight,

                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.X,
                            RelativePositionAxes = Axes.Y,
                            AutoSizeAxes = Axes.Y,

                            Y = -0.05f,

                            Shear = shear * 0.7f,
                        }
                    }
                }
            };

            displayUnicode.BindValueChanged(_ => updateMetaText(beatmap.Value?.Metadata));
        }

        public void Refresh(WorkingBeatmap beatmap)
        {
            this.Delay(2).Schedule(() =>
            {
                this.beatmap.Value = beatmap;
                var meta = beatmap.Metadata;

                updateMetaText(meta);

                if (IsPresent)
                {
                    var workingBeatmap = this.beatmap.Value;
                    LoadComponentAsync(new CoverContainer(workingBeatmap), newCover =>
                    {
                        var prevBackground = beatmapBackground;

                        newCover.X = -0.1f;

                        coverAnimContainer.Add(newCover);

                        prevBackground?.MoveToX(-0.1f, 450, Easing.OutQuint).FadeOut(450, Easing.OutQuint).Expire();
                        newCover.MoveToX(0, 450, Easing.OutQuint).FadeInFromZero(450, Easing.OutQuint);

                        beatmapBackground = newCover;
                    });
                }
            });
        }

        private void updateMetaText(BeatmapMetadata meta)
        {
            meta ??= new BeatmapMetadata();

            artistDisplay.Text = displayUnicode.Value ? meta.GetArtist() : meta.Artist;
            sourceDisplay.Text = string.IsNullOrEmpty(meta.Source)
                ? displayUnicode.Value
                    ? meta.Title
                    : meta.GetTitle()
                : meta.Source;

            titleDisplay.Text = displayUnicode.Value ? meta.GetTitle() : meta.Title;
        }

        public override void Hide()
        {
            const int duration = 450;

            coverAnimContainer.MoveToX(-1, duration, Easing.OutCubic);
            metadataContainer.MoveToX(1, duration, Easing.OutCubic);
            bgBox.FadeOut(duration, Easing.OutCubic);

            this.FadeTo(0.99f, duration).Then().FadeOut();
        }

        public override void Show()
        {
            this.FadeIn();
            const int duration = 450;

            coverAnimContainer.MoveToX(0, duration, Easing.OutQuint);
            metadataContainer.MoveToX(0, duration, Easing.OutQuint);
            bgBox.FadeIn(duration, Easing.OutQuint);

            Schedule(beatmap.TriggerChange);
        }

        private partial class CoverContainer : CompositeDrawable
        {
            private readonly WorkingBeatmap beatmap;
            private static readonly Vector2 shear = new Vector2(0.25f, 0);

            public CoverContainer(WorkingBeatmap beatmap)
            {
                this.beatmap = beatmap;

                Name = "Beatmap Cover Container";
                RelativeSizeAxes = Axes.Both;
                RelativePositionAxes = Axes.Both;

                Width = 0.45f;
                Masking = true;
                Shear = shear;
                Depth = -1;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.4f),
                    Radius = 30
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new BeatmapCover(beatmap)
                    {
                        Shear = -shear,
                        TimeBeforeWrapperLoad = 0,
                        BackgroundBox = true,
                        Colour = Color4Extensions.FromHex("#dddddd"),
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        NoFadeIn = true
                    }
                };
            }
        }
    }
}
