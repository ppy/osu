// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Dialog
{
    public partial class PopupDialogButton : DialogButton
    {
        public PopupDialogButton(HoverSampleSet sampleSet = HoverSampleSet.Button)
            : base(sampleSet)
        {
            Height = 50;
            TextSize = 18;
        }
    }
}
