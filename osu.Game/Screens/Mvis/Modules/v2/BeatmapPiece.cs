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
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class BeatmapPiece : OsuClickableContainer
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> b { get; set; }

        [Resolved]
        private MusicController controller { get; set; }

        public readonly BindableBool Active = new BindableBool();
        public bool IsCurrent;

        public readonly WorkingBeatmap beatmap;
        private Flash flash;
        private Box maskBox;
        private Box hover;

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
            AddRangeInternal(new Drawable[]
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
                },
                hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White.Opacity(0.1f),
                    Alpha = 0
                }
            });

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

                switch(timingPoint.TimeSignature)
                {
                    case TimeSignatures.SimpleQuadruple:
                        if ( beatIndex % 4 == 0 || effectPoint.KiaiMode )
                            flashBox.FadeOutFromOne(1000);
                        break;

                    case TimeSignatures.SimpleTriple:
                        if ( beatIndex % 3 == 0 || effectPoint.KiaiMode )
                            flashBox.FadeOutFromOne(1000);
                        break;
                }
            }
        }

        private void OnActiveChanged(ValueChangedEvent<bool> v)
        {
            switch (v.NewValue)
            {
                case true:
                    if ( IsCurrent )
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
        public void TriggerActiveChange() => Active.TriggerChange();

        protected override bool OnClick(ClickEvent e)
        {
            if (IsCurrent && b.Value != beatmap )
            {
                b.Value = beatmap;
                controller.Play();
            }
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeIn(250);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            hover.FadeOut(250);
        }
    }
}