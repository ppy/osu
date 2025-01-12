// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    public partial class BottomBarContainer : Container
    {
        private const float contents_padding = 15;

        protected readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public readonly Drawable Background;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public BottomBarContainer()
        {
            Masking = true;

            InternalChildren = new[]
            {
                Background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Transparent,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = contents_padding },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            Beatmap.BindTo(beatmap);
        }
    }
}
