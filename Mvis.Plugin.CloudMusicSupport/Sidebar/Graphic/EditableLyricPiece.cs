using System;
using System.Globalization;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.LLin;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class EditableLyricPiece : DrawableLyric, IHasTooltip
    {
        public LocalisableString TooltipText { get; set; }

        private Box hoverBox;

        public Action OnAdjust;
        public Action OnDeleted;
        private OsuTextBox timeTextBox;
        private OsuTextBox translationTextBox;
        private OsuTextBox contentTextBox;

        public EditableLyricPiece(Lyric lrc)
        {
            Value = lrc;
        }

        public EditableLyricPiece()
        {
            Value = new Lyric();
        }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider colourProvider, OsuColour osuColour, IImplementLLin mvisScreen)
        {
            CornerRadius = 5f;
            Masking = true;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Box bgBox;
            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both
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
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.5f,
                            PlaceholderText = CloudMusicStrings.LyricTime,
                            CommitOnFocusLost = true
                        },
                        contentTextBox = new OsuTextBox
                        {
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            PlaceholderText = CloudMusicStrings.LyricRaw,
                            CommitOnFocusLost = true
                        },
                        translationTextBox = new OsuTextBox
                        {
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            PlaceholderText = CloudMusicStrings.LyricTranslated,
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
                                    Text = CloudMusicStrings.Delete,
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
                                    Text = CloudMusicStrings.TrackTimeToLyric,
                                    Size = new Vector2(120, 40),
                                    Action = () =>
                                    {
                                        Value.Time = mvisScreen.CurrentTrack.CurrentTime;
                                        OnAdjust?.Invoke();
                                        timeTextBox.Text = Value.Time.ToString(CultureInfo.InvariantCulture);
                                    },
                                    BackgroundColour = osuColour.GreySeafoamDarker
                                },
                                new OsuButton
                                {
                                    Text = CloudMusicStrings.LyricTimeToTrack,
                                    Size = new Vector2(120, 40),
                                    Action = () => mvisScreen.SeekTo(Value.Time)
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
                if (double.TryParse(sender.Text, out double newTime))
                {
                    Value.Time = newTime;
                    OnAdjust?.Invoke();
                }
                else
                    timeTextBox.Text = Value.Time.ToString(CultureInfo.InvariantCulture);
            };

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.Dark4;
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

        //下方按钮(40) + 3 * 文本框高度(40) + 2 * 文本框Spacing(5) + 上下Margin(10) + Spacing(10)
        public override int FinalHeight() => 40 + 3 * 40 + 2 * 5 + 10 + 10;

        protected override void UpdateValue(Lyric lyric)
        {
            contentTextBox.Text = lyric.Content;
            translationTextBox.Text = lyric.TranslatedString;
            timeTextBox.Text = lyric.Time.ToString(CultureInfo.InvariantCulture);
        }
    }
}
