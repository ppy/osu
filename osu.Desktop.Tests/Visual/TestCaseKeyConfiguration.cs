// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using System.Collections.Generic;
using osu.Game.Graphics;

namespace osu.Desktop.Tests.Visual
{
    public class TestCaseKeyConfiguration : OsuTestCase
    {
        private readonly KeyConfiguration configuration;

        public override string Description => @"Key configuration";

        public TestCaseKeyConfiguration()
        {
            Child = configuration = new KeyConfiguration();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            configuration.Show();
        }
    }

    public class KeyConfiguration : SettingsOverlay
    {
        protected override IEnumerable<SettingsSection> CreateSections() => new[]
        {
            new BindingsSection(),
            new BindingsSection()
        };
    }

    public class BindingsSection : SettingsSection
    {
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Header => "Header";
    }
}
