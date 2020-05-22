// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class MigrationSelectScreen : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg2");

        private DirectorySelector directorySelector;
        private Container contentContainer;
        private DialogOverlay dialogOverlay;

        public override bool AllowExternalScreenChange => false;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool HideOverlaysOnEnter => true;

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, Storage storage, OsuColour colours, DialogOverlay dialogOverlay)
        {
            game?.Toolbar.Hide();
            this.dialogOverlay = dialogOverlay;

            // begin selection in the parent directory of the current storage location
            var initialPath = new DirectoryInfo(storage.GetFullPath(string.Empty)).Parent?.FullName;

            InternalChild = contentContainer = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.5f, 0.8f),
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.GreySeafoamDark,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.Relative, 0.8f),
                            new Dimension(),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "请选择一个新的地址",
                                    Font = OsuFont.Default.With(size: 40)
                                },
                            },
                            new Drawable[]
                            {
                                directorySelector = new DirectorySelector(initialPath)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            },
                            new Drawable[]
                            {
                                new TriangleButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 300,
                                    Text = "开始数据迁移",
                                    Action = start
                                },
                            }
                        }
                    }
                }
            };
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            contentContainer.ScaleTo(0.8f).Then().FadeOut().Then()
                            .ScaleTo(1f, 1000, Easing.OutElastic).FadeIn(1000, Easing.OutExpo);
        }

        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(1000, Easing.OutExpo);

            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            this.FadeOut(250);
        }

        private void start()
        {
            var target = directorySelector.CurrentDirectory.Value;

            dialogOverlay?.Push(new MigrateConfirmDialog(() =>
            {
                try
                {
                    if (target.GetDirectories().Length > 0 || target.GetFiles().Length > 0)
                        target = target.CreateSubdirectory("osu-lazer");
                }
                catch (Exception e)
                {
                    Logger.Log($"Error during migration: {e.Message}", level: LogLevel.Error);
                    return;
                }

                ValidForResume = false;
                BeginMigration(target);
            }, $"{target}"));

        }

        protected virtual void BeginMigration(DirectoryInfo target) => this.Push(new MigrationRunScreen(target));
    }
}
