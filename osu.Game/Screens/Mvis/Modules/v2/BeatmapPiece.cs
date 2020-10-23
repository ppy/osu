using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class BeatmapPiece : Container
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> b { get; set; }

        [Resolved]
        private MusicController controller { get; set; }

        public readonly BindableBool Active = new BindableBool();

        public readonly WorkingBeatmap beatmap;
        private Flash flash;
        private Box maskBox;
        public bool isCurrent;

        public BeatmapPiece(WorkingBeatmap b)
        {
            Masking = true;
            CornerRadius = 12.5f;
            BorderThickness = 3f;
            RelativeSizeAxes = Axes.X;
            Height = 80;

            beatmap = b;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes= Axes.Both,
                    Colour = Colour4.Gray
                },
                new BeatmapCover(beatmap)
                {
                    BackgroundBox = false
                },
                maskBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3.Opacity(0.65f)
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.75f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.3f,
                            Colour = colourProvider.Background3
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f,
                            Colour = ColourInfo.GradientHorizontal(
                                colourProvider.Background3,
                                colourProvider.Background3.Opacity(0.5f)
                            )
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.1f,
                            Colour = ColourInfo.GradientHorizontal(
                                colourProvider.Background3.Opacity(0.5f),
                                colourProvider.Background3.Opacity(0.2f)
                            )
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.1f,
                            Colour = ColourInfo.GradientHorizontal(
                                colourProvider.Background3.Opacity(0.2f),
                                colourProvider.Background3.Opacity(0)
                            )
                        }
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding{Left = 15},
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold)
                        },
                        new OsuSpriteText
                        {
                            Text = beatmap.Metadata.ArtistUnicode ?? beatmap.Metadata.Artist,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold)
                        }
                    }
                },
                flash = new Flash
                {
                    RelativeSizeAxes = Axes.Both
                }
            };

            Active.BindValueChanged(OnActiveChanged, true);
        }

        private class Flash : BeatSyncedContainer
        {
            private Box flashBox;
            public Flash()
            {
                Child = flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Colour = Colour4.White.Opacity(0.4f)
                };
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                if ( beatIndex % 4 == 0 || effectPoint.KiaiMode )
                    flashBox.FadeOutFromOne(1000);
            }
        }

        private void OnActiveChanged(ValueChangedEvent<bool> v)
        {
            switch (v.NewValue)
            {
                case true:
                    if ( isCurrent )
                            BorderColour = colourProvider.Highlight1;
                    else
                            BorderColour = Colour4.Gold;
                    maskBox.FadeOut(500);
                    flash.Show();
                    break;

                case false:
                    BorderColour = Colour4.Gray;
                    maskBox.FadeIn(500);
                    flash.Hide();
                    break;
            }
        }

        public void MakeActive() => Active.Value = true;
        public void InActive() => Active.Value = false;

        protected override bool OnClick(ClickEvent e)
        {
            if (isCurrent && b.Value != beatmap )
            {
                b.Value = beatmap;
                controller.Play();
            }
            return base.OnClick(e);
        }
    }
}