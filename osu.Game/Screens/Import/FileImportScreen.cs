using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osu.Game.Overlays.Settings;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Screens.Import
{
    public class FileImportScreen : OsuScreen
    {
        private Container contentContainer;
        private FileSelector fileSelector;
        private Container fileSelectContainer;

        public override bool HideOverlaysOnEnter => true;

        private string[] FileExtensions = { ".foo" };
        private string defaultPath;

        private readonly Bindable<FileInfo> currentFile = new Bindable<FileInfo>();
        private readonly IBindable<DirectoryInfo> currentDirectory = new Bindable<DirectoryInfo>();
        private Bindable<FileFilterType> FilterType = new Bindable<FileFilterType>();
        private TextFlowContainer currentFileText;
        private OsuScrollContainer fileNameScroll;
        private readonly OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);

        [Resolved]
        private OsuGameBase gameBase { get; set; }

        [Resolved]
        private DialogOverlay dialogOverlay { get; set; }

        private Storage storage { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, MfConfigManager config)
        {
            this.storage = storage;

            storage.GetStorageForDirectory("imports");
            var originalPath = storage.GetFullPath("imports", true);

            defaultPath = originalPath;

            InternalChild = contentContainer = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.9f, 0.8f),
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = overlayColourProvider.Background5,
                        RelativeSizeAxes = Axes.Both,
                    },
                    fileSelectContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.65f,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.35f,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                RowDimensions = new Dimension[]
                                {
                                    new Dimension(GridSizeMode.Distributed),
                                    new Dimension(GridSizeMode.AutoSize),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    Colour = overlayColourProvider.Background3,
                                                    RelativeSizeAxes = Axes.Both
                                                },
                                                fileNameScroll = new OsuScrollContainer
                                                {
                                                    Masking = false,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Child = currentFileText = new TextFlowContainer(t => t.Font = OsuFont.Default.With(size: 30))
                                                    {
                                                        AutoSizeAxes = Axes.Y,
                                                        RelativeSizeAxes = Axes.X,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        TextAnchor = Anchor.Centre
                                                    },
                                                },
                                            }
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    Colour = overlayColourProvider.Background4,
                                                    RelativeSizeAxes = Axes.Both
                                                },
                                                new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Spacing = new Vector2(10),
                                                    Children = new Drawable[]
                                                    {
                                                        new SettingsEnumDropdown<FileFilterType>
                                                        {
                                                            Anchor = Anchor.BottomCentre,
                                                            Origin = Anchor.BottomCentre,
                                                            LabelText = "文件类型",
                                                            Current = config.GetBindable<FileFilterType>(MfSetting.FileFilter),
                                                            Margin = new MarginPadding{ Bottom = 15 }
                                                        },
                                                        new GridContainer
                                                        {
                                                            Anchor = Anchor.BottomCentre,
                                                            Origin = Anchor.BottomCentre,
                                                            AutoSizeAxes = Axes.Y,
                                                            RelativeSizeAxes = Axes.X,
                                                            Margin = new MarginPadding{Top = 15},
                                                            RowDimensions = new Dimension[]
                                                            {
                                                                new Dimension(GridSizeMode.AutoSize)
                                                            },
                                                            Content = new []
                                                            {
                                                                new Drawable[]
                                                                {
                                                                    new TriangleButton
                                                                    {
                                                                        Anchor = Anchor.BottomCentre,
                                                                        Origin = Anchor.BottomCentre,
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Height = 50,
                                                                        Width = 0.9f,
                                                                        Text = "刷新文件列表",
                                                                        Action = Refresh
                                                                    },
                                                                    new TriangleButton()
                                                                    {
                                                                        Text = "导入该文件",
                                                                        Anchor = Anchor.BottomCentre,
                                                                        Origin = Anchor.BottomCentre,
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Height = 50,
                                                                        Width = 0.9f,
                                                                        Action = () =>
                                                                        {
                                                                            var d = currentFile.Value?.FullName;
                                                                            var n = currentFile.Value?.Name;
                                                                            if ( d != null )
                                                                                StartImport(d);
                                                                            else
                                                                                currentFileText.FlashColour(Color4.Red, 500);
                                                                        },
                                                                    }
                                                                },
                                                            }
                                                        },
                                                    }
                                                },
                                            }
                                        }
                                    }
                                }
                            },
                        }
                    }
                }
            };

            fileNameScroll.ScrollContent.Anchor = Anchor.Centre;
            fileNameScroll.ScrollContent.Origin = Anchor.Centre;

            config.BindWith(MfSetting.FileFilter, FilterType);

            currentFile.BindValueChanged(updateFileSelectionText, true);
            currentDirectory.BindValueChanged(_ =>
            {
                currentFile.Value = null;
            });

            FilterType.BindValueChanged(OnFilterTypeChanged, true);
        }

        private void OnFilterTypeChanged(ValueChangedEvent<FileFilterType> v)
        {
            switch (v.NewValue)
            {
                case FileFilterType.Beatmap:
                    FileExtensions = new string[] { ".osz" };
                    break;

                case FileFilterType.Skin:
                    FileExtensions = new string[] { ".osk" };
                    break;

                case FileFilterType.Replay:
                    FileExtensions = new string[] { ".osr" };
                    break;

                default:
                case FileFilterType.All:
                    FileExtensions = new string[] { ".osk", ".osr", ".osz" };
                    break;
            }

            Refresh();
        }

        private void Refresh()
        {
            //解绑
            currentFile.UnbindBindings();
            currentDirectory.UnbindBindings();

            //清理文件选择器fileSelector
            fileSelector?.Expire();

            //创建字符串directory，赋值为当前目录或默认目录
            var directory = currentDirectory.Value?.FullName ?? defaultPath;

            //设置文件选择器
            fileSelector = new FileSelector(initialPath: directory, validFileExtensions: FileExtensions)
            {
                RelativeSizeAxes = Axes.Both
            };

            //绑定
            currentDirectory.BindTo(fileSelector.CurrentPath);
            currentFile.BindTo(fileSelector.CurrentFile);

            //添加
            fileSelectContainer.Add(fileSelector);
        }

        private void updateFileSelectionText(ValueChangedEvent<FileInfo> v)
        {
            currentFileText.Text = v.NewValue?.Name ?? "请选择一个文件";
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            contentContainer.FadeOut().Then().ScaleTo(0.8f).RotateTo(-15).MoveToX(300)
                            .Then()
                            .ScaleTo(1, 1500, Easing.OutElastic)
                            .FadeIn(500)
                            .MoveToX(0, 500, Easing.OutQuint)
                            .RotateTo(0, 500, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            contentContainer.ScaleTo(0.8f, 500, Easing.OutExpo).RotateTo(-15, 500, Easing.OutExpo).MoveToX(300, 500, Easing.OutQuint).FadeOut(500);
            this.FadeOut(500, Easing.OutExpo);

            return base.OnExiting(next);
        }

        private void StartImport(string path)
        {
            //在某些特殊情况下会这样...
            if (string.IsNullOrEmpty(path))
                return;

            //如果文件被移动或删除
            if (!storage.Exists(path))
            {
                Refresh();
                currentFileText.Text = "文件不存在";
                currentFileText.FlashColour(Color4.Red, 500);
                return;
            }

            string[] paths = { path };

            Task.Factory.StartNew(() => gameBase.Import(paths), TaskCreationOptions.LongRunning);
        }
    }
}
