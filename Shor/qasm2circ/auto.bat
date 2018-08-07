if exist computing.log del computing.log
if exist circuit.pdf del circuit.pdf
if exist circuit.qasm del circuit.qasm
dotnet run < n.txt >computing.log
if exist circuit.qasm bash auto.sh
if exist circuit.tex del circuit.tex
if exist circuit.aux del circuit.aux
if exist circuit.dvi del circuit.dvi
if exist circuit.eps del circuit.eps
if exist circuit.idx del circuit.idx
if exist circuit.log del circuit.log

