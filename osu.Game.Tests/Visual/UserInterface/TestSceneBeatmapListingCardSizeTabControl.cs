// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBeatmapListingCardSizeTabControl : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly Bindable<BeatmapCardSize> cardSize = new Bindable<BeatmapCardSize>();

        private SpriteText cardSizeText;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    cardSizeText = new OsuSpriteText
                    {
                        Font = OsuFont.Default.With(size: 24)
                    },
                    new BeatmapListingCardSizeTabControl
                    {
                        Current = cardSize,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            cardSize.BindValueChanged(size => cardSizeText.Text = $"Current size: {size.NewValue}", true);
        }
    }
}
