// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit
{
    public class EditorRoundedScreen : EditorScreen
    {
        public const int HORIZONTAL_PADDING = 100;

        [Resolved]
        private OsuColour colours { get; set; }

        private Container roundedContent;

        protected override Container<Drawable> Content => roundedContent;

        public EditorRoundedScreen(EditorScreenMode mode)
            : base(mode)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            base.Content.Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(50),
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background3,
                            RelativeSizeAxes = Axes.Both,
                        },
                        roundedContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                }
            });
        }
    }
}
