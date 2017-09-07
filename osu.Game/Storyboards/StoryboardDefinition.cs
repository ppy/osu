// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;
using System.Linq;
using System;

namespace osu.Game.Storyboards
{
    public class StoryboardDefinition
    {
        private Dictionary<string, LayerDefinition> layers = new Dictionary<string, LayerDefinition>();
        public IEnumerable<LayerDefinition> Layers => layers.Values;
        
        public StoryboardDefinition()
        {
            layers.Add("Background", new LayerDefinition("Background", 3));
            layers.Add("Fail", new LayerDefinition("Fail", 2) { EnabledWhenPassing = false, });
            layers.Add("Pass", new LayerDefinition("Pass", 1) { ShowWhenFailing = false, });
            layers.Add("Foreground", new LayerDefinition("Foreground", 0));
        }

        public LayerDefinition GetLayer(string name)
        {
            LayerDefinition layer;
            if (!layers.TryGetValue(name, out layer))
                layers[name] = layer = new LayerDefinition(name, layers.Values.Min(l => l.Depth) - 1);

            return layer;
        }

        public Storyboard CreateDrawable()
            => new Storyboard(this);
    }
}
