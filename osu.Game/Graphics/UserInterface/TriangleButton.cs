// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A button with moving triangles in the background.
    /// </summary>
    public class TriangleButton : OsuButton, IFilterable
    {
        protected Triangles Triangles { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Add(Triangles = new Triangles
            {
                RelativeSizeAxes = Axes.Both,
                ColourDark = colours.BlueDarker,
                ColourLight = colours.Blue,
            });
        }

        public virtual IEnumerable<string> FilterTerms => new[] { Text };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }
    }
}
