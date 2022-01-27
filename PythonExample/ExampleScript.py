# An example python script that can be run manually to confirm operation of Python Environment
# and use of debuuger in Visual Studio

# This script is called using the PythonRunner in the ExampleUsage project

import argparse
import time
import sys
import os
import uuid

# disable warnings usually written to sys.stderr
# https://docs.python.org/3/library/warnings.html
if not sys.warnoptions:
    import warnings
    warnings.simplefilter("ignore")

# # turn on text trap
# import io
# text_trap = io.StringIO()
# sys.stdout = text_trap
#
# # turn off text trap
# sys.stdout = sys.__stdout__

# parse command line arguments
parser = argparse.ArgumentParser(description="The Parser's Description")
parser.add_argument('Title')
parser.add_argument('Delay')
parser.add_argument('FileOutputDir')
parser.add_argument('Arg')

class CommandLineArgs:
    pass

args = CommandLineArgs()

# See the following Micorosft Windows notes when passing command line arguymnets using escaping for quotes and spaces
# https://docs.microsoft.com/en-gb/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way
# https://docs.microsoft.com/en-us/previous-versions//17w5ykft(v=vs.85)?redirectedfrom=MSDN

args = parser.parse_args(namespace=CommandLineArgs)

print("Title:", args.Title)
print("Delay:", args.Delay)
print("FileOutputDir:", args.FileOutputDir)
print("Arg:", args.Arg)

print("Python script is starting delay of ", args.Delay, " seconds.")
sys.stdout.flush()

time.sleep(float(args.Delay))
print("Python script continue after delay.")
print("Current working directory is:", os.getcwd())
print("Writing file (and dir) at ", args.FileOutputDir)

if not os.path.isdir(args.FileOutputDir):
    os.makedirs(args.FileOutputDir)
    print("Directory created.")
else:
    print("Directory exists.")

os.chdir(str(args.FileOutputDir))

file = open("example.txt", "w")
file.write(str(uuid.uuid4()))
file.close()
print("File written.")

print("Python script finished.")
