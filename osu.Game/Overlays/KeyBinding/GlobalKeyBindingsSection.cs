// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays.KeyBinding
{
    public class GlobalKeyBindingsSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Globe
        };

        public override string Header => "全局";

        public GlobalKeyBindingsSection(GlobalActionContainer manager)
        {
            Add(new DefaultBindingsSubsection(manager));
            Add(new AudioControlKeyBindingsSubsection(manager));
            Add(new MvisBindingsSection(manager));
            Add(new SongSelectKeyBindingSubsection(manager));
            Add(new InGameKeyBindingsSubsection(manager));
            Add(new EditorKeyBindingsSubsection(manager));
        }
        private class MvisBindingsSection : KeyBindingsSubsection
        {
            protected override string Header => "Mvis播放器";

            public MvisBindingsSection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.MvisControlKeyBindings;
            }
        }
        private class DefaultBindingsSubsection : KeyBindingsSubsection
        {
            protected override string Header => string.Empty;

            public DefaultBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.GlobalKeyBindings;
            }
        }

        private class SongSelectKeyBindingSubsection : KeyBindingsSubsection
        {
            protected override string Header => "歌曲选择";

            public SongSelectKeyBindingSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.SongSelectKeyBindings;
            }
        }

        private class InGameKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override string Header => "游戏内";

            public InGameKeyBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.InGameKeyBindings;
            }
        }

        private class AudioControlKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override string Header => "音频";

            public AudioControlKeyBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.AudioControlKeyBindings;
            }
        }

        private class EditorKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override string Header => "编辑器";

            public EditorKeyBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.EditorKeyBindings;
            }
        }
    }
}
