// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Linq;
using osu.Framework.Graphics.Containers;

namespace osu.Mods.Evast.Galaga
{
    public class GalagaObject : Container
    {
        protected readonly BulletsContainer BulletsContainer;

        public GalagaObject(BulletsContainer bulletsContainer)
        {
            BulletsContainer = bulletsContainer;
        }

        protected override void Update()
        {
            base.Update();

            if (!BulletsContainer.Children.Any())
                return;
        }
    }
}
