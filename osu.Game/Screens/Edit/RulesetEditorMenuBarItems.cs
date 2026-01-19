// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Allows for providing custom menu items to be appended to the <see cref="Editor"/>'s menu bar.
    /// </summary>
    public abstract partial class RulesetEditorMenuBarItems : Component
    {
        /// <summary>
        /// Creates an enumerable with menu items to be appended to the <see cref="Editor"/>'s menu bar for the supplied <paramref name="type"/>.
        /// </summary>
        public abstract IEnumerable<MenuItem> CreateMenuItems(EditorMenuBarItemType type);

        public sealed partial class Default : RulesetEditorMenuBarItems
        {
            public override IEnumerable<MenuItem> CreateMenuItems(EditorMenuBarItemType type) => Enumerable.Empty<MenuItem>();
        }
    }

    public enum EditorMenuBarItemType
    {
        File,
        Edit,
        View,
        Timing
    }
}
