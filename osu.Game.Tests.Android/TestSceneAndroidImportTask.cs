// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.App;
using Android.Provider;
using NUnit.Framework;
using osu.Android;
using osu.Framework.Testing;

namespace osu.Game.Tests.Android
{
    [TestFixture]
    public partial class TestSceneAndroidImportTask : TestScene
    {
        private AndroidImportTask? importTask;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create import task", () =>
            {
                var uri = DocumentsContract.BuildDocumentUri(
                    TestDocumentsProvider.AUTHORITY,
                    TestDocumentsProvider.DOCUMENT_ID);

                importTask = AndroidImportTask.Create(
                    Application.Context.ContentResolver!,
                    uri!).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestDocumentProviderFileDeletion()
        {
            AddAssert("import task created", () => importTask != null);
            AddAssert("document not deleted", () => !TestDocumentsProvider.Deleted);

            AddStep("delete file", () => importTask!.DeleteFile());

            AddAssert("document deleted", () => TestDocumentsProvider.Deleted);
        }
    }
}