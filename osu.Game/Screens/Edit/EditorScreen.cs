// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// TODO: eventually make this inherit Screen and add a local screen stack inside the Editor.
    /// </summary>
    public abstract class EditorScreen : Container
    {
        [Resolved]
        protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        protected EditorScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            InternalChild = content = new Container { RelativeSizeAxes = Axes.Both };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeTo(0)
                .Then()
                .FadeTo(1f, 250, Easing.OutQuint);
        }

        public void Exit()
        {
            this.FadeOut(250).Expire();
        }
    }
}
