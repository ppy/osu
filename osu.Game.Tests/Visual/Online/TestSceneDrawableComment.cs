// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneDrawableComment : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private Container container;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                container = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };
        });

        [TestCaseSource(nameof(comments))]
        public void TestComment(string description, string text)
        {
            AddStep(description, () =>
            {
                comment.Pinned = description == "Pinned";
                comment.Message = text;
                container.Add(new DrawableComment(comment));
            });
        }

        private static readonly Comment comment = new Comment
        {
            Id = 1,
            LegacyName = "Test User",
            CreatedAt = DateTimeOffset.Now,
            VotesCount = 0,
        };

        private static object[] comments =
        {
            new[] { "Plain", "This is plain comment" },
            new[] { "Pinned", "This is pinned comment" },
            new[] { "Link", "Please visit https://osu.ppy.sh" },

            new[]
            {
                "Heading", @"# Heading 1
## Heading 2
### Heading 3
#### Heading 4
##### Heading 5
###### Heading 6"
            },

            // Taken from https://github.com/ppy/osu/issues/13993#issuecomment-885994077
            new[]
            {
                "Problematic", @"My tablet doesn't work :(
It's a Huion 420 and it's apparently incompatible with OpenTablet Driver. The warning I get is: ""DeviceInUseException: Device is currently in use by another kernel module. To fix this issue, please follow the instructions from https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Linux-FAQ#arg    umentoutofrangeexception-value-0-15"" and it repeats 4 times on the notification before logging subsequent warnings.
Checking the logs, it looks for other Huion tablets before sending the notification (e.g.
 ""2021-07-23 03:52:33 [verbose]: Detect: Searching for tablet 'Huion WH1409 V2'
 20 2021-07-23 03:52:33 [error]: DeviceInUseException: Device is currently in use by another kernel module. To fix this     issue, please follow the instructions from https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Linux-FAQ#arg    umentoutofrangeexception-value-0-15"")
I use an Arch based installation of Linux and the tablet runs perfectly with Digimend kernel driver, with area configuration, pen pressure, etc. On osu!lazer the cursor disappears until I set it to ""Borderless"" instead of ""Fullscreen"" and even after it shows up, it goes to the bottom left corner as soon as a map starts.
I have honestly 0 idea of whats going on at this point."
            }
        };
    }
}
