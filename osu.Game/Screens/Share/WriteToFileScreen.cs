using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Share
{
    public class WriteToFileScreen : OsuScreen
    {
        public BindableList<BeatmapSetInfo> SelectedBeatmapSets = new BindableList<BeatmapSetInfo>();
        private OsuTextBox textBox;
        private bool writeRunning;

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
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                    Height = 0.9f,
                    Masking = true,
                    CornerRadius = 12.5f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GreySeafoamDark
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
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
            }
            catch (Exception e)
            {
                Logger.Error(e, "尝试写入时发生了错误");
            }

            writeRunning = false;
        }
    }
}
