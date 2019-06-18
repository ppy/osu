// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public abstract class TournamentEditorScreen<T> : TournamentScreen, IProvideVideo
        where T : Drawable
    {
        protected readonly FillFlowContainer<T> Flow;

        protected TournamentEditorScreen()
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.9f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = Flow = new FillFlowContainer<T>
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        LayoutDuration = 200,
                        LayoutEasing = Easing.OutQuint,
                        Spacing = new Vector2(20)
                    },
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Add new",
                            Action = AddNew
                        },
                    }
                }
            });
        }

        protected abstract void AddNew();
    }
}
