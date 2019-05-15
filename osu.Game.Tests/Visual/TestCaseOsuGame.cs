// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Menu;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseOsuGame : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuLogo),
        };

        private IReadOnlyList<Type> requiredGameDependencies => new[]
        {
            typeof(OsuGame),
            typeof(RavenLogger),
            typeof(Bindable<RulesetInfo>),
            typeof(IBindable<RulesetInfo>),
            typeof(OsuLogo),
            typeof(IdleTracker),
            typeof(OnScreenDisplay),
            typeof(NotificationOverlay),
            typeof(DirectOverlay),
            typeof(SocialOverlay),
            typeof(ChannelManager),
            typeof(ChatOverlay),
            typeof(SettingsOverlay),
            typeof(UserProfileOverlay),
            typeof(BeatmapSetOverlay),
            typeof(LoginOverlay),
            typeof(MusicController),
            typeof(AccountCreationOverlay),
            typeof(DialogOverlay),
        };

        private IReadOnlyList<Type> requiredGameBaseDependencies => new[]
        {
            typeof(OsuGameBase),
            typeof(DatabaseContextFactory),
            typeof(LargeTextureStore),
            typeof(OsuConfigManager),
            typeof(SkinManager),
            typeof(ISkinSource),
            typeof(IAPIProvider),
            typeof(RulesetStore),
            typeof(FileStore),
            typeof(ScoreManager),
            typeof(BeatmapManager),
            typeof(KeyBindingStore),
            typeof(SettingsStore),
            typeof(RulesetConfigCache),
            typeof(OsuColour),
            typeof(IBindable<WorkingBeatmap>),
            typeof(Bindable<WorkingBeatmap>),
            typeof(GlobalActionContainer),
            typeof(PreviewTrackManager),
        };

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            OsuGame game = new OsuGame();
            game.SetHost(host);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                game
            };

            AddUntilStep("wait for load", () => game.IsLoaded);

            AddAssert("check OsuGame DI members", () => requiredGameDependencies.All(d => game.Dependencies.Get(d) != null));
            AddAssert("check OsuGameBase DI members", () => requiredGameBaseDependencies.All(d => Dependencies.Get(d) != null));
        }
    }
}
