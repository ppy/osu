// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using File = Java.IO.File;

namespace osu.Game.Tests.Android
{
#pragma warning disable CS8765 // Nullability annotations in the Android bindings do not match the overridden members consistently.

    [ContentProvider(
        new[] { AUTHORITY },
        Exported = true,
        GrantUriPermissions = true,
        Permission = "android.permission.MANAGE_DOCUMENTS")]
    [IntentFilter(new[] { DocumentsContract.ProviderInterface })]
    public class TestDocumentsProvider : DocumentsProvider
    {
        public const string AUTHORITY = "sh.ppy.osulazer.tests.documents";
        public const string DOCUMENT_ID = "test.osz";

        private static readonly string[] default_root_projection =
        {
            DocumentsContract.Root.ColumnRootId,
            DocumentsContract.Root.ColumnDocumentId,
            DocumentsContract.Root.ColumnTitle,
            DocumentsContract.Root.ColumnFlags,
        };

        private static readonly string[] default_document_projection =
        {
            DocumentsContract.Document.ColumnDocumentId,
            DocumentsContract.Document.ColumnDisplayName,
            DocumentsContract.Document.ColumnMimeType,
            DocumentsContract.Document.ColumnFlags,
            DocumentsContract.Document.ColumnSize,
        };

        public static bool Deleted { get; private set; }

        private File testFile = null!;

        public override bool OnCreate()
        {
            testFile = new File(Context!.CacheDir, DOCUMENT_ID);

            createTestFile();

            return true;
        }

        public override ICursor QueryRoots(string[]? projection)
        {
            return new MatrixCursor(projection ?? default_root_projection);
        }

        public override ICursor QueryDocument(string? documentId, string[]? projection)
        {
            // Recreate the file when the test is executed more than once in the
            // same process.
            createTestFile();

            string[] columns = projection ?? default_document_projection;

            var cursor = new MatrixCursor(columns);
            MatrixCursor.RowBuilder row = cursor.NewRow()!;

            foreach (string? column in columns)
            {
                switch (column)
                {
                    case DocumentsContract.Document.ColumnDocumentId:
                        row.Add(DOCUMENT_ID);
                        break;

                    case DocumentsContract.Document.ColumnDisplayName:
                        row.Add(DOCUMENT_ID);
                        break;

                    case DocumentsContract.Document.ColumnMimeType:
                        row.Add("application/x-osu-beatmap-archive");
                        break;

                    case DocumentsContract.Document.ColumnFlags:
                        row.Add((int)DocumentContractFlags.SupportsDelete);
                        break;

                    case DocumentsContract.Document.ColumnSize:
                        row.Add(testFile.Length());
                        break;

                    default:
                        row.Add(null);
                        break;
                }
            }

            return cursor;
        }

        public override ICursor QueryChildDocuments(string? parentDocumentId, string[]? projection, string? sortOrder)
        {
            return new MatrixCursor(projection ?? default_document_projection);
        }

        public override ParcelFileDescriptor OpenDocument(string? documentId, string? mode, CancellationSignal? signal)
        {
            return ParcelFileDescriptor.Open(testFile, ParcelFileMode.ReadOnly)!;
        }

        public override void DeleteDocument(string? documentId)
        {
            Deleted = true;
            testFile.Delete();
        }

        private void createTestFile()
        {
            Deleted = false;

            using var stream = new FileStream(testFile.AbsolutePath!, FileMode.Create, FileAccess.Write);
            stream.WriteByte(0);
        }
    }

#pragma warning restore CS8765
}