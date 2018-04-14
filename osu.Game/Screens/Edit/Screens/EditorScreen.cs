// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit.Screens
{
    /// <summary>
    /// TODO: eventually make this inherit Screen and add a local scren stack inside the Editor.
    /// </summary>
    public class EditorScreen : Container
    {
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public EditorScreen()
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
