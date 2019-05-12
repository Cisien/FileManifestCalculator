using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileManifestCalculator
{
    public class ManifestCalculator
    {
        /// <summary>
        /// Calculates the SHA256 hash of all files in the directory provided
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>Returns a file/hash pair for each file found.</returns>
        public async IAsyncEnumerable<KeyValuePair<FileInfo, byte[]>> GenerateManifestEntries(DirectoryInfo directory)
        {
            var files = directory.GetFiles("*", new EnumerationOptions { RecurseSubdirectories = true });
            foreach (var file in files)
            {
                yield return await CalculateFileHash(file);
            }
        }

        /// <summary>
        /// Calculates the SHA256 hash of the supplied file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Returns a file/hash pair for the file</returns>
        public async Task<KeyValuePair<FileInfo, byte[]>> CalculateFileHash(FileInfo file)
        {
            using var fileStream = file.OpenRead();

            var contents = new byte[fileStream.Length];
            await fileStream.ReadAsync(contents);

            var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(contents, 0, contents.Length);
  
            return new KeyValuePair<FileInfo, byte[]>(file, hash);
        }

        /// <summary>
        /// Builds a CSV containing the file/hash pair for every file found in the path
        /// </summary>
        /// <param name="path">The path to the directory to recursivly search for files to include in the manifest</param>
        /// <returns>A manifest file</returns>
        public async Task<string> BuildManifest(string path)
        {
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"{path} does not exist.");
            }

            var manifest = new StringBuilder();

            var baseDirectory = $"{directory.FullName}{Path.DirectorySeparatorChar}";
            await foreach (var entry in GenerateManifestEntries(directory))
            {
                var relativeFile = entry.Key.FullName.Replace(baseDirectory, string.Empty);
                var hashValue = Convert.ToBase64String(entry.Value);
                manifest.AppendLine($"{relativeFile},{hashValue}");
            }

            return manifest.ToString();
        }

        /// <summary>
        /// Compares the hash of the current file to the supplied hash
        /// </summary>
        /// <param name="target">The current file</param>
        /// <param name="source">The known hash to compare with</param>
        /// <returns>Returns a bool indicating if there was a match.</returns>
        public async Task<bool> CompareHashes(FileInfo target, byte[] source)
        {
            var file = await CalculateFileHash(target);

            return file.Value == source;
        }
    }
}
