using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Share
{
    public class WriteToFileScreen : OsuScreen
    {
        public BindableList<BeatmapSetInfo> SelectedBeatmapSets = new BindableList<BeatmapSetInfo>();
        private OsuTextBox textBox;
        private bool writeRunning;
        private GridContainer grid;
        private FillFlowContainer writeSuccessContainer;
        private OsuSpriteText successText;
        private Container baseContainer;

        [Resolved]
        private Storage storage { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            ShareBeatmapDetailArea.DrawableBeatmapList list;
            InternalChildren = new Drawable[]
            {
                baseContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                    Height = 0.9f,
                    Masking = true,
                    CornerRadius = 12.5f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GreySeafoamDark
                        },
                        grid = new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Padding = new MarginPadding { Vertical = 22 },
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = "确认一下...",
                                                Font = OsuFont.GetFont(size: 40),
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre
                                            },
                                            new OsuSpriteText
                                            {
                                                Text = "即将根据下面的这些谱面创建列表",
                                                Font = OsuFont.GetFont(size: 20),
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre
                                            }
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    list = new ShareBeatmapDetailArea.DrawableBeatmapList
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                },
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Horizontal = 22 },
                                        Margin = new MarginPadding { Vertical = 22 },
                                        Child = textBox = new OsuTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            PlaceholderText = "在这里输入你想导出的文件名(不带后缀)",
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre
                                        }
                                    }
                                }
                            }
                        },
                        writeSuccessContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(10),
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.CheckCircle,
                                    Size = new Vector2(50),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                },
                                successText = new OsuSpriteText
                                {
                                    Text = "写入成功",
                                    Font = OsuFont.GetFont(size: 25),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                }
                            }
                        }
                    }
                }
            };

            textBox.OnCommit += startWrite;
            list.Items.BindTo(SelectedBeatmapSets);
        }

        private void startWrite(TextBox sender, bool newText)
        {
            if (writeRunning) return;

            writeRunning = true;

            string fileName = sender.Text + ".bl";

            try
            {
                //打开文件流，如果没有文件则创建一个
                var stream = storage.GetStream(fileName, FileAccess.Write, FileMode.Create);

                //创建StreamWriter
                using (var writer = new StreamWriter(stream))
                {
                    //递归信息
                    foreach (var info in SelectedBeatmapSets)
                    {
                        int onlineID = info.OnlineBeatmapSetID ?? -1;
                        if (onlineID < 0) continue;

                        //写入信息
                        writer.WriteLine($"{onlineID}");
                    }
                }

                //处理文件流
                stream.Dispose();

                grid.FadeOut(200);
                writeSuccessContainer.Delay(200).FadeIn(300);
                this.Delay(1500).Schedule(this.Exit);

                successText.Text = $"已写入到文件{fileName}中";
            }
            catch (Exception e)
            {
                Logger.Error(e, "尝试写入时发生了错误");
            }

            writeRunning = false;
        }

        public override void OnEntering(IScreen last)
        {
            baseContainer.FadeIn(300);
            base.OnEntering(last);
        }

        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(300).ScaleTo(0.9f, 300, Easing.OutQuint);
            return base.OnExiting(next);
        }
    }
}
