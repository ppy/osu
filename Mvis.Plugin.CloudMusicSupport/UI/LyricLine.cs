using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace Mvis.Plugin.CloudMusicSupport.UI
{
    public class LyricLine : CompositeDrawable
    {
        private OsuSpriteText currentLine;
        private OsuSpriteText currentLineTranslated;
        private string currentRawText;

        public float FadeOutDuration = 200;
        private Easing fadeOutEasing => Easing.OutQuint;

        public float FadeInDuration = 200;
        private Easing fadeInEasing => Easing.OutQuint;

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
            }
        }

        public string TranslatedText
        {
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
            }
        }

        public LyricLine()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.Both;
        }
    }
}
