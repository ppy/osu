using System;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Helper;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class Toolbox : CompositeDrawable
    {
        private readonly FillFlowContainer buttonFillFlow;
        private readonly OsuSpriteText idText;

        private readonly IconButton backButton = new IconButton
        {
            Icon = FontAwesome.Solid.ArrowLeft,
            Size = new Vector2(45),
            IconColour = Color4.Black,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };

        private readonly FillFlowContainer contentFillFlow;
        private OsuTextBox textBox;

        [Resolved]
        private LyricPlugin plugin { get; set; }

        private UserDefinitionHelper udh;

        public Toolbox()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            Margin = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                contentFillFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        idText = new OsuSpriteText
                        {
                            Margin = new MarginPadding { Horizontal = 15, Top = 15 },
                            Font = OsuFont.GetFont(size: 20)
                        },
                        new TrackTimeIndicator(),
                        buttonFillFlow = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Spacing = new Vector2(5),
                            AutoSizeDuration = 200,
                            AutoSizeEasing = Easing.OutQuint,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        }
                    }
                }
            };
        }

        public Action OnBackAction { get; set; }

        public string IdText
        {
            set => idText.Text = value;
        }

        public void AddButtonRange(IconButton[] range, bool isRootScreen)
        {
            buttonFillFlow.Clear(false);

            foreach (var btn in range)
            {
                btn.Anchor = btn.Origin = Anchor.Centre;

                buttonFillFlow.Add(btn);
            }

            if (!isRootScreen)
            {
                buttonFillFlow.Add(backButton);
                backButton.Action = OnBackAction;
            }
        }

        [BackgroundDependencyLoader]
        private void load(LyricConfigManager config, LyricConfigManager lcm)
        {
            udh ??= plugin.UserDefinitionHelper;

            contentFillFlow.AddRange(new Drawable[]
            {
                new SettingsSlider<double>
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Current = plugin.Offset,
                    LabelText = CloudMusicStrings.LocalOffset,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Right = 10 }
                },
                textBox = new OsuTextBox
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "按网易云ID搜索歌词"
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new IconButton
                        {
                            Size = new Vector2(30),
                            TooltipText = "更新定义",
                            Action = () =>
                            {
                                udh.UpdateDefinition();

                                if (lcm.Get<bool>(LyricSettings.OutputDefinitionInLogs))
                                    udh.Debug();
                            },
                            Icon = FontAwesome.Solid.Cloud
                        },
                        new IconButton
                        {
                            Size = new Vector2(30),
                            TooltipText = "复制单条信息",
                            Action = () =>
                            {
                                SDL2.SDL.SDL_SetClipboardText(resolveBeatmapVerboseString(plugin.CurrentWorkingBeatmap));
                            },
                            Icon = FontAwesome.Solid.Clipboard
                        },
                        new IconButton
                        {
                            Size = new Vector2(30),
                            TooltipText = "复制模板",
                            Action = () =>
                            {
                                string targetString = "\n{\n"
                                                      + "  \"Target\": 把这条中文替换成你得到的网易云ID,\n"
                                                      + "  \"Beatmaps\":\n"
                                                      + "  [\n"
                                                      + $"    {resolveBeatmapVerboseString(plugin.CurrentWorkingBeatmap)}\n"
                                                      + "  ]\n"
                                                      + "},";
                                SDL2.SDL.SDL_SetClipboardText(targetString);
                            },
                            Icon = FontAwesome.Solid.Pen
                        }
                    }
                }
            });

            textBox.OnCommit += (sender, isNewText) =>
            {
                if (int.TryParse(sender.Text, out var id))
                    plugin.GetLyricFor(id);
                else
                {
                    textBox.Text = "";
                }
            };
        }

        private string resolveBeatmapVerboseString(WorkingBeatmap working)
        {
            return $"{working.BeatmapSetInfo.OnlineID},"
                   + $" // Title: {working.Metadata.TitleUnicode}"
                   + $"({working.Metadata.Title})"
                   + $" Artist: {working.Metadata.ArtistUnicode}"
                   + $"({working.Metadata.Artist})"
                   + $" Source: {working.Metadata.Source}";
        }
    }
}
