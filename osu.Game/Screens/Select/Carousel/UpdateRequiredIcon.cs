// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Select.Carousel
{
    public class UpdateRequiredIcon : OsuAnimatedButton
    {
        private readonly BeatmapSetInfo beatmapSetInfo;
        private SpriteIcon icon;

        public UpdateRequiredIcon(BeatmapSetInfo beatmapSetInfo)
        {
            this.beatmapSetInfo = beatmapSetInfo;

            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
        }

        [Resolved]
        private BeatmapModelDownloader beatmaps { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            const float icon_size = 14;

            Content.Anchor = Anchor.CentreLeft;
            Content.Origin = Anchor.CentreLeft;

            Content.AddRange(new Drawable[]
            {
                new FillFlowContainer
                {
                    Padding = new MarginPadding { Horizontal = 5, Vertical = 3 },
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Size = new Vector2(icon_size),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                icon = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.SyncAlt,
                                    Size = new Vector2(icon_size),
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.Default.With(weight: FontWeight.Bold),
                            Text = "Update",
                        }
                    }
                },
            });

            TooltipText = "Update beatmap with online changes";

            Action = () => beatmaps.Download(beatmapSetInfo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            icon.Spin(4000, RotationDirection.Clockwise);
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.Spin(400, RotationDirection.Clockwise);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.Spin(4000, RotationDirection.Clockwise);
            base.OnHoverLost(e);
        }
    }
}
