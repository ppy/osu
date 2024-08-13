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
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2.Wedge
{
    public partial class DifficultyNameContent : CompositeDrawable
    {
        private OsuSpriteText difficultyName = null!;
        private OsuSpriteText mappedByLabel = null!;
        private OsuHoverContainer mapperLink = null!;
        private OsuSpriteText mapperName = null!;

        [Resolved]
        private IBindable<IBeatmapInfo?> beatmapInfo { get; set; } = null!;

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

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
                    mapperLink = new MapperLink
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.Both,
                        Child = mapperName = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 14),
                        }
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
            mapperName.Text = string.Empty;

            switch (beatmapInfo.Value)
            {
                case BeatmapInfo localBeatmap:
                    // TODO: should be the mapper of the guest difficulty, but that isn't stored correctly yet (see https://github.com/ppy/osu/issues/12965)
                    mapperName.Text = localBeatmap.Metadata.Author.Username;
                    mapperLink.Action = () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, localBeatmap.Metadata.Author));
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

        /// <summary>
        /// This class is a workaround for the single-frame layout issues with `{Link|Text|Fill}FlowContainer`s.
        /// See https://github.com/ppy/osu-framework/issues/3369.
        /// </summary>
        private partial class MapperLink : OsuHoverContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
            {
                TooltipText = ContextMenuStrings.ViewProfile;
                IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
            }
        }
    }
}
