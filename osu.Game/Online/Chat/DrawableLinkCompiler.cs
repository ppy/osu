// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Cursor;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// An invisible drawable that brings multiple <see cref="SpriteText"/> pieces together to form a consumable clickable link.
    /// </summary>
    public class DrawableLinkCompiler : OsuHoverContainer, IHasTooltip
    {
        /// <summary>
        /// Each word part of a chat link (split for word-wrap support).
        /// </summary>
        public List<SpriteText> Parts;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override HoverClickSounds CreateHoverClickSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        public DrawableLinkCompiler(IEnumerable<SpriteText> parts)
        {
            Parts = parts.ToList();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.Blue;
        }

        protected override IEnumerable<Drawable> EffectTargets => Parts;

        public string TooltipText { get; set; }

        private class LinkHoverSounds : HoverClickSounds
        {
            private readonly List<SpriteText> parts;

            public LinkHoverSounds(HoverSampleSet sampleSet, List<SpriteText> parts)
                : base(sampleSet)
            {
                this.parts = parts;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));
        }
    }
}
