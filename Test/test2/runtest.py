#!/usr/bin/python

# 2013 Pantazis Deligiannis
#
# runs the test cases of this directory

import sys;
import os;
import getopt;

MONO = "/usr/bin/mono "
BOOGIE = "/Users/pantazis/workspace/boogie/Binaries/Boogie.exe "

PROVER_OPTIONS_Z3 = "/nologo "
PROVER_OPTIONS_CVC4 = "/proverOpt:SOLVER=cvc4 /nologo "

# choose a parser
opts, args = getopt.getopt(sys.argv[1:], "s:", ["solver="])
solver = "Z3"
for o, a in opts:
  if o in ("-s", "--solver"):
      if a == "cvc4":
          solver = a

# if the Output file exists remove it
if os.path.exists("Output"):
  os.remove("Output")

# runs the given test
def runtest(filename):
  if solver == "cvc4":
    if os.name == "nt":
      command = BOOGIE + PROVER_OPTIONS_CVC4 + filename
    else:
	  command = MONO + BOOGIE + PROVER_OPTIONS_CVC4 + filename
  else:
    if os.name == "nt":
      command = BOOGIE + PROVER_OPTIONS_Z3 + filename
    else:
	  command = MONO + BOOGIE + PROVER_OPTIONS_Z3 + filename
  
  print "===== Testing: " + filename
  
  if os.path.exists("Output"):
    os.system(command + " >> Output")
  else:
    os.system(command + " > Output")

files = ["FormulaTerm.bpl", "FormulaTerm2.bpl", "Passification.bpl", "B.bpl",
  "Ensures.bpl", "Old.bpl", "OldIllegal.bpl", "Arrays.bpl", "Axioms.bpl",
  "Quantifiers.bpl", "Call.bpl", "AssumeEnsures.bpl",
  "CutBackEdge.bpl", "False.bpl", "LoopInvAssume.bpl",
  "strings-no-where.bpl", "strings-where.bpl",
  "Structured.bpl", "Where.bpl", "UpdateExpr.bpl",
  "NeverPattern.bpl", "NullaryMaps.bpl", "Implies.bpl",
  "IfThenElse1.bpl", "Lambda.bpl", "LambdaPoly.bpl", "LambdaOldExpressions.bpl",
  "SelectiveChecking.bpl", "FreeCall.bpl"]

for file in files:
  os.system("echo >> Output")
  os.system("echo -------------------- " + file + " -------------------- >> Output")
  runtest("/noinfer " + file)
  
files = ["Arrays.bpl", "Lambda.bpl", "TypeEncodingM.bpl"]

for file in files:
  os.system("echo >> Output")
  os.system("echo -------------------- " + file + " /typeEncoding:m -------------------- >> Output")
  runtest("/noinfer /typeEncoding:m " + file)
  
os.system("echo >> Output")
os.system("echo -------------------- sk_hack.bpl -------------------- >> Output")
runtest("/noinfer sk_hack.bpl")

os.system("echo >> Output")
os.system("echo -------------------- ContractEvaluationOrder.bpl -------------------- >> Output")
runtest("ContractEvaluationOrder.bpl")

os.system("echo >> Output")
os.system("echo -------------------- Timeouts0.bpl -------------------- >> Output")
runtest("/timeLimit:4 Timeouts0.bpl")

# compare the output with the expected answers
test_file = open("Output").readlines()
correct_file = open("Answer").readlines()

for test, correct in zip(test_file, correct_file):
    if test.strip(' \t\n\r') != correct.strip(' \t\n\r'):
        print "\n===== " + os.getcwd().split(os.sep)[-1] + ": FAILED"
        break
else:
    print "\n===== " + os.getcwd().split(os.sep)[-1] + ": PASSED"
