using System;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.LLin;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class LyricPiece : DrawableLyric, IHasTooltip, IHasContextMenu
    {
        public LocalisableString TooltipText { get; private set; }

        [Resolved]
        private LyricConfigManager config { get; set; }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(
                CloudMusicStrings.AdjustOffsetToLyric.ToString(),
                MenuItemType.Standard,
                () => config.SetValue(LyricSettings.LyricOffset, Value.Time - mvisScreen.CurrentTrack.CurrentTime))
        };

        private Box hoverBox;
        private OsuSpriteText contentText;
        private OsuSpriteText translateText;
        private OsuSpriteText timeText;

        public LyricPiece(Lyric lrc)
        {
            Value = lrc;
        }

        public LyricPiece()
        {
            Value = new Lyric
            {
                Content = "missingno"
            };
        }

        [Resolved]
        private IImplementLLin mvisScreen { get; set; }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider colourProvider)
        {
            CornerRadius = 5f;
            Masking = true;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

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
                                timeText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold),
                                    Colour = Color4.Black,
                                    Margin = new MarginPadding { Horizontal = 5, Vertical = 3 }
                                },
                            }
                        },
                        contentText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Truncate = true,
                            Margin = new MarginPadding { Left = 5, Bottom = 5 }
                        },
                        translateText = new OsuSpriteText
                        {
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

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                fgBox.Colour = colourProvider.Highlight1;
            }, true);
        }

        protected override void UpdateValue(Lyric lyric)
        {
            contentText.Text = lyric.Content;
            translateText.Text = lyric.TranslatedString;

            var timeSpan = TimeSpan.FromMilliseconds(lyric.Time);
            timeText.Text = TooltipText = $"{timeSpan:mm\\:ss\\.fff}";

            Colour = string.IsNullOrEmpty(lyric.Content)
                ? Color4Extensions.FromHex(@"555")
                : Color4.White;
        }

        protected override bool OnClick(ClickEvent e)
        {
            mvisScreen.SeekTo(Value.Time + 1);
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
