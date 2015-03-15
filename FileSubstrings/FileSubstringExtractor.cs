using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSubstrings
{
    /// <summary>
    /// This class will extract all substrings of all files in a directory and all its subdirectories. A substring is a sequence
    /// of alphanumeric characters that is not surrounded by other alphanumeric characters. Only the base name of the files is considered
    /// and not the extension.
    /// </summary>
    public class FileSubstringExtractor
    {
        /// <summary>
        /// The options which define how the substrings are calculated.
        /// </summary>
        private ExtractorOptions _options;

        /// <summary>
        /// All the found substrings. Null means that an error occured
        /// or the search has not been started.
        /// </summary>
        private IEnumerable<SubString> _result;

        /// <summary>
        /// The number of files looked at during search.
        /// </summary>
        private long _fileCount;

        /// <summary>
        /// The number of files looked at during search.
        /// </summary>
        public long FileCount
        { 
            get { 
                CheckResultAvailability(); 
                return _fileCount; 
            } 
        }

        /// <summary>
        /// The given path could not be found.
        /// </summary>
        public class PathNotFoundException : Exception
        {
            /// <summary>
            /// The path that could not be found.
            /// </summary>
            public string Path { get; set; }
        }

        /// <summary>
        /// Simple structure to hold the options for the substring extraction.
        /// </summary>
        public struct ExtractorOptions
        {
            /// <summary>
            /// The path of the directory to search in.
            /// </summary>
            public string path;

            /// <summary>
            /// The minimum number of characters of substrings considered.
            /// </summary>
            public int minSubstringLength;

            /// <summary>
            /// The minimum number of occurences of a substring to be included in the result.
            /// </summary>
            public int minOccurenceCount;

            /// <summary>
            /// The maximum number of substrings included in the result. THe subtrings with the
            /// most occurrences are taken.
            /// </summary>
            public int maxSubstrings;
        }

        /// <summary>
        /// A substring of a filename. A substring is a non-empty sequence of alphanumeric characters
        /// of the base name of a file. The extension is not considered.
        /// </summary>
        public struct SubString
        {
            /// <summary>
            /// The substring itself.
            /// </summary>
            public string Name;

            /// <summary>
            /// The number of files that have this subtring in their base name.
            /// </summary>
            public int OccurrenceCount;

            /// <summary>
            /// The files that have this substring in their base name.
            /// </summary>
            public List<FileInfo> Matches;
        }

        /// <summary>
        /// Constructs this extractor with the given options.
        /// </summary>
        /// <param name="options">The options used for the extraction.</param>
        public FileSubstringExtractor(ExtractorOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Returns the files that have a given substring in their name.
        /// </summary>
        /// <param name="subString">The substring to search for.</param>
        /// <returns>The files for the substring or null if there is no such substring.</returns>
        public IEnumerable<FileInfo> getFiles(string subString)
        {
            CheckResultAvailability();
            var fileList = _result
                .Where(x => x.Name == subString)
                .Select(x => x.Matches)
                .SingleOrDefault();
            return fileList != null ? fileList.Take(_options.maxSubstrings) : null;
        }

        /// <summary>
        /// Returns the result of the extraction.
        /// </summary>
        /// <returns>All subtrings found with the set options.</returns>
        public IEnumerable<SubString> getSubstrings()
        {
            CheckResultAvailability();
            return _result;
        }

        /// <summary>
        /// Starts the extraction if there is no result yet.
        /// </summary>
        private void CheckResultAvailability()
        {
            if (_result == null)
            {
                CollectSubStrings();
            }
        }

        /// <summary>
        /// Resets the result. The next call to any other public method will start the extraction again.
        /// </summary>
        public void Reset()
        {
            _fileCount = 0;
            _result = null;
        }

        /// <summary>
        /// The actual extraction method. Will iterate over all files in the directory and all subdirectories
        /// and extract all substrings of those files. The substrings are put into a map which is then
        /// filtered and converted into the result.
        /// </summary>
        private void CollectSubStrings()
        {
            Reset();

            DirectoryInfo dir = new DirectoryInfo(_options.path);
            Dictionary<string, List<FileInfo>> mapping = new Dictionary<string, List<FileInfo>>();
            Regex re = new Regex("[a-zA-Z0-9]+", RegexOptions.IgnoreCase);

            // check if directory exists
            if (!dir.Exists)
            {
                throw new PathNotFoundException { Path = _options.path };
            }

            // iterate over all files
            foreach (FileInfo file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                ++_fileCount;

                // get all substrings
                MatchCollection matches = re.Matches(Path.GetFileNameWithoutExtension(file.Name));
                // TODO make matches unique
                foreach (Match match in matches)
                {
                    // enter substrings into mapping
                    string matchedString = match.Value;
                    if (matchedString.Length >= _options.minSubstringLength)
                    {
                        if (!mapping.ContainsKey(matchedString))
                        {
                            mapping.Add(matchedString, new List<FileInfo>());
                        }
                        mapping[matchedString].Add(file);
                    }
                }
            }

            _result = mapping
                .Where(l => l.Value.Count >= _options.minOccurenceCount) 
                .OrderByDescending(x => x.Value.Count)
                .Take(_options.maxSubstrings)
                .Select(x => new SubString { 
                    Name = x.Key, 
                    Matches = x.Value, 
                    OccurrenceCount = x.Value.Count })
                .ToList();
        }

    }
}
