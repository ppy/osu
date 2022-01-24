// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Edit
{
    public class ScrollingToolboxGroup : EditorToolboxGroup
    {
        protected readonly OsuScrollContainer Scroll;

        protected override Container<Drawable> Content { get; }

        public ScrollingToolboxGroup(string title, float scrollAreaHeight)
            : base(title)
        {
            base.Content.Add(Scroll = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = scrollAreaHeight,
                Child = Content = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                },
            });
        }
    }
}
