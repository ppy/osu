// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.SelectV2.Wedge
{
    public partial class DifficultyNameContent : CompositeDrawable
    {
        private OsuSpriteText difficultyName = null!;
        private OsuSpriteText mappedByLabel = null!;
        private LinkFlowContainer mapperName = null!;

        [Resolved]
        private IBindable<IBeatmapInfo?> beatmapInfo { get; set; } = null!;

        public DifficultyNameContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    difficultyName = new TruncatingSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                    },
                    mappedByLabel = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        // TODO: better null display? beatmap carousel panels also just show this text currently.
                        Text = " mapped by ",
                        Font = OsuFont.GetFont(size: 14),
                    },
                    mapperName = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 14))
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.Both,
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapInfo.BindValueChanged(b =>
            {
                difficultyName.Text = b.NewValue?.DifficultyName ?? string.Empty;
                updateMapper();
            }, true);
        }

        private void updateMapper()
        {
            mapperName.Clear();

            switch (beatmapInfo.Value)
            {
                case BeatmapInfo localBeatmap:
                    // TODO: should be the mapper of the guest difficulty, but that isn't stored correctly yet (see https://github.com/ppy/osu/issues/12965)
                    mapperName.AddUserLink(localBeatmap.Metadata.Author);
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            // truncate difficulty name when width exceeds bounds, prioritizing mapper name display
            difficultyName.MaxWidth = Math.Max(DrawWidth - mappedByLabel.DrawWidth
                                                         - mapperName.DrawWidth, 0);
        }
    }
}
