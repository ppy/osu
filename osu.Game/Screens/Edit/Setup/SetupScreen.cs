// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    [Cached]
    public partial class SetupScreen : EditorScreen
    {
        public const float COLUMN_WIDTH = 450;
        public const float SPACING = 28;
        public const float MAX_WIDTH = 2 * COLUMN_WIDTH + SPACING;

        public Action? MetadataChanged { get; set; }

        public SetupScreen()
            : base(EditorScreenMode.SongSetup)
        {
        }

        private OsuScrollContainer scroll = null!;
        private FillFlowContainer flow = null!;

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
                scroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(15),
                    Child = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = new Vector2(25),
                        ChildrenEnumerable = ruleset.CreateEditorSetupSections().Select(section => section.With(s =>
                        {
                            s.Width = 450;
                            s.Anchor = Anchor.TopCentre;
                            s.Origin = Anchor.TopCentre;
                        })),
                    }
                }
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (scroll.DrawWidth > MAX_WIDTH)
            {
                flow.RelativeSizeAxes = Axes.None;
                flow.Width = MAX_WIDTH;
            }
            else
            {
                flow.RelativeSizeAxes = Axes.X;
                flow.Width = 1;
            }
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
