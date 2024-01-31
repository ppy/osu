// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.Versioning;
using NUnit.Framework;
using osu.Game.Updater;

namespace osu.Game.Tests.Updater
{
    [TestFixture]
    [SupportedOSPlatform("windows")]
    public class AssociationManagerTest
    {
        AssociationManager manager = new AssociationManager();

        [Test]
        [SupportedOSPlatform("windows")]
        public void TestRemoveAssociations()
        {
            manager.RemoveFileAssociations();
        }

        [Test]
        [SupportedOSPlatform("windows")]
        public void TestSetAssociations()
        {
            manager.InitializeFileAssociations();
        }

        [Test]
        [SupportedOSPlatform("windows")]
        public void TestEnsureAssociations()
        {
            bool isensured = manager.EnsureAssociationsSet();

            if (isensured)
            {
                Console.WriteLine("File associations are set up correctly!");
            }
            else
            {
                Console.WriteLine("File associations are not set up correctly!");
            }
        }
    }
}
