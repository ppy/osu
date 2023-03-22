// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class GlobalKeyBindingsSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Globe
        };

        public override LocalisableString Header => InputSettingsStrings.GlobalKeyBindingHeader;

        public GlobalKeyBindingsSection(GlobalActionContainer manager)
        {
            Add(new DefaultBindingsSubsection(manager));
            Add(new OverlayBindingsSubsection(manager));
            Add(new AudioControlKeyBindingsSubsection(manager));
            Add(new SongSelectKeyBindingSubsection(manager));
            Add(new InGameKeyBindingsSubsection(manager));
            Add(new ReplayKeyBindingsSubsection(manager));
            Add(new EditorKeyBindingsSubsection(manager));
        }

        private partial class DefaultBindingsSubsection : KeyBindingsSubsection
        {
            protected override LocalisableString Header => string.Empty;

            public DefaultBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.GlobalKeyBindings;
            }
        }

        private partial class OverlayBindingsSubsection : KeyBindingsSubsection
        {
            protected override LocalisableString Header => InputSettingsStrings.OverlaysSection;

            public OverlayBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.OverlayKeyBindings;
            }
        }

        private partial class SongSelectKeyBindingSubsection : KeyBindingsSubsection
        {
            protected override LocalisableString Header => InputSettingsStrings.SongSelectSection;

            public SongSelectKeyBindingSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.SongSelectKeyBindings;
            }
        }

        private partial class InGameKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override LocalisableString Header => InputSettingsStrings.InGameSection;

            public InGameKeyBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.InGameKeyBindings;
            }
        }

        private partial class ReplayKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override LocalisableString Header => InputSettingsStrings.ReplaySection;

            public ReplayKeyBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.ReplayKeyBindings;
            }
        }

        private partial class AudioControlKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override LocalisableString Header => InputSettingsStrings.AudioSection;

            public AudioControlKeyBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.AudioControlKeyBindings;
            }
        }

        private partial class EditorKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override LocalisableString Header => InputSettingsStrings.EditorSection;

            public EditorKeyBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.EditorKeyBindings;
            }
        }
    }
}
