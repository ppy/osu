// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;

namespace osu.Game.Skinning
{
    public class SkinManager
    {
        public Bindable<Skin> CurrentSkin = new Bindable<Skin>(new DefaultSkin());
    }
}
