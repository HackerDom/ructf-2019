using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using indexReact.db.Models;
using indexReact.db.Services;
using Microsoft.AspNetCore.Http;

namespace indexReact.Helpers
{
    public interface IIndexHelper
    {
        void AddToIndex(string user, IFormFile zip);
    }

    public class IndexHelper : IIndexHelper
    {
        private const long MaxSize = 1024 * 1024 * 4;
        private const string IndexRoot = "index";
        private readonly IServiceBase<Node> nodesDb;
        private readonly IServiceBase<IndexEntity> indexDb;
        private readonly string cwd;

        public IndexHelper(IServiceBase<Node> nodesDb, IServiceBase<IndexEntity> indexDb)
        {
            cwd = Directory.GetCurrentDirectory();
            this.nodesDb = nodesDb;
            this.indexDb = indexDb;
            Init();
        }

        private void Init()
        {
            var nodes = nodesDb.Get();
            if (!nodes.Any() || nodes.Count > 1 || nodes[0].Name != IndexRoot)
            {
                nodesDb.RemoveAll();
                indexDb.RemoveAll();

                nodesDb.Create(new Node(IndexRoot));
            }
        }

        public void AddToIndex(string user, IFormFile zip)
        {
            using (var fileStream = zip.OpenReadStream())
            {
                var root = nodesDb.Get().First();
                var files = Unzip(fileStream);
                foreach (var (fileName, fullName) in files)
                {
                    if (string.IsNullOrEmpty(fileName))
                        continue;

                    var filePath = Path.GetFullPath(
                            Path.Join(
                                Path.Join(IndexRoot, user),
                                Path.GetFileNameWithoutExtension(zip.FileName),
                                fullName))
                        .Replace($"{cwd}{Path.DirectorySeparatorChar}", "");
                    var current = root;
                    AddNodes(current, filePath, fileName);
                    AddIndex(filePath, fileName, user);
                }

                nodesDb.Update(root.Id, root);
            }
        }

        private void AddNodes(Node current, string filePath, string fileName)
        {
            if (!filePath.StartsWith(IndexRoot))
                throw new IndexImportException($"wrong file name {fileName}");

            var nodesToAdd = Split(filePath);
            foreach (var nodeName in nodesToAdd.Skip(1))
            {
                var next = current.Children.FirstOrDefault(n => n.Name == nodeName);
                if (next!=null)
                {
                    current = next;
                    continue;
                }

                var newNode = new Node(nodeName);
                current.Children.Add(newNode);
                current = newNode;
            }
        }

        private void AddIndex(string filePath, string fileName, string user)
        {
            var indexEntity = indexDb.Get(ie => ie.User == user) ?? new IndexEntity(user);
            if(!indexEntity.Hash.ContainsKey(fileName))
                indexEntity.Hash[fileName] = new List<string>();

            indexEntity.Hash[fileName].Add(Path.GetDirectoryName(filePath));

            if (indexEntity.Id != null)
                indexDb.Update(indexEntity.Id, indexEntity);
            else
                indexDb.Create(indexEntity);
        }

        private static List<string> Split(string path)
        {
            var l = new List<string>();
            while (!string.IsNullOrEmpty(path))
            {
                l.Add(Path.GetFileName(path));
                path = Path.GetDirectoryName(path);
            }

            l.Reverse();
            return l;
        }

        private (string, string)[] Unzip(Stream file)
        {
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                if (zip.Entries.Count > 500)
                    throw new IndexImportException("Too much entries in archive");

                var totalLen = zip.Entries.Sum(e => e.Length);
                if (totalLen > MaxSize)
                    throw new IndexImportException("Zip too big");

                return zip.Entries.Select(e => (e.Name, e.FullName)).ToArray();
            }
        }
    }
}