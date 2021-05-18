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
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class EditableLyricPiece : DrawableLyric, IHasTooltip
    {
        public string TooltipText { get; set; }

        private Box hoverBox;

        public Action OnSeekTriggered;
        public Action OnAdjustTriggered;
        public Action OnDeleted;
        public Action OnSave;

        public EditableLyricPiece(Lyric lrc)
        {
            CornerRadius = 5f;
            Masking = true;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Value = lrc;
        }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider colourProvider, OsuColour osuColour)
        {
            Box bgBox;
            OsuTextBox timeTextBox;
            OsuTextBox contentTextBox;
            OsuTextBox translationTextBox;
            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 5 },
                    Margin = new MarginPadding { Vertical = 5 },
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        timeTextBox = new OsuTextBox
                        {
                            Text = Value.Time.ToString(),
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.5f,
                            PlaceholderText = "歌词时间(毫秒)",
                            CommitOnFocusLost = true
                        },
                        contentTextBox = new OsuTextBox
                        {
                            Text = Value.Content,
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            PlaceholderText = "歌词原文",
                            CommitOnFocusLost = true
                        },
                        translationTextBox = new OsuTextBox
                        {
                            Text = Value.TranslatedString,
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            PlaceholderText = "歌词翻译",
                            CommitOnFocusLost = true,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new OsuButton
                                {
                                    Text = "删除这条歌词",
                                    Size = new Vector2(90, 40),
                                    Action = () =>
                                    {
                                        OnDeleted?.Invoke();
                                        Expire();
                                    },
                                    BackgroundColour = osuColour.PinkDark
                                },
                                new OsuButton
                                {
                                    Text = "调整歌词到歌曲时间",
                                    Size = new Vector2(120, 40),
                                    Action = () =>
                                    {
                                        OnAdjustTriggered?.Invoke();
                                        timeTextBox.Text = Value.Time.ToString();
                                    },
                                    BackgroundColour = osuColour.GreySeafoamDarker
                                },
                                new OsuButton
                                {
                                    Text = "调整歌曲到歌词时间",
                                    Size = new Vector2(120, 40),
                                    Action = () => OnSeekTriggered?.Invoke()
                                }
                            }
                        }
                    }
                },
                hoverBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White.Opacity(0.1f),
                    Alpha = 0
                },
                new HoverClickSounds()
            };

            translationTextBox.OnCommit += (sender, isNewText) =>
            {
                Value.TranslatedString = sender.Text;
            };

            contentTextBox.OnCommit += (sender, isNewText) =>
            {
                Value.Content = sender.Text;
            };

            timeTextBox.OnCommit += (sender, isNewText) =>
            {
                if (int.TryParse(sender.Text, out int newTime))
                {
                    Value.Time = newTime;
                }
                else
                    timeTextBox.Text = Value.Time.ToString();
            };

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.Background4;
            }, true);
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
    }
}
