// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Screens.Edit.Screens.Compose.Layers;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit.Layers
{
    public class ManiaHitObjectMaskLayer : HitObjectMaskLayer
    {
        public readonly IBindable<bool> Inverted = new Bindable<bool>();

        public ManiaHitObjectMaskLayer()
        {
            Inverted.ValueChanged += invertedChanged;
        }

        private void invertedChanged(bool newValue)
        {
            Scale = new Vector2(1, newValue ? -1 : 1);
        }
    }
}
