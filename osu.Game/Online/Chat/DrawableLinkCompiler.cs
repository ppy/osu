// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osuTK;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// An invisible drawable that brings multiple <see cref="Drawable"/> pieces together to form a consumable clickable link.
    /// </summary>
    public partial class DrawableLinkCompiler : OsuHoverContainer, IHasContextMenu
    {
        /// <summary>
        /// Each word part of a chat link (split for word-wrap support).
        /// </summary>
        public readonly List<Drawable> Parts;

        [Resolved(CanBeNull = true)]
        private OverlayColourProvider overlayColourProvider { get; set; }
        [Resolved]
        private OnScreenDisplay onScreenDisplay { get; set; }
        [Resolved]
        private GameHost host { get; set; } = null!;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        public DrawableLinkCompiler(ITextPart part)
            : this(part.Drawables.OfType<SpriteText>())
        {
        }

        public DrawableLinkCompiler(IEnumerable<Drawable> parts)
        {
            Parts = parts.ToList();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
        }

        protected override IEnumerable<Drawable> EffectTargets => Parts;

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();
                bool hasTooltip = this.TooltipText.ToString() != "";
                string text;
                if(hasTooltip)
                {
                    text = this.TooltipText.ToString();
                }
                else
                {
                    text = getUrlFromPart(Parts);
                }
                bool isChannelorLobbyLink = text.Contains("osu//chan") || text.Contains("osump//");
                if(!isChannelorLobbyLink)
                {
                    items.Add(new OsuMenuItem("Open", MenuItemType.Highlighted, () => host.OpenUrlExternally(text)));
                }
                items.Add(new OsuMenuItem("Copy URL", MenuItemType.Standard, () => 
                {
                    host.GetClipboard()?.SetText(text);
                    onScreenDisplay?.Display(new CopyUrlToast());
                }));

                return items.ToArray();
            }
        }

        private partial class LinkHoverSounds : HoverClickSounds
        {
            private readonly List<Drawable> parts;

            public LinkHoverSounds(HoverSampleSet sampleSet, List<Drawable> parts)
                : base(sampleSet)
            {
                this.parts = parts;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));
        }
        public string getUrlFromPart (List<Drawable> part)
        {
            string url = part[0].ToString();
            int startIndex = url.IndexOf('"') + 1;
            int endIndex = url.LastIndexOf('"');
            return url.Substring(startIndex, endIndex - startIndex);
        }
    }
}
