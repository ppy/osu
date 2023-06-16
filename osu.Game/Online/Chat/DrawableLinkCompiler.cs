// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public readonly LinkDetails Link;

        [Resolved]
        private OverlayColourProvider? overlayColourProvider { get; set; }

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        public DrawableLinkCompiler(ITextPart part, LinkDetails link)
            : this(part.Drawables.OfType<SpriteText>(), link)
        {
        }

        public DrawableLinkCompiler(IEnumerable<Drawable> parts, LinkDetails link)
        {
            Parts = parts.ToList();
            Link = link;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;

            Action ??= () =>
            {
                Debug.Assert(Link.Action != LinkAction.Custom);

                if (linkHandler != null)
                    linkHandler.HandleLink(Link);
                // fallback to handle cases where OsuGame is not available, ie. tournament client.
                else if (Link.Action == LinkAction.External)
                    host.OpenUrlExternally(Link.Argument.ToString());
            };
        }

        protected override IEnumerable<Drawable> EffectTargets => Parts;

        public MenuItem[] ContextMenuItems
        {
            get
            {
                string? url = MessageFormatter.GetUrl(Link);

                if (url == null)
                    return Array.Empty<MenuItem>();

                return new MenuItem[]
                {
                    new OsuMenuItem("Open", MenuItemType.Highlighted, Action),
                    new OsuMenuItem("Copy URL", MenuItemType.Standard, () =>
                    {
                        host.GetClipboard()?.SetText(url);
                        onScreenDisplay?.Display(new CopyUrlToast());
                    })
                };
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
    }
}
