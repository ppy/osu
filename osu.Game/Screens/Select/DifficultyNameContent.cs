// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Select
{
    public partial class DifficultyNameContent : FillFlowContainer
    {
        private OsuSpriteText difficultyName = null!;
        private OsuSpriteText mappedByLabel = null!;
        private LinkFlowContainer mapperName = null!;

        [Resolved]
        private IBindable<IBeatmapInfo?> beatmapInfo { get; set; } = null!;

        [Resolved]
        private IBindable<IBeatmapSetInfo?> beatmapSetInfo { get; set; } = null!;

        public DifficultyNameContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Horizontal;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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

            beatmapSetInfo.BindValueChanged(_ => updateMapper());
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

                case APIBeatmap apiBeatmap:
                    var beatmapSet = (APIBeatmapSet?)beatmapSetInfo.Value;

                    APIUser? user = beatmapSet?.RelatedUsers?.SingleOrDefault(u => u.OnlineID == apiBeatmap.AuthorID);

                    if (user != null)
                        mapperName.AddUserLink(user);
                    break;
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // truncate difficulty name when width exceeds bounds, prioritizing mapper name display
            difficultyName.MaxWidth = Math.Max(ChildSize.X - mappedByLabel.DrawWidth
                                                           - mapperName.DrawWidth, 0);
        }
    }
}
