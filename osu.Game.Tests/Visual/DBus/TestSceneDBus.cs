// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Tests.Visual.DBus
{
    public class TestSceneDBus : ScreenTestScene
    {
        private DBusManager dbusServer;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Box
            {
                Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#000"), Color4Extensions.FromHex("#333")),
                RelativeSizeAxes = Axes.Both
            };

            Add(dbusServer = new DBusManager(false));

            dbusServer.RegisterNewObject(new Greet("osu.Game.Tests"));

            AddStep("启动/重启DBusServer", dbusServer.Connect);

            AddStep("停止DBusServer", () => dbusServer.Disconnect());

            AddStep("列出所有服务", () => Task.Run(dbusServer.GetAllServices));
        }
    }
}
