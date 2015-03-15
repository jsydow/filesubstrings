using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSubstrings
{
    class Program
    {
        class CommandSwitch
        {
            public CommandSwitch( string command, bool hasParameter, Action<string> switchFunction)
            {
                Command = command;
                HasParameter = hasParameter;
                SwitchFunction = switchFunction;
            }

            public string Command;
            public bool HasParameter;
            public Action<string> SwitchFunction;
        }

        static List<CommandSwitch> _switches = new List<CommandSwitch>()
            {
                new CommandSwitch("-sl", true, SetLength),
                new CommandSwitch("-oc", true, SetOccurrences),
                new CommandSwitch("-c", true, SetOutputCount),
                new CommandSwitch("-s", true, SetSubString),
                new CommandSwitch("-f", false, x => _fullPath = true),
                new CommandSwitch("-h", false, x => _showHelp = true),
                new CommandSwitch("--help", false, x =>  _showHelp = true),
            };

        static string _path = Directory.GetCurrentDirectory();
        static int _minSubstringLength = 3;
        static int _minOccurenceCount = 2;
        static int _maxSubstrings = Int32.MaxValue;
        static string _substring = null;
        static bool _fullPath = false;
        static bool _showHelp = false;
        static bool _hasSubstring = false;

        static void ParseCommands(string[] args)
        {

            CommandSwitch currentSwitch = null;
            foreach (var arg in args)
            {
                if(currentSwitch == null)
                {
                    bool hasSwitch = false;
                    foreach (var sw in _switches)
                    {
                        if(sw.Command == arg)
                        {
                            if(sw.HasParameter)
                            {
                                currentSwitch = sw;
                            }
                            else
                            {
                                sw.SwitchFunction(arg);
                            }
                            hasSwitch = true;
                        }
                    }
                    if(!hasSwitch)
                    {
                        _path = arg;
                    }
                }
                else
                {
                    currentSwitch.SwitchFunction(arg);
                    currentSwitch = null;
                }
            }

            _hasSubstring = _substring != null;
        }

        private static void SetSubString(string arg)
        {
            _substring = arg;
        }

        private static void SetOutputCount(string arg)
        {
            int c;
            if (int.TryParse(arg, out c))
            {
                _maxSubstrings = c;
            }
        }

        private static void SetOccurrences(string arg)
        {
            int oc;
            if (int.TryParse(arg, out oc))
            {
                _minOccurenceCount = oc;
            }
        }

        private static void SetLength(string arg)
        {
            int sl;
            if (int.TryParse(arg, out sl))
            {
                _minSubstringLength = sl;
            }
        }

        static int Main(string[] args)
        {
            ParseCommands(args);

            if(_showHelp)
            {
                Console.WriteLine("Usage: FileSubstrings.exe [OPTIONS]");
                Console.WriteLine("Options:");
                Console.WriteLine("-sl <length> : the minimum substring length (default: 3)");
                Console.WriteLine("-oc <count> : the minimum number of occurences of a substring (default: 2)");
                Console.WriteLine("-c <count> : the maximum number of shown entries of the result");
                Console.WriteLine("-h, --help : prints this help");
                Console.WriteLine("-s <substring> : shows the files for this substring");
                Console.WriteLine("-f : output full path of files");
                Console.WriteLine("<dir> : the directory to search in");

                return 0;
            }

            FileSubstringExtractor extractor = new FileSubstringExtractor(new FileSubstringExtractor.ExtractorOptions {
                maxSubstrings = _maxSubstrings,
                minOccurenceCount = _minOccurenceCount,
                minSubstringLength = _minSubstringLength,
                path = _path
            });

            try
            {
                if (_hasSubstring)
                {
                    var files = extractor.getFiles(_substring);
                    if (files.Any())
                    {
                        try
                        {
                            files.ToList().ForEach(x =>
                                Console.WriteLine(_fullPath ? x.FullName : x.Name)
                                );
                        } catch( Exception)
                        {
                            // just ignore the file
                        }
                    }
                    else
                    {
                        Console.WriteLine("Found no files containing \"" + _substring + "\"");
                    }
                }
                else
                {
                    foreach (var substr in extractor.getSubstrings())
                    {
                        Console.WriteLine(substr.Name + " : " + substr.OccurrenceCount);
                    }
                }
            }
            catch (SecurityException)
            {
                Console.WriteLine("Could not access directory "+_path);
                return 1;
            }
            catch (PathTooLongException)
            {
                Console.WriteLine("Directory path is too long " + _path);
                return 1;
            }
            catch(FileSubstrings.FileSubstringExtractor.PathNotFoundException)
            {
                Console.WriteLine("Could not find directory " + _path);
                return 1;
            }

            return 0;
        }
    }
}
