// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelRandom : MatchmakingSelectPanel
    {
        public new MatchmakingPlaylistItemRandom Item => (MatchmakingPlaylistItemRandom)base.Item;

        public MatchmakingSelectPanelRandom(MatchmakingPlaylistItemRandom item)
            : base(item)
        {
        }

        private Container? randomPanelContent;
        private BeatmapCardMatchmakingBeatmapContent? beatmapPanelContent;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Add(randomPanelContent = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background2,
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children =
                        [
                            new SpriteIcon
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(32),
                                Icon = FontAwesome.Solid.Random,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = "Random",
                            }
                        ]
                    },
                }
            });
        }

        public override void PresentAsChosenBeatmap(MatchmakingPlaylistItemBeatmap item)
        {
            ShowChosenBorder();

            this.MoveTo(Vector2.Zero, 1000, Easing.OutExpo)
                .ScaleTo(1.5f, 1000, Easing.OutExpo);

            randomPanelContent?.Expire();

            var flashLayer = new Box { RelativeSizeAxes = Axes.Both };

            AddRange(new Drawable[]
            {
                beatmapPanelContent = new BeatmapCardMatchmakingBeatmapContent(item.Beatmap, item.Mods),
                flashLayer,
            });

            flashLayer.FadeOutFromOne(1000, Easing.In).Expire();
        }

        protected override float AvatarOverlayOffset => base.AvatarOverlayOffset + (beatmapPanelContent?.AvatarOffset ?? 0);
    }
}
