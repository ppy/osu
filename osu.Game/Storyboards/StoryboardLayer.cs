﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;

namespace osu.Game.Storyboards
{
    public class StoryboardLayer
    {
        public string Name;
        public int Depth;
        public bool EnabledWhenPassing = true;
        public bool EnabledWhenFailing = true;

        private readonly List<IStoryboardElement> elements = new List<IStoryboardElement>();
        public IEnumerable<IStoryboardElement> Elements => elements;

        public StoryboardLayer(string name, int depth)
        {
            Name = name;
            Depth = depth;
        }

        public void Add(IStoryboardElement element)
        {
            elements.Add(element);
        }

        public DrawableStoryboardLayer CreateDrawable()
            => new DrawableStoryboardLayer(this) { Depth = Depth, };
    }
}
