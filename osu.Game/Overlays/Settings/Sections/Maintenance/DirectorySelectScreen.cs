// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using System.IO;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens;
using osuTK;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Screens;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public abstract class DirectorySelectScreen : OsuScreen
    {
        private TriangleButton selectionButton;

        private DirectorySelector directorySelector;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        protected abstract OsuSpriteText CreateHeader();

        /// <summary>
        /// Called upon selection of a directory by the user.
        /// </summary>
        /// <param name="directory">The selected directory</param>
        protected abstract void OnSelection(DirectoryInfo directory);

        protected virtual bool IsValidDirectory(DirectoryInfo info) => info != null;

        protected virtual DirectoryInfo InitialPath => null;

        public override bool AllowExternalScreenChange => false;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new Container
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
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreySeafoamDark
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
                                CreateHeader().With(header =>
                                {
                                    header.Origin = Anchor.Centre;
                                    header.Anchor = Anchor.Centre;
                                })
                            },
                            new Drawable[]
                            {
                                directorySelector = new DirectorySelector
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            },
                            new Drawable[]
                            {
                                selectionButton = new TriangleButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 300,
                                    Text = "Select directory",
                                    Action = () => OnSelection(directorySelector.CurrentPath.Value)
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            if (InitialPath != null)
                directorySelector.CurrentPath.Value = InitialPath;

            directorySelector.CurrentPath.BindValueChanged(e => selectionButton.Enabled.Value = IsValidDirectory(e.NewValue), true);
            base.LoadComplete();
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            this.FadeOut(250);
        }
    }
}
