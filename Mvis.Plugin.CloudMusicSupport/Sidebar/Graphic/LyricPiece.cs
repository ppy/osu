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
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class LyricPiece : DrawableLyric, IHasTooltip, IHasContextMenu
    {
        public LocalisableString TooltipText { get; private set; }

        [Resolved]
        private LyricConfigManager config { get; set; }

        [Resolved]
        private LyricPlugin plugin { get; set; }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(
                CloudMusicStrings.AdjustOffsetToLyric.ToString(),
                MenuItemType.Standard,
                () => plugin.Offset.Value = Value.Time - mvisScreen.CurrentTrack.CurrentTime)
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

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private Box bgBox;

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerRadius = 5f;
            Masking = true;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    Colour = colourProvider.Highlight1,
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(5),
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "时间显示",
                            Masking = true,
                            CornerRadius = 5,
                            AutoSizeAxes = Axes.Y,
                            Width = 80,
                            Margin = new MarginPadding { Vertical = 5, Left = 5, Right = -1 },
                            Children = new Drawable[]
                            {
                                timeText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold),
                                    Margin = new MarginPadding { Horizontal = 5, Vertical = 3 },
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                }
                            }
                        },
                        new Circle
                        {
                            Name = "分隔",
                            Height = 3,
                            Colour = Color4.Gray.Opacity(0.6f),
                            Width = 20,
                            Margin = new MarginPadding { Top = 16 }
                        },
                        new Container
                        {
                            Name = "歌词内容",
                            Width = 380,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Top = 6 },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Height = 18,
                                    Colour = Color4.White.Opacity(0)
                                },
                                textFillFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        contentText = new OsuSpriteText
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Truncate = true,
                                            Margin = new MarginPadding { Left = 5, Bottom = 5 }
                                        },
                                        translateText = new OsuSpriteText
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Truncate = true,
                                            Margin = new MarginPadding { Left = 5, Bottom = 5 }
                                        },
                                    }
                                },
                            }
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
                bgBox.Colour = colourProvider.Highlight1.Opacity(isCurrent ? 1 : 0);
            }, true);
        }

        private bool isCurrent_real;

        private bool isCurrent
        {
            get => isCurrent_real;
            set
            {
                bgBox.FadeColour(colourProvider.Highlight1.Opacity(value ? 1 : 0), 300, Easing.OutQuint);
                textFillFlow.FadeColour(value ? Color4.Black : Color4.White, 300, Easing.OutQuint);
                timeText.FadeColour(value ? Color4.Black : Color4.White, 300, Easing.OutQuint);

                isCurrent_real = value;
            }
        }

        protected override void Update()
        {
            isCurrent = plugin.CurrentLine.Equals(Value);

            base.Update();
        }

        private bool haveLyric;
        private FillFlowContainer textFillFlow;

        protected override void UpdateValue(Lyric lyric)
        {
            contentText.Text = lyric.Content;
            translateText.Text = lyric.TranslatedString;

            var timeSpan = TimeSpan.FromMilliseconds(lyric.Time);
            timeText.Text = $"{timeSpan:mm\\:ss\\.fff}";
            TooltipText = $"{timeText.Text}"
                          + (string.IsNullOrEmpty(lyric.Content)
                              ? ""
                              : $"－ {lyric.Content}")
                          + (string.IsNullOrEmpty(lyric.TranslatedString)
                              ? ""
                              : $"－ {lyric.TranslatedString}");

            haveLyric = string.IsNullOrEmpty(lyric.Content);

            Colour = haveLyric
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

        //时间显示的高度(23) + 2 * (文本高度 + 文本Margin(5))
        public override int FinalHeight()
        {
            int val = 23; //时间显示大小

            val += 10; //时间显示Margin

            if (!string.IsNullOrEmpty(Value.Content) && !string.IsNullOrEmpty(Value.TranslatedString))
            {
                val += (int)(string.IsNullOrEmpty(Value.TranslatedString)
                    ? (string.IsNullOrEmpty(Value.Content)
                        ? 0
                        : (translateText?.Height ?? 18 + 5))
                    : (contentText?.Height ?? 18 + 5));
            }

            val += 5; //向下Margin

            return val;
        }
    }
}
