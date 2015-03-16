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
    /// <summary>
    /// The console program that collects all substrings of all files in a directory and its subdirectories and prints those 
    /// substring to stdout. Alternatively all file names of files that have a specific substring are printed out.
    /// </summary>
    class Program
    {
        /// <summary>
        /// This class resembles a command line switch. If the command is found in the command line parameters
        /// then a specified function is called. A switch may have an argument which is passed as the next
        /// command line parameter. 
        /// </summary>
        class CommandSwitch
        {
            public CommandSwitch( string command, bool hasParameter, Action<string> switchFunction)
            {
                Command = command;
                HasParameter = hasParameter;
                SwitchFunction = switchFunction;
            }

            /// <summary>
            /// The command line switch.
            /// </summary>
            public string Command;

            /// <summary>
            /// If HasParameter is true the next command line parameter is passed to the 
            /// SwitchFunction.
            /// </summary>
            public bool HasParameter;

            /// <summary>
            /// The function that is called when the command is found in the command line arguments.
            /// If HasParameter is true then the parameter to this function is the next command line argument.
            /// Otherwise the switch itself is passed to this function.
            /// </summary>
            public Action<string> SwitchFunction;
        }

        /// <summary>
        /// The list of all command line switches available
        /// </summary>
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

        /// <summary>
        /// The path to the directory that is looked into
        /// </summary>
        static string _path = Directory.GetCurrentDirectory();

        /// <summary>
        /// The minimum length (character count) of a substring
        /// </summary>
        static int _minSubstringLength = 3;

        /// <summary>
        /// The minimum number of occurrences of a substring necessary
        /// in order to be considered
        /// </summary>
        static int _minOccurenceCount = 2;

        /// <summary>
        /// The maximum number of results displayed.
        /// </summary>
        static int _maxSubstrings = Int32.MaxValue;

        /// <summary>
        /// The specific substring whos file are to be printed out.
        /// Is null if subtrings are printed out.
        /// </summary>
        static string _substring = null;

        /// <summary>
        /// When printing out file names: Print out full path or just file name?
        /// </summary>
        static bool _fullPath = false;

        /// <summary>
        /// Show help?
        /// </summary>
        static bool _showHelp = false;

        /// <summary>
        /// Has a specific substring been specified?
        /// </summary>
        static bool _hasSubstring = false;

        /// <summary>
        /// Parses the command line arguments using the command switches.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        static void ParseCommands(string[] args)
        {
            // if non-null this is the last command. It expects an argument
            CommandSwitch currentSwitch = null;

            // iterate over all arguments
            foreach (var arg in args)
            {
                if(currentSwitch == null)
                {
                    // read next switch

                    bool hasSwitch = false;
                    foreach (var sw in _switches)
                    {
                        if(sw.Command == arg)
                        {
                            // command found
                            if(sw.HasParameter)
                            {
                                // command has argument, execute switching function with next parameter
                                currentSwitch = sw;
                            }
                            else
                            {
                                // no argument, execute switching function directly.
                                sw.SwitchFunction(arg);
                            }

                            hasSwitch = true;
                        }
                    }

                    // if no switch was found then this is the directory
                    if(!hasSwitch)
                    {
                        _path = arg;
                    }
                }
                else
                {
                    // read argument to last switch and execute switching function.
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

        /// <summary>
        /// This is the main method that is executed when this program is started.
        /// </summary>
        /// <param name="args">The command line parameters</param>
        /// <returns>The exit code. Returns 1 on errors, 0 otherwise.</returns>
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

            // initialize extractor
            FileSubstringExtractor extractor = new FileSubstringExtractor(new FileSubstringExtractor.ExtractorOptions {
                maxSubstrings = _maxSubstrings,
                minOccurenceCount = _minOccurenceCount,
                minSubstringLength = _minSubstringLength,
                path = _path
            });

            // do the extraction and printing of the result
            try
            {
                if (_hasSubstring)
                {
                    // output files for a given substring
                    var files = extractor.getFiles(_substring);
                    if (files.Any())
                    {
                        try
                        {
                            // write filenames.
                            files.ToList().ForEach(x =>
                                Console.WriteLine(_fullPath ? x.FullName : x.Name)
                                );
                        } catch( Exception)
                        {
                            // if a single file reports an error then just ignore the file
                        }
                    }
                    else
                    {
                        Console.WriteLine("Found no files containing \"" + _substring + "\"");
                    }
                }
                else
                {
                    // write out substrings
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
