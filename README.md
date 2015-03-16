# FileSubstrings

## Description

This small command line tool collects all substrings of the filenames (without extension) of all files in a directory and all subdirectories. 
A substring is a sequence of alphanumeric characters that have no other alphanumeric characters to the side of the substring.

For example the filename `image_2013_12_03.jpg` has the substrings `image`, `2013`, `12` and `03`.

The tool can either list all found subtrings with their number of occurrences or list the files that have a given substrings in their file name.

## Usage

FileSubstrings.exe [OPTIONS]

The options are:

-s <substring> : If this option is specified then, instead of the substrings with their occurrence count, 
the file names of the files that have the given substring are printed out.  
-sl <length> : Specifies the minimum length of a substring to be considered. The default value is 3.  
-oc <count> : Specifies a minimum number of occurrences of a substring to be considered. The default value is 2.  
-c <count> : Specifies the maximum number of results shown. The default is `MAX_INT`.  
-h, --help : Prints the help.  
-f : If this flag is specified then instead of the file names the full path of the files are printed out.
This has only an effect if a substring is specified using `-s`.  
<dir> : The directory to look at.
