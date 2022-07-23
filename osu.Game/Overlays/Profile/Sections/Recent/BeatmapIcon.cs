// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class BeatmapIcon : Container
    {
        private readonly SpriteIcon icon;
        private readonly BeatmapActionType type;

        public enum BeatmapActionType
        {
            PlayedTimes,
            Qualified,
            Deleted,
            Revived,
            Updated,
            Submitted
        }

        public BeatmapIcon(BeatmapActionType type)
        {
            this.type = type;
            Child = icon = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            icon.Icon = getIcon(type);
            icon.Colour = getColor(type, colours);
        }

        private IconUsage getIcon(BeatmapActionType type)
        {
            switch (type)
            {
                case BeatmapActionType.Qualified:

                case BeatmapActionType.Submitted:
                    return FontAwesome.Solid.ArrowUp;

                case BeatmapActionType.Updated:
                    return FontAwesome.Solid.SyncAlt;

                case BeatmapActionType.Revived:
                    return FontAwesome.Solid.TrashRestore;

                case BeatmapActionType.Deleted:
                    return FontAwesome.Solid.TrashAlt;

                case BeatmapActionType.PlayedTimes:
                    return FontAwesome.Solid.Play;

                default:
                    return FontAwesome.Solid.Map;
            }
        }

        private Color4 getColor(BeatmapActionType type, OsuColour colours)
        {
            switch (type)
            {
                case BeatmapActionType.Qualified:
                    return colours.Blue1;

                case BeatmapActionType.Submitted:
                    return colours.Yellow;

                case BeatmapActionType.Updated:
                    return colours.Lime1;

                case BeatmapActionType.Deleted:
                    return colours.Red1;

                default:
                    return Color4.White;
            }
        }
    }
}
