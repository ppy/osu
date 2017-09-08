// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;

namespace osu.Game.Storyboards
{
    public class LayerDefinition
    {
        public string Name;
        public int Depth;
        public bool EnabledWhenPassing = true;
        public bool EnabledWhenFailing = true;

        private readonly List<IElementDefinition> elements = new List<IElementDefinition>();
        public IEnumerable<IElementDefinition> Elements => elements;

        public LayerDefinition(string name, int depth)
        {
            Name = name;
            Depth = depth;
        }

        public void Add(IElementDefinition element)
        {
            elements.Add(element);
        }

        public StoryboardLayer CreateDrawable()
            => new StoryboardLayer(this) { Depth = Depth, };
    }
}
