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

        private OsuGame game;

        public LinkDetails Link;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override HoverClickSounds CreateHoverClickSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        public DrawableLinkCompiler(IEnumerable<Drawable> parts)
        {
            Parts = parts.ToList();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuGame game)
        {
            IdleColour = colours.Blue;
            this.game = game;
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

                        if (game?.GetBeatmapIdFromLink(Link) != null)
                        {
                            OsuMenuItem goToBeatmap;
                            beatmapId = (int)game?.GetBeatmapIdFromLink(Link);

                            if (game?.GetBeatmapFromId(beatmapId) != null)
                                goToBeatmap = new OsuMenuItem("Go To Beatmap", MenuItemType.Highlighted, () => game?.SelectBeatmap(beatmapId));
                            else
                            {
                                goToBeatmap = new OsuMenuItem("Go To Beatmap", MenuItemType.Highlighted);
                                goToBeatmap.Action.Disabled = true;
                            }

                            items.Add(goToBeatmap);
                        }

                        items.Add(new OsuMenuItem("Details", MenuItemType.Standard, () => game?.ShowBeatmap(beatmapId)));
                        return items.ToArray();

                    case LinkAction.OpenBeatmapSet:
                        int setId = 0;

                        if (game?.GetBeatmapSetIdFromLink(Link) != null)
                        {
                            OsuMenuItem goToBeatmapSet;
                            setId = (int)game?.GetBeatmapSetIdFromLink(Link);

                            if (game?.GetBeatmapSetFromId(setId) != null)
                                goToBeatmapSet = new OsuMenuItem("Go To Beatmapset", MenuItemType.Highlighted, () => game?.SelectBeatmapSet(setId));
                            else
                            {
                                goToBeatmapSet = new OsuMenuItem("Go To Beatmapset", MenuItemType.Highlighted);
                                goToBeatmapSet.Action.Disabled = true;
                            }

                            items.Add(goToBeatmapSet);
                        }

                        items.Add(new OsuMenuItem("Details", MenuItemType.Standard, () => game?.ShowBeatmapSet(setId)));
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
