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

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, OsuColour colours, MfConfigManager config)
        {
            var originalPath = new DirectoryInfo(storage.GetFullPath(string.Empty)).Parent?.FullName;
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
                        Child = fileSelector = new FileSelector(originalPath, validFileExtensions: FileExtensions)
                        {
                            RelativeSizeAxes = Axes.Both,
                            CurrentFile = { BindTarget = currentFile },
                            CurrentPath = { BindTarget = currentDirectory }
                        }
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
                                                        new TriangleButton()
                                                        {
                                                            Text = "选中该文件",
                                                            Anchor = Anchor.BottomCentre,
                                                            Origin = Anchor.BottomCentre,
                                                            RelativeSizeAxes = Axes.X,
                                                            Height = 50,
                                                            Action = () => dialogOverlay.Push(new ImportConfirmDialog(currentFile.Value?.Name)
                                                            {
                                                                OnConfirmedAction = StartImport
                                                            }),
                                                            Margin = new MarginPadding{Top = 15},
                                                            Padding = new MarginPadding{Horizontal = 15}
                                                        }
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

            currentFile.UnbindBindings();
            currentDirectory.UnbindBindings();

            fileSelector?.Expire();
            var directory = currentDirectory.Value?.FullName ?? defaultPath;

            fileSelector = new FileSelector(directory, validFileExtensions: FileExtensions)
            {
                RelativeSizeAxes = Axes.Both,
                CurrentFile = { BindTarget = currentFile },
                CurrentPath = { BindTarget = currentDirectory }
            };

            fileSelectContainer.Add(fileSelector);
        }

        private void updateFileSelectionText(ValueChangedEvent<FileInfo> v)
        {
            currentFileText.Text = v.NewValue?.FullName ?? "请选择一个文件";
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

        private void StartImport()
        {
            var path = currentFile.Value?.FullName ?? null;
            if (path == null) return;

            string[] paths = { path };

            Task.Factory.StartNew(() => gameBase.Import(paths), TaskCreationOptions.LongRunning);
        }
    }
}
