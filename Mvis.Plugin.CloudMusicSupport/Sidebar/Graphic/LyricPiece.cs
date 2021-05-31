using System;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class LyricPiece : DrawableLyric, IHasTooltip
    {
        public Action<Lyric> Action;
        public string TooltipText { get; private set; }

        private Box hoverBox;
        private OsuSpriteText contentText;
        private OsuSpriteText translateText;

        public LyricPiece(Lyric lrc)
        {
            Value = lrc;
        }

        public LyricPiece()
        {
            Value = new Lyric();
        }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider colourProvider)
        {
            CornerRadius = 5f;
            Masking = true;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Colour = string.IsNullOrEmpty(Value.Content)
                ? Color4Extensions.FromHex(@"555")
                : Color4.White;

            var timeSpan = TimeSpan.FromMilliseconds(Value.Time);

            Box fgBox;
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Masking = true,
                            CornerRadius = 5,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                fgBox = new Box
                                {
                                    Colour = colourProvider.Highlight1,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold),
                                    Text = $"{timeSpan:mm\\:ss\\.fff}",
                                    Colour = Color4.Black,
                                    Margin = new MarginPadding { Horizontal = 5, Vertical = 3 }
                                },
                            }
                        },
                        contentText = new OsuSpriteText
                        {
                            Text = Value.Content,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Truncate = true,
                            Margin = new MarginPadding { Left = 5, Bottom = 5 }
                        },
                        translateText = new OsuSpriteText
                        {
                            Text = Value.TranslatedString,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Truncate = true,
                            Margin = new MarginPadding { Left = 5, Bottom = 5 }
                        },
                    }
                },
                hoverBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White.Opacity(0.2f),
                    Alpha = 0
                },
                new HoverClickSounds()
            };

            TooltipText = $"调整到 {timeSpan:mm\\:ss\\.fff}";

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                fgBox.Colour = colourProvider.Highlight1;
            }, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke(Value);
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverBox.FadeIn(300);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBox.FadeOut(300);
            base.OnHoverLost(e);
        }

        //时间显示的高度(23) + 时间显示的Margin(10) + 2 * (文本高度 + 文本Margin(5))
        public override int FinalHeight() => 23 + 10
                                                + (int)(string.IsNullOrEmpty(Value.TranslatedString)
                                                    ? 0
                                                    : (contentText?.Height ?? 18 + 5))
                                                + (int)(string.IsNullOrEmpty(Value.Content)
                                                    ? 0
                                                    : (translateText?.Height ?? 18 + 5))
                                                + 10; //向下Margin
    }
}
