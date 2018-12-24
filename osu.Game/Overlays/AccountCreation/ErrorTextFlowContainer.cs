// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Overlays.AccountCreation
{
    public class ErrorTextFlowContainer : OsuTextFlowContainer
    {
        private readonly List<Drawable> errorDrawables = new List<Drawable>();

        public ErrorTextFlowContainer()
            : base(cp => { cp.TextSize = 12; })
        {
        }

        public void ClearErrors()
        {
            errorDrawables.ForEach(d => d.Expire());
        }

        public void AddErrors(string[] errors)
        {
            ClearErrors();

            if (errors == null) return;

            foreach (var error in errors)
                errorDrawables.AddRange(AddParagraph(error, cp => cp.Colour = Color4.Red));
        }
    }
}
