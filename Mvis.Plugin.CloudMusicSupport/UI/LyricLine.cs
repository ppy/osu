using System.Linq;
using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        private readonly BufferedContainer outlineEffectContainer;

        private readonly Container lyricContainer = new Container
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
        };

        private Easing fadeOutEasing => Easing.OutQuint;
        private Easing fadeInEasing => Easing.OutQuint;

        public string Text
        {
            get => currentRawText;
            set
            {
                currentLine?.MoveToY(5, fadeOutDuration.Value, fadeOutEasing)
                           .FadeOut(fadeOutDuration.Value, fadeOutEasing).Then().Expire();

                lyricContainer.Add(currentLine = new OsuSpriteText
                {
                    Text = value,
                    Alpha = 0,
                    Y = -5,
                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Black),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                });
                currentLine.MoveToY(0, fadeInDuration.Value, fadeInEasing)
                           .FadeIn(fadeInDuration.Value, fadeInEasing);

                currentRawText = value;
                checkIfEmpty();
            }
        }

        public string TranslatedText
        {
            get => currentRawTranslateText;
            set
            {
                currentLineTranslated?.MoveToY(5, fadeOutDuration.Value, fadeOutEasing)
                                     .FadeOut(fadeOutDuration.Value, fadeOutEasing).Then().Expire();

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
                currentLineTranslated.MoveToY(0, fadeInDuration.Value, fadeInEasing)
                                     .FadeIn(fadeInDuration.Value, fadeInEasing);

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

        private readonly Bindable<float> fadeInDuration = new Bindable<float>();
        private readonly Bindable<float> fadeOutDuration = new Bindable<float>();
        private readonly Bindable<bool> disableOutline = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load(LyricConfigManager config)
        {
            //var config = Dependencies.Get<LyricConfigManager>();

            config.BindWith(LyricSettings.LyricFadeInDuration, fadeInDuration);
            config.BindWith(LyricSettings.LyricFadeOutDuration, fadeOutDuration);
            config.BindWith(LyricSettings.NoExtraShadow, disableOutline);
        }

        protected override void LoadComplete()
        {
            disableOutline.BindValueChanged(v =>
            {
                if (v.NewValue)
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
            }, true);

            base.LoadComplete();
        }
    }
}
