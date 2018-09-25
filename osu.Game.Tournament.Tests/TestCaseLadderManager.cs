// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens.Ladder;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseLadderManager : OsuTestCase
    {
        [Cached]
        private readonly LadderManager manager;

        public TestCaseLadderManager()
        {
            var ladder = File.Exists(@"bracket.json") ? JsonConvert.DeserializeObject<LadderInfo>(File.ReadAllText(@"bracket.json")) : new LadderInfo();

            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = manager = new LadderManager(ladder)
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            File.WriteAllText(@"bracket.json", JsonConvert.SerializeObject(manager.CreateInfo(),
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }));
        }
    }
}
