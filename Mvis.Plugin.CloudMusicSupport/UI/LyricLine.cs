using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.UI
{
    public class LyricLine : CompositeDrawable
    {
        private OsuSpriteText currentLine;
        private OsuSpriteText currentLineTranslated;
        private string currentRawText;
        private string currentRawTranslateText;
        private bool alwaysHideBox;
        private Container boxContainer;

        public float FadeOutDuration = 200;
        private Easing fadeOutEasing => Easing.OutQuint;
        private Easing fadeInEasing => Easing.OutQuint;

        public float FadeInDuration = 200;

        public bool AlwaysHideBox
        {
            get => alwaysHideBox;
            set
            {
                alwaysHideBox = value;

                if (value)
                    boxContainer.FadeOut(300, Easing.OutQuint);
                else
                    boxContainer.FadeIn(300, Easing.OutQuint);
            }
        }

        public string Text
        {
            get => currentRawText;
            set
            {
                if (currentLine != null)
                {
                    if (value == currentLine.Text) return;

                    currentLine?.MoveToY(5, FadeOutDuration, fadeOutEasing)
                               .FadeOut(FadeOutDuration, fadeOutEasing).Then().Expire();
                }

                AddInternal(currentLine = new OsuSpriteText
                {
                    Text = value,
                    Alpha = 0,
                    Y = -5,
                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Black),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                });
                currentLine.MoveToY(0, FadeInDuration, fadeInEasing)
                           .FadeIn(FadeInDuration, fadeInEasing);

                currentRawText = value;
                checkIfEmpty();
            }
        }

        public string TranslatedText
        {
            get => currentRawTranslateText;
            set
            {
                if (currentLineTranslated != null)
                {
                    if (value == currentLineTranslated.Text) return;

                    currentLineTranslated?.MoveToY(5, FadeOutDuration, fadeOutEasing)
                                         .FadeOut(FadeOutDuration, fadeOutEasing).Then().Expire();
                }

                AddInternal(currentLineTranslated = new OsuSpriteText
                {
                    Text = value,
                    Alpha = 0,
                    Y = -5,
                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Black),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = 40 }
                });
                currentLineTranslated.MoveToY(0, FadeInDuration, fadeInEasing)
                                     .FadeIn(FadeInDuration, fadeInEasing);

                currentRawTranslateText = value;
                checkIfEmpty();
            }
        }

        private void checkIfEmpty()
        {
            if (string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(TranslatedText))
                this.FadeOut(200, Easing.OutQuint);
            else
                this.FadeIn(200, Easing.OutQuint);
        }

        public LyricLine()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                boxContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.4f)
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 50,
                            Colour = ColourInfo.GradientVertical(
                                Color4.Black.Opacity(0.4f),
                                Color4.Black.Opacity(0)),
                            Rotation = 180,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 50,
                            Colour = ColourInfo.GradientVertical(
                                Color4.Black.Opacity(0),
                                Color4.Black.Opacity(0.4f)),
                            Rotation = 180,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre
                        }
                    }
                }
            };
        }
    }
}
