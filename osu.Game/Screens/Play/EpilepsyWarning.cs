// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using osuTK;

namespace osu.Game.Screens.Play
{
    public class EpilepsyWarning : VisibilityContainer
    {
        public const double FADE_DURATION = 250;

        public EpilepsyWarning()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0f;
        }

        private BackgroundScreenBeatmap dimmableBackground;

        public BackgroundScreenBeatmap DimmableBackground
        {
            get => dimmableBackground;
            set
            {
                dimmableBackground = value;

                if (IsLoaded)
                    updateBackgroundFade();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
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
        }

        protected override void PopIn()
        {
            updateBackgroundFade();

            this.FadeIn(FADE_DURATION, Easing.OutQuint);
        }

        private void updateBackgroundFade()
        {
            DimmableBackground?.FadeColour(OsuColour.Gray(0.5f), FADE_DURATION, Easing.OutQuint);
        }

        protected override void PopOut() => this.FadeOut(FADE_DURATION);
    }
}
