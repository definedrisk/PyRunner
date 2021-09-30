import argparse
import time
import sys

if not sys.warnoptions:
    import warnings
    warnings.simplefilter("ignore")

parser = argparse.ArgumentParser(description="The Parser's Description")
parser.add_argument('Title')
parser.add_argument('Delay')
parser.add_argument('ExampleArg2')
parser.add_argument('ExampleArg3')

class CommandLineArgs:
    pass

args = CommandLineArgs()

args = parser.parse_args(namespace=CommandLineArgs)

print("Title: ", args.Title)
print("Arg1 (delay time in seconds):\n" + args.Delay)
print("Arg2:\n" + args.ExampleArg2)
print("Arg3:\n" + args.ExampleArg3)

print("Python script delay ", args.Delay, " seconds is starting delay.")
sys.stdout.flush()

time.sleep(float(args.Delay))

print("Python script delay ", args.Delay, " seconds is finished.")
