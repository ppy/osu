// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class EpilepsyWarning : VisibilityContainer
    {
        private readonly BindableDouble trackVolumeOnEpilepsyWarning = new BindableDouble(1f);

        private Track track;
        private FillFlowContainer warningContent;

        public EpilepsyWarning()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0f;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IBindable<WorkingBeatmap> beatmap)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                warningContent = new FillFlowContainer
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
                            tfc.AddText("This beatmap contains scenes with ");
                            tfc.AddText("rapidly flashing colours", s =>
                            {
                                s.Font = s.Font.With(weight: FontWeight.Bold);
                                s.Colour = colours.Yellow;
                            });
                            tfc.AddText(".");

                            tfc.NewParagraph();
                            tfc.AddText("Please take caution if you are affected by epilepsy.");
                        }),
                    }
                }
            };

            track = beatmap.Value.Track;
            track.AddAdjustment(AdjustableProperty.Volume, trackVolumeOnEpilepsyWarning);
        }

        protected override void PopIn()
        {
            this.FadeIn(500, Easing.InQuint)
                .TransformBindableTo(trackVolumeOnEpilepsyWarning, 0.25, 500, Easing.InQuint);

            warningContent.FadeIn(500, Easing.InQuint);
        }

        protected override void PopOut()
            => this.FadeOut(500, Easing.OutQuint)
                   .TransformBindableTo(trackVolumeOnEpilepsyWarning, 1, 500, Easing.OutQuint);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            track?.RemoveAdjustment(AdjustableProperty.Volume, trackVolumeOnEpilepsyWarning);
        }
    }
}
