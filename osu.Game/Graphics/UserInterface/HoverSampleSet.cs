// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Graphics.UserInterface
{
    public enum HoverSampleSet
    {
        [Description("default")]
        Default,

        [Description("button")]
        Button,

        [Description("button-sidebar")]
        ButtonSidebar,

        [Description("toolbar")]
        Toolbar,

        [Description("tabselect")]
        TabSelect,

        [Description("scrolltotop")]
        ScrollToTop,

        [Description("dialog-cancel")]
        DialogCancel,

        [Description("dialog-ok")]
        DialogOk
    }
}
