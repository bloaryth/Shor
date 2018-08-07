### How to run - Under Windows

First make sure you can run C# and Q# program on your computer(.NET).

Move <Driver.cs>, <Operation.qs>, <Shor.csproj> into "qasm2circ".

Put your input into <n.txt>.

Output of computing process is in <computing.log>, with the answer in the last line.

Output of quantum circuit(if there is) with qasm format is in <circuit.qasm>.

For our program of an optimization, we may not have a quantum circuit.

#### REQUIREMENTS

##### Windows

- WSL(Windows Subsystem for Linux)

##### WSL

- csh(tcsh)
- latex2e with xypic (included in tetex)
- python version 2.3 or greater
- ghostscript (and epstopdf)
- netpbm (for creation of png files)

#### RUN

##### Without requirements

Run <auto_wo.vbs>, and <circuit.pdf> cannot be generated.

##### With requirements

Run <auto.vbs>, and if there is a quantum circuit, <circuit.pdf> will be made to show the circuit.

### Designing Details

The code is <Driver.cs>, <Operation.qs>.

The code has three parts -- Q# code to do order finding, some C# code to do continued fraction and other classical part in Shor Algorithm, bash and some C# code to output the quantum circuits.

The Implementation details are in the code with detailed comments.