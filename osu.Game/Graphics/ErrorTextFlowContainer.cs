// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Graphics
{
    public class ErrorTextFlowContainer : OsuTextFlowContainer
    {
        private readonly List<ITextPart> errorTextParts = new List<ITextPart>();

        public ErrorTextFlowContainer()
            : base(cp => cp.Font = cp.Font.With(size: 12))
        {
        }

        public void ClearErrors()
        {
            foreach (var textPart in errorTextParts)
                RemovePart(textPart);
        }

        public void AddErrors(string[] errors)
        {
            ClearErrors();

            if (errors == null) return;

            foreach (string error in errors)
                errorTextParts.Add(AddParagraph(error, cp => cp.Colour = Color4.Red));
        }
    }
}
