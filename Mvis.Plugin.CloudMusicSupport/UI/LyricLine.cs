using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.UI
{
    public class LyricLine : CompositeDrawable
    {
        private OsuSpriteText currentLine;
        private OsuSpriteText currentLineTranslated;
        private string currentRawText;
        private string currentRawTranslateText;
        private bool disableOutline;
        private readonly BufferedContainer outlineEffectContainer;

        private readonly Container lyricContainer = new Container
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
        };

        public float FadeOutDuration = 200;
        private Easing fadeOutEasing => Easing.OutQuint;
        private Easing fadeInEasing => Easing.OutQuint;

        public float FadeInDuration = 200;

        public bool DisableOutline
        {
            get => disableOutline;
            set
            {
                disableOutline = value;

                if (value)
                {
                    if (outlineEffectContainer.Contains(lyricContainer))
                        outlineEffectContainer.Remove(lyricContainer);

                    AddInternal(lyricContainer);
                    outlineEffectContainer.Hide();
                }
                else
                {
                    if (InternalChildren.Contains(lyricContainer))
                        RemoveInternal(lyricContainer);

                    outlineEffectContainer.Add(lyricContainer);
                    outlineEffectContainer.Show();
                }
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

                lyricContainer.Add(currentLine = new OsuSpriteText
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

                lyricContainer.Add(currentLineTranslated = new OsuSpriteText
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

            InternalChild = outlineEffectContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            }.WithEffect(new OutlineEffect
            {
                Colour = Color4.Black.Opacity(0.3f),
                BlurSigma = new Vector2(3f),
                Strength = 3f
            });

            outlineEffectContainer.FrameBufferScale = new Vector2(1.1f);
        }
    }
}
