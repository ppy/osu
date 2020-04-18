// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Direct;
using osuTK;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingResultsDisplay : Container
    {
        private NotFoundDrawable noResultsDrawable;
        private FillFlowContainer<DirectPanel> resultPanels;

        public BeatmapListingResultsDisplay()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            noResultsDrawable = new NotFoundDrawable();
            resultPanels = new FillFlowContainer<DirectPanel>()
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Alpha = 0,
                Margin = new MarginPadding { Vertical = 15 }
            };
        }

        public void ReplaceBeatmaps(List<BeatmapSetInfo> beatmaps)
        {
            clearBeatmapPanels();

            if (beatmaps.Count > 0)
            {
                if (noResultsDrawable.IsAlive)
                    noResultsDrawable.FadeOut(100, Easing.OutQuint).Then().Schedule(() => RemoveInternal(noResultsDrawable));

                if (!resultPanels.IsAlive)
                    AddInternal(resultPanels);

                resultPanels.Show();

                AddBeatmaps(beatmaps);
            }
            else
            {
                if (resultPanels.IsAlive)
                    resultPanels.FadeOut(100, Easing.OutQuint).Then().Schedule(() => RemoveInternal(resultPanels));

                if (!noResultsDrawable.IsAlive)
                    AddInternal(noResultsDrawable);

                noResultsDrawable.FadeIn(200, Easing.OutQuint);
            }
        }

        public void AddBeatmaps(List<BeatmapSetInfo> beatmaps)
        {
            beatmaps.ForEach(addBeatmapPanel);
        }

        private void clearBeatmapPanels()
        {
            resultPanels.Children.ForEach(removeBeatmapPanel);
        }

        private void addBeatmapPanel(BeatmapSetInfo beatmap)
        {
            LoadComponentAsync(new DirectGridPanel(beatmap)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            }, loaded =>
            {
                resultPanels.Add(loaded);
                loaded.FadeIn(200, Easing.OutQuint);
            });
        }

        private void removeBeatmapPanel(DirectPanel panel)
        {
            panel.FadeOut(100, Easing.OutQuint).Expire();
        }

        private class NotFoundDrawable : CompositeDrawable
        {
            public NotFoundDrawable()
            {
                RelativeSizeAxes = Axes.X;
                Height = 250;
                Alpha = 0;
                Margin = new MarginPadding { Top = 15 };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get(@"Online/not-found")
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = @"... nope, nothing found.",
                        }
                    }
                });
            }
        }
    }
}
