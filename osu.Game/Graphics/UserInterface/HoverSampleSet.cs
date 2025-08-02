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

        [Description("tabselect")]
        TabSelect,

        [Description("dialog-cancel")]
        DialogCancel,

        [Description("dialog-ok")]
        DialogOk,

        [Description("menu-open")]
        MenuOpen,
    }
}
