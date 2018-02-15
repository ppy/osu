// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public abstract class Skin
    {
        public string Name { get; }

        public abstract Drawable GetComponent(string componentName);

        protected Skin(string name)
        {
            Name = name;
        }
    }
}
