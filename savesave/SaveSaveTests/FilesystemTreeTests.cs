using Microsoft.VisualStudio.TestTools.UnitTesting;
using savesave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace savesave.Tests
{
    [TestClass]
    public class FilesystemTreeTests
    {

        string path;
        FilesystemTree tree;

        [TestInitialize]
        public void SetUp()
        {
            path = Path.GetFullPath(Path.GetRandomFileName());
            path = Path.Combine(path, "tree");
            Directory.CreateDirectory(path);
            tree = new FilesystemTree(path);
        }

        [TestMethod]
        public void EmptyTreeHasEntry()
        {
            List<string> entries = new List<string>(tree.EnumerateEntries());
            CollectionAssert.AreEqual(new string[] { "" }, entries);
        }

        [TestMethod]
        public void EmptyTreeModifiedDate()
        {
            DateTime time = new DateTime(2000, 9, 8, 7, 6, 5);
            Directory.SetLastWriteTimeUtc(path, time);
            Assert.AreEqual(time, tree.LastWriteTimeUtc());
        }

        [TestMethod]
        public void FileModifiedDate()
        {
            DateTime time1 = new DateTime(2000, 1, 8, 7, 6, 1);
            DateTime time2 = new DateTime(2000, 2, 8, 7, 6, 2);

            string fpath = Path.Combine(path, "foo");
            File.Create(fpath).Close();

            Directory.SetLastWriteTimeUtc(path, time1);
            File.SetLastWriteTimeUtc(fpath, time1);
            Assert.AreEqual(time1, tree.LastWriteTimeUtc());

            File.SetLastWriteTimeUtc(fpath, time2);
            Assert.AreEqual(time2, tree.LastWriteTimeUtc());

            File.SetLastWriteTimeUtc(fpath, time1);
            Assert.AreEqual(time1, tree.LastWriteTimeUtc());

            Directory.SetLastWriteTimeUtc(path, time2);
            Assert.AreEqual(time2, tree.LastWriteTimeUtc());
        }

        [TestMethod]
        public void NestedMofidiedDate()
        {

            DateTime time1 = new DateTime(2000, 1, 8, 7, 6, 1);
            DateTime time2 = new DateTime(2000, 2, 8, 7, 6, 2);

            string dpath = Path.Combine(path, "foo");
            Directory.CreateDirectory(dpath);

            string fpath = Path.Combine(path, "foo", "bar");
            File.Create(fpath).Close();

            Directory.SetLastWriteTimeUtc(path, time1);
            Directory.SetLastWriteTimeUtc(dpath, time1);
            File.SetLastWriteTimeUtc(fpath, time1);
            Assert.AreEqual(time1, tree.LastWriteTimeUtc());

            File.SetLastWriteTimeUtc(fpath, time2);
            Assert.AreEqual(time2, tree.LastWriteTimeUtc());

            File.SetLastWriteTimeUtc(fpath, time1);
            Assert.AreEqual(time1, tree.LastWriteTimeUtc());

            Directory.SetLastWriteTimeUtc(dpath, time2);
            Assert.AreEqual(time2, tree.LastWriteTimeUtc());
        }

        [TestMethod]
        public void EmptyEqual()
        {
            string foo = Path.Combine(path, "foo");
            string bar = Path.Combine(path, "bar");

            Directory.CreateDirectory(foo);
            Directory.CreateDirectory(bar);

            FilesystemTree tfoo = new FilesystemTree(foo);
            FilesystemTree tbar = new FilesystemTree(bar);

            CollectionAssert.AreEqual(tfoo.ContentDigest(), tbar.ContentDigest());
        }

        [TestMethod]
        public void NestedEqual()
        {
            string foo = Path.Combine(path, "foo");
            string bar = Path.Combine(path, "bar");

            Directory.CreateDirectory(foo);
            Directory.CreateDirectory(bar);

            FilesystemTree tfoo = new FilesystemTree(foo);
            FilesystemTree tbar = new FilesystemTree(bar);

            CollectionAssert.AreEqual(tfoo.ContentDigest(), tbar.ContentDigest());

            Directory.CreateDirectory(Path.Combine(foo, "subdir"));
            CollectionAssert.AreNotEqual(tfoo.ContentDigest(), tbar.ContentDigest());
            Directory.CreateDirectory(Path.Combine(bar, "subdir"));

            CollectionAssert.AreEqual(tfoo.ContentDigest(), tbar.ContentDigest());

            File.Create(Path.Combine(foo, "subdir", "file")).Close();
            CollectionAssert.AreNotEqual(tfoo.ContentDigest(), tbar.ContentDigest());

            File.Create(Path.Combine(bar, "subdir", "file")).Close();
            CollectionAssert.AreEqual(tfoo.ContentDigest(), tbar.ContentDigest());

            File.WriteAllText(Path.Combine(foo, "subdir", "file"), "contents");
            CollectionAssert.AreNotEqual(tfoo.ContentDigest(), tbar.ContentDigest());

            File.WriteAllText(Path.Combine(bar, "subdir", "file"), "contents");
            CollectionAssert.AreEqual(tfoo.ContentDigest(), tbar.ContentDigest());
        }


    }
}