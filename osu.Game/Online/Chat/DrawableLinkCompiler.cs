// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
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

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private LinkDetails link;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override HoverClickSounds CreateHoverClickSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        public DrawableLinkCompiler(IEnumerable<Drawable> parts, LinkDetails link)
        {
            Parts = parts.ToList();
            this.link = link;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, OsuGame game, BeatmapManager beatmapManager)
        {
            IdleColour = colours.Blue;
            this.game = game;
            this.beatmapManager = beatmapManager;
        }

        protected override IEnumerable<Drawable> EffectTargets => Parts;

        private BeatmapInfo getBeatmapFromLink(LinkDetails link)
        {
            var id = getBeatmapIdFromLink(link);
            if (id.Type == StoreId.IdType.Beatmap)
                return beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == id.Id);
            else
                return beatmapManager.QueryBeatmap(b => b.BeatmapSet.OnlineBeatmapSetID == id.Id);
        }

        private StoreId getBeatmapIdFromLink(LinkDetails link)
        {
            if (link.Action == LinkAction.OpenBeatmap && link.Argument != null && int.TryParse(link.Argument.Contains('?') ? link.Argument.Split('?')[0] : link.Argument, out int beatmapId))
                return new StoreId(beatmapId, StoreId.IdType.Beatmap);
            if (link.Action == LinkAction.OpenBeatmapSet && int.TryParse(link.Argument, out int setId))
                return new StoreId(setId, StoreId.IdType.BeatmapSet);

            return new StoreId(0, StoreId.IdType.Beatmap);
        }

        private class StoreId
        {
            public int Id { get; }
            public IdType Type { get; }

            public StoreId(int id, IdType type)
            {
                Id = id;
                Type = type;
            }

            public enum IdType
            {
                Beatmap,
                BeatmapSet
            }
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();
                var map = getBeatmapFromLink(link);

                switch (link.Action)
                {
                    case LinkAction.OpenBeatmap:
                        items.Add(new OsuMenuItem("Go to beatmap", MenuItemType.Highlighted)
                        {
                            Action =
                            {
                                Value = () => game?.PresentBeatmap(map.BeatmapSet, b => b.OnlineBeatmapID == map.OnlineBeatmapID),
                                Disabled = map == null
                            }
                        });

                        items.Add(new OsuMenuItem("Details", MenuItemType.Standard)
                        {
                            Action =
                            {
                                Value = () => game?.ShowBeatmap(getBeatmapIdFromLink(link).Id)
                            }
                        });

                        return items.ToArray();

                    case LinkAction.OpenBeatmapSet:
                        items.Add(new OsuMenuItem("Go to beatmap set", MenuItemType.Highlighted)
                        {
                            Action =
                            {
                                Value = () => game?.PresentBeatmap(map.BeatmapSet),
                                Disabled = map == null
                            }
                        });

                        items.Add(new OsuMenuItem("Details", MenuItemType.Standard)
                        {
                            Action =
                            {
                                Value = () => game?.ShowBeatmapSet(getBeatmapIdFromLink(link).Id)
                            }
                        });

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
