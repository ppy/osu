// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;

namespace osu.Game.Storyboards
{
    public class StoryboardLayer
    {
        public readonly string Name;

        public readonly int Depth;

        public readonly bool Masking;

        public bool VisibleWhenPassing = true;

        public bool VisibleWhenFailing = true;

        public List<IStoryboardElement> Elements = new List<IStoryboardElement>();

        public StoryboardLayer(string name, int depth, bool masking = true)
        {
            Name = name;
            Depth = depth;
            Masking = masking;
        }

        public void Add(IStoryboardElement element)
        {
            Elements.Add(element);
        }

        public virtual DrawableStoryboardLayer CreateDrawable()
            => new DrawableStoryboardLayer(this) { Depth = Depth, Name = Name };
    }
}
