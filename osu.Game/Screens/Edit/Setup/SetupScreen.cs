// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class SetupScreen : EditorScreen
    {
        public SetupScreen()
            : base(EditorScreenMode.SongSetup)
        {
        }

        [Cached]
        private SetupScreenHeaderBackground background = new SetupScreenHeaderBackground { RelativeSizeAxes = Axes.Both, };

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap beatmap, OverlayColourProvider colourProvider)
        {
            var ruleset = beatmap.BeatmapInfo.Ruleset.CreateInstance();

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions =
                    [
                        new Dimension(GridSizeMode.Absolute, 110),
                        new Dimension()
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            background,
                        },
                        new Drawable[]
                        {
                            new OsuScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(15),
                                Child = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Full,
                                    Spacing = new Vector2(28),
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            Width = 925,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Spacing = new Vector2(25),
                                            ChildrenEnumerable = ruleset.CreateEditorSetupSections().Select(section => section.With(s =>
                                            {
                                                s.Width = 450;
                                            })),
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public override void OnExiting(ScreenExitEvent e)
        {
            base.OnExiting(e);

            // Before exiting, trigger a focus loss.
            //
            // This is important to ensure that if the user is still editing a textbox, it will commit
            // (and potentially block the exit procedure for save).
            GetContainingFocusManager()?.TriggerFocusContention(this);
        }
    }
}
