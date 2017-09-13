// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Storyboards
{
    public class Storyboard
    {
        private readonly Dictionary<string, StoryboardLayer> layers = new Dictionary<string, StoryboardLayer>();
        public IEnumerable<StoryboardLayer> Layers => layers.Values;

        public Storyboard()
        {
            layers.Add("Background", new StoryboardLayer("Background", 3));
            layers.Add("Fail", new StoryboardLayer("Fail", 2) { EnabledWhenPassing = false, });
            layers.Add("Pass", new StoryboardLayer("Pass", 1) { EnabledWhenFailing = false, });
            layers.Add("Foreground", new StoryboardLayer("Foreground", 0));
        }

        public StoryboardLayer GetLayer(string name)
        {
            StoryboardLayer layer;
            if (!layers.TryGetValue(name, out layer))
                layers[name] = layer = new StoryboardLayer(name, layers.Values.Min(l => l.Depth) - 1);

            return layer;
        }

        public DrawableStoryboard CreateDrawable()
            => new DrawableStoryboard(this);
    }
}
