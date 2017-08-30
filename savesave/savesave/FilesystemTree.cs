using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace savesave
{
    public class FilesystemTree
    {

        string root;

        static string date_format = "yyyy-MM-dd-HH-mm-ss";

        public FilesystemTree(string root)
        {
            this.root = Path.GetFullPath(root);
        }

        protected string RelativePath(string path)
        {
            List<string> parts = new List<string>();
            while (true) {
                if ((new FileInfo(path)).FullName == (new FileInfo(this.root)).FullName) {
                    return Path.Combine(parts.ToArray());
                }
                parts.Insert(0, Path.GetFileName(path));
                string prev = path;
                path = Path.GetDirectoryName(path);
                if (prev == path) {
                    throw new ArgumentException("Path outside root");
                }
            }
        }

        protected string AbsolutePath(string path)
        {
            return Path.Combine(this.root, path);
        }

        public IEnumerable<string> EnumerateEntries(string path = "")
        {
            string abs = this.AbsolutePath(path);
            yield return this.RelativePath(abs);
            if (File.Exists(abs)) {
                yield break;
            } else if (Directory.Exists(abs)) {
                foreach (string entry in Directory.EnumerateFileSystemEntries(abs)) {
                    foreach (string subentry in this.EnumerateEntries(this.RelativePath(entry))) {
                        yield return subentry;
                    }
                }
            } else {
                throw new ArgumentException();
            }
        }

        public DateTime LastWriteTimeUtc()
        {
            DateTime m = DateTime.MinValue;
            foreach (string entry in this.EnumerateEntries()) {
                DateTime d = File.GetLastWriteTimeUtc(this.AbsolutePath(entry));
                if (d > m) {
                    m = d;
                }
            }
            return m;
        }

        protected static byte[] HashFile(string path)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create()) {
                using (var stream = File.OpenRead(path)) {
                    return sha1.ComputeHash(stream);
                }
            }
        }

        public byte[] ContentDigest()
        {
            List<string> entries = new List<string>(this.EnumerateEntries());
            entries.Sort(); // sort enties to get a deterministic result
            using (var sha = System.Security.Cryptography.SHA1.Create()) {
                foreach (string entry in entries) {
                    byte[] name_digest;
                    byte[] content_digest;
                    using (var name_sha = System.Security.Cryptography.SHA1.Create()) {
                        name_digest = name_sha.ComputeHash(Encoding.Unicode.GetBytes(entry));
                    }
                    if (File.Exists(this.AbsolutePath(entry))) {
                        content_digest = HashFile(this.AbsolutePath(entry));
                    } else if (Directory.Exists(this.AbsolutePath(entry))) {
                        content_digest = new byte[sha.OutputBlockSize];
                    } else {
                        throw new ArgumentException();
                    }

                    sha.TransformBlock(name_digest, 0, name_digest.Length, null, 0);
                    sha.TransformBlock(content_digest, 0, content_digest.Length, null, 0);
                }
                sha.TransformFinalBlock(new byte[] { }, 0, 0);
                return sha.Hash;
            }
        }

        public string Snapshot(string destination)
        {
            string temppath = destination + ".tmp";
            using (FileStream fs = new FileStream(temppath, FileMode.Open)) {
                using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create)) {
                    foreach (string entry in this.EnumerateEntries()) {
                        archive.CreateEntryFromFile(this.AbsolutePath(entry), entry);
                    }
                }
            }
            return "";
        }

        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public string DebugString()
        {
            StringWriter s = new StringWriter();
            string m = this.LastWriteTimeUtc().ToString(date_format);
            string d = ByteArrayToString(this.ContentDigest());
            s.WriteLine("{0} {1} {2}", m, d, this.root);
            foreach (string entry in this.EnumerateEntries()) {
                m = File.GetLastWriteTimeUtc(this.AbsolutePath(entry)).ToString(date_format);
                if (File.Exists(this.AbsolutePath(entry))) {
                    d = ByteArrayToString(HashFile(this.AbsolutePath(entry)));
                } else {
                    d = ByteArrayToString(new byte[20]);
                }
                s.WriteLine("{0} {1} {2}", m, d, entry);
            }
            return s.ToString();
        }
    }
}
