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
        public bool ShowWhenFailing = true;

        private List<ElementDefinition> elements = new List<ElementDefinition>();
        public IEnumerable<ElementDefinition> Elements => elements;
        
        public LayerDefinition(string name, int depth)
        {
            Name = name;
            Depth = depth;
        }

        public void Add(ElementDefinition element)
        {
            elements.Add(element);
        }

        public StoryboardLayer CreateDrawable()
            => new StoryboardLayer(this) { Depth = Depth, };
    }
}
