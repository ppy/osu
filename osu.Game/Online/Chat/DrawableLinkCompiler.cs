﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ListExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Lists;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// An invisible drawable that brings multiple <see cref="Drawable"/> pieces together to form a consumable clickable link.
    /// </summary>
    public partial class DrawableLinkCompiler : OsuHoverContainer
    {
        /// <summary>
        /// Each word part of a chat link (split for word-wrap support).
        /// </summary>
        public readonly SlimReadOnlyListWrapper<Drawable> Parts;

        [Resolved]
        private OverlayColourProvider? overlayColourProvider { get; set; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            foreach (var part in Parts)
            {
                if (part.ReceivePositionalInputAt(screenSpacePos))
                    return true;
            }

            return false;
        }

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        public DrawableLinkCompiler(ITextPart part)
            : this(part.Drawables.OfType<SpriteText>())
        {
        }

        public DrawableLinkCompiler(IEnumerable<Drawable> parts)
        {
            Parts = parts.ToList().AsSlimReadOnly();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
        }

        protected override IEnumerable<Drawable> EffectTargets => Parts;

        private partial class LinkHoverSounds : HoverClickSounds
        {
            private readonly SlimReadOnlyListWrapper<Drawable> parts;

            public LinkHoverSounds(HoverSampleSet sampleSet, SlimReadOnlyListWrapper<Drawable> parts)
                : base(sampleSet)
            {
                this.parts = parts;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                foreach (var part in parts)
                {
                    if (part.ReceivePositionalInputAt(screenSpacePos))
                        return true;
                }

                return false;
            }
        }
    }
}
