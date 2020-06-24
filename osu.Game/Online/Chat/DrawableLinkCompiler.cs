// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// An invisible drawable that brings multiple <see cref="Drawable"/> pieces together to form a consumable clickable link.
    /// </summary>
    public class DrawableLinkCompiler : OsuHoverContainer, IHasContextMenu
    {
        /// <summary>
        /// Each word part of a chat link (split for word-wrap support).
        /// </summary>
        public List<Drawable> Parts;

        public OsuGame Game;

        public LinkDetails Link;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override HoverClickSounds CreateHoverClickSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        public DrawableLinkCompiler(IEnumerable<Drawable> parts)
        {
            Parts = parts.ToList();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.Blue;
        }

        protected override IEnumerable<Drawable> EffectTargets => Parts;

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                switch (Link.Action)
                {
                    case LinkAction.OpenBeatmap:
                        int beatmapId = 0;

                        if (Game?.GetBeatmapIdFromLink(Link) != null)
                        {
                            OsuMenuItem goToBeatmap;
                            beatmapId = (int)Game?.GetBeatmapIdFromLink(Link);
                            if (Game?.GetBeatmapFromId(beatmapId) != null)
                                goToBeatmap = new OsuMenuItem("Go To Beatmap", MenuItemType.Highlighted, () => Game?.SelectBeatmap(beatmapId));
                            else
                            {
                                goToBeatmap = new OsuMenuItem("Go To Beatmap", MenuItemType.Highlighted);
                                goToBeatmap.Action.Disabled = true;
                            }
                            items.Add(goToBeatmap);
                        }

                        items.Add(new OsuMenuItem("Details", MenuItemType.Standard, () => Game?.ShowBeatmap(beatmapId)));
                        return items.ToArray();

                    case LinkAction.OpenBeatmapSet:
                        int setId = 0;

                        if (Game?.GetBeatmapSetIdFromLink(Link) != null)
                        {
                            OsuMenuItem goToBeatmapSet;
                            setId = (int)Game?.GetBeatmapSetIdFromLink(Link);
                            if (Game?.GetBeatmapSetFromId(setId) != null)
                                goToBeatmapSet = new OsuMenuItem("Go To Beatmapset", MenuItemType.Highlighted, () => Game?.SelectBeatmapSet(setId));
                            else
                            {
                                goToBeatmapSet = new OsuMenuItem("Go To Beatmapset", MenuItemType.Highlighted);
                                goToBeatmapSet.Action.Disabled = true;
                            }
                            items.Add(goToBeatmapSet);
                        }

                        items.Add(new OsuMenuItem("Details", MenuItemType.Standard, () => Game?.ShowBeatmapSet(setId)));
                        return items.ToArray();

                    default:
                        return Array.Empty<MenuItem>();
                }
            }
        }

        private class LinkHoverSounds : HoverClickSounds
        {
            private readonly List<Drawable> parts;

            public LinkHoverSounds(HoverSampleSet sampleSet, List<Drawable> parts)
                : base(sampleSet)
            {
                this.parts = parts;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));
        }
    }
}
