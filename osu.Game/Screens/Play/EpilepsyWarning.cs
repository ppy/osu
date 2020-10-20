// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using osuTK;

namespace osu.Game.Screens.Play
{
    public class EpilepsyWarning : VisibilityContainer
    {
        public const double FADE_DURATION = 500;

        private readonly BindableDouble trackVolumeOnEpilepsyWarning = new BindableDouble(1f);

        private Track track;

        public EpilepsyWarning()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0f;
        }

        public BackgroundScreenBeatmap DimmableBackground { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IBindable<WorkingBeatmap> beatmap)
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Colour = colours.Yellow,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Icon = FontAwesome.Solid.ExclamationTriangle,
                            Size = new Vector2(50),
                        },
                        new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 25))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }.With(tfc =>
                        {
                            tfc.AddText("警告: 该谱面包含高速闪光等可能会导致");
                            tfc.AddText("光敏性癫痫", s =>
                            {
                                s.Font = s.Font.With(weight: FontWeight.Bold);
                                s.Colour = colours.Yellow;
                            });
                            tfc.AddText("的因素存在");
                            tfc.AddText(".");

                            tfc.NewParagraph();
                            tfc.AddText("如果您受到了这些因素的影响而感到不适，请立即休息。");
                        }),
                    }
                }
            };

            track = beatmap.Value.Track;
            track.AddAdjustment(AdjustableProperty.Volume, trackVolumeOnEpilepsyWarning);
        }

        protected override void PopIn()
        {
            this.TransformBindableTo(trackVolumeOnEpilepsyWarning, 0.25, FADE_DURATION);

            DimmableBackground?.FadeColour(OsuColour.Gray(0.5f), FADE_DURATION, Easing.OutQuint);

            this.FadeIn(FADE_DURATION, Easing.OutQuint);
        }

        protected override void PopOut() => this.FadeOut(FADE_DURATION);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            track?.RemoveAdjustment(AdjustableProperty.Volume, trackVolumeOnEpilepsyWarning);
        }
    }
}
