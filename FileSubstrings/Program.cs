using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSubstrings
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            int minSubstringLength = 3;
            int minOccurenceCount = 2;
            int maxSubstrings = Int32.MaxValue;
            string substring = null;

            bool lengthSwitch = false;
            bool occurenceSwitch = false;
            bool limitSwitch = false;
            bool substringSwitch = false;
            foreach (string arg in args)
            {
                if (arg == "-sl")
                {
                    lengthSwitch = true;
                    continue;
                } 
                
                if (arg == "-oc")
                {
                    occurenceSwitch = true;
                    continue;
                } 
                
                if (arg == "-c")
                {
                    limitSwitch = true;
                    continue;
                }

                if (arg == "-s")
                {
                    substringSwitch = true;
                    continue;
                }

                if (arg == "-h" || arg.Contains("help"))
                {
                    Console.WriteLine("Usage: FileSubstrings.exe [OPTIONS]");
                    Console.WriteLine("Options:");
                    Console.WriteLine("-sl <length> : the minimum substring length (default: 3)");
                    Console.WriteLine("-oc <count> : the minimum number of occurences of a substring (default: 2)");
                    Console.WriteLine("-c <count> : the maximum number of shown substrings");
                    Console.WriteLine("-h, -help : prints this help");
                    Console.WriteLine("-s <substring> : shows the files for this substring");
                    Console.WriteLine("<dir> : the directory to search in");
                    return;
                }

                if (lengthSwitch)
                {
                    lengthSwitch = false;
                    int sl;
                    if (int.TryParse(arg, out sl))
                    {
                        minSubstringLength = sl;
                    }
                    continue;
                }

                if (substringSwitch)
                {
                    lengthSwitch = false;
                    substring = arg;
                    continue;
                }
                
                if (occurenceSwitch)
                {
                    occurenceSwitch = false;
                    int oc;
                    if (int.TryParse(arg, out oc))
                    {
                        minOccurenceCount = oc;
                    }
                    continue;
                }
                
                if (limitSwitch)
                {
                    limitSwitch = false;
                    int c;
                    if (int.TryParse(arg, out c))
                    {
                        maxSubstrings = c;
                    }
                    continue;
                }

                path = arg;
            }
            bool hasSubstring = substring != null;

            Console.WriteLine("Directory: "+path);

            string[] files = null;
            try
            {
                files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();
            Regex re = new Regex("[a-zA-Z0-9]+",RegexOptions.IgnoreCase);
            int fileCount = 0;
            foreach (string file in files)
            {
                ++fileCount;
                MatchCollection matches = re.Matches(Path.GetFileNameWithoutExtension(file));
                foreach(Match match in matches) 
                {
                    string matchedString = match.Value;
                    if (matchedString.Length >= minSubstringLength || hasSubstring) 
                    { 
                        if (!mapping.ContainsKey(matchedString))
                        {
                            mapping.Add(matchedString, new List<string>());
                        }
                        mapping[matchedString].Add(file);
                    }
                }
            }

            Console.WriteLine("Found " + fileCount + " files");
            Console.WriteLine();

            if (hasSubstring)
            {
                if (mapping.ContainsKey(substring) && mapping[substring].Count>0)
                {
                    foreach (string file in mapping[substring].Take(maxSubstrings))
                    {
                    Console.WriteLine(file);
                    }
                }
                else
                {
                    Console.WriteLine("Found no files containing \""+substring+"\"");
                }
            }
            else
            {
                foreach (var substr in mapping.Where(l => l.Value.Count >= minOccurenceCount).OrderByDescending(x => x.Value.Count).Take(maxSubstrings))
                {
                    Console.WriteLine(substr.Key + " : " + substr.Value.Count);
                }
            }
        }
    }
}
