// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public abstract partial class SongSelectComponentsTestScene : OsuTestScene
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        protected override Container<Drawable> Content { get; } = new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding(10),
        };

        private Container? resizeContainer;
        private float relativeWidth;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddSliderStep("change relative width", 0, 1f, 0.5f, v =>
            {
                if (resizeContainer != null)
                    resizeContainer.Width = v;

                relativeWidth = v;
            });
        }

        [SetUpSteps]
        public virtual void SetUpSteps()
        {
            AddStep("reset dependencies", () =>
            {
                Beatmap.SetDefault();
                SelectedMods.SetDefault();
            });

            AddStep("setup content", () =>
            {
                base.Content.Add(resizeContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Width = relativeWidth,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourProvider.Background5,
                        },
                        Content
                    }
                });
            });
        }
    }
}
