// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// TODO: eventually make this inherit Screen and add a local screen stack inside the Editor.
    /// </summary>
    public abstract class EditorScreen : VisibilityContainer
    {
        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public readonly EditorScreenMode Type;

        protected EditorScreen(EditorScreenMode type)
        {
            Type = type;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            InternalChild = content = new Container { RelativeSizeAxes = Axes.Both };
        }

        protected override void PopIn()
        {
            this.ScaleTo(1f, 200, Easing.OutQuint)
                .FadeIn(200, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.ScaleTo(0.98f, 200, Easing.OutQuint)
                .FadeOut(200, Easing.OutQuint);
        }
    }
}
