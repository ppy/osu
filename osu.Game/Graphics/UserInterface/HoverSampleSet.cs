// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

    public static class HoverSampleSetExtensions
    {
        public static string GetResourceName(this HoverSampleSet value)
        {
            switch (value)
            {
                case HoverSampleSet.Default:
                    return "default";

                case HoverSampleSet.Button:
                    return "button";

                case HoverSampleSet.ButtonSidebar:
                    return "button-sidebar";

                case HoverSampleSet.TabSelect:
                    return "tabselect";

                case HoverSampleSet.DialogCancel:
                    return "dialog-cancel";

                case HoverSampleSet.DialogOk:
                    return "dialog-ok";

                case HoverSampleSet.MenuOpen:
                    return "menu-open";

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
