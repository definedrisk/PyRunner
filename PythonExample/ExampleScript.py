import argparse
import time
import sys
import os
import uuid

if not sys.warnoptions:
    import warnings
    warnings.simplefilter("ignore")

parser = argparse.ArgumentParser(description="The Parser's Description")
parser.add_argument('Title')
parser.add_argument('Delay')
parser.add_argument('FileOutputDir')
parser.add_argument('Arg')

class CommandLineArgs:
    pass

args = CommandLineArgs()

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
