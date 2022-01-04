// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Setup
{
    public class SetupScreenHeaderBackground : CompositeDrawable
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; }

        private readonly Container content;

        public SetupScreenHeaderBackground()
        {
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            UpdateBackground();
        }

        public void UpdateBackground()
        {
            LoadComponentAsync(new BeatmapBackgroundSprite(working.Value)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            }, background =>
            {
                if (background.Texture != null)
                    content.Child = background;
                else
                {
                    content.Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeaFoamDarker,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 24))
                        {
                            Text = "Drag image here to set beatmap background!",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both
                        }
                    };
                }

                background.FadeInFromZero(500);
            });
        }
    }
}
