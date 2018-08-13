// Created by Yaoyao Ding, Jiuqin Zhou and Jie Xie.
// Licensed under the MIT License.

using System;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System.IO;

namespace Shor
{
    using myShor;
    using System.Collections;

    class Driver
    {
        static int Gcd(int a, int b)
        {
            return b == 0 ? a : Gcd(b, a % b);
        }

        static void Swap(ref int a, ref int b)
        {
            int tmp = b;
            b = a;
            a = tmp;
        }

        // output the quantum circuits to .qasm file, which can 
        // be converted to png or pdf
        static void w2qasm(string msg)
        {
            using (FileStream fs = new FileStream("circuit.qasm", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(msg);
                }
            }
        }

        // generate the quantum circuits
        static void CircuitMaking(int a, int n)
        {
            string str = "";
            int k = 0;
            int n1 = n;
            while (n1 > 0)
            {
                ++k;
                n1 /= 2;
            }
            int k2 = 2 * k + 1;
            int temp = 1;
            string tempstr = "";
            for (int i = 0; i < k2; ++i)
            {
                if (temp == 1) tempstr = "\'U\'\n";
                else tempstr = "\'U^{" + temp.ToString() + "}\'\n";
                temp *= 2;
                str += "\tdefbox CU" + i.ToString() + "," + (k + 1).ToString() + ",1," + tempstr;
            }
            for (int i = 2; i <= k2; ++i)
            {
                str += "\tdef CR" + i.ToString() + ",1," + "\'R" + i.ToString() + "\'\n";
            }
            for (int i = 0; i < k2; ++i)
            {
                str += "\tqubit i" + i.ToString() + ",0\n";
            }
            for (int i = 0; i < k; ++i)
            {
                str += "\tqubit j" + i.ToString() + ",0\n";
            }
            for (int i = 0; i < k2; ++i)
            {
                str += "\th i" + i.ToString() + "\n";
            }
            str += "\tX j0\n";
            for (int i = 0; i < k2; ++i)
            {
                str += "\tCU" + i.ToString() + " i" + (k2 - i - 1).ToString();
                for (int j = 0; j < k; ++j)
                {
                    str += ",j" + j.ToString();
                }
                str += "\n";
                for (int j = k2 - i; j < k2; ++j)
                {
                    str += "\tnop i" + j.ToString() + "\n";
                }
            }
            for (int i = 0; i < k2; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    int delta = i - j + 1;
                    str += "\tCR" + delta.ToString() + " i" + j.ToString() + ",i" + i.ToString() + "\n";
                }
                str += "\th i" + i.ToString() + "\n";
                for (int j = 0; j < i; ++j)
                {
                    for (int l = 0; l < i - j; ++l)
                        str += "\tnop i" + j.ToString() + "\n";
                }
            }
            for (int i = 0; i < k2; ++i)
            {
                str += "\tmeasure i" + i.ToString() + "\n";
            }
            w2qasm(str);
        }

        static (int, int) GetFraction(ArrayList list, int ind)
        {
            int ns = 1;
            int r = (int)list[--ind];
            while (ind > 0)
            {
                ns = ns + (int)list[--ind] * r;
                Swap(ref ns, ref r);
            }
            Swap(ref ns, ref r);
            return (ns, r);
        }

        static int ContinuedFraction(double s, int a, int n)
        {
            const double eps = 1e-9;
            double ss = s;
            ArrayList list = new ArrayList();
            while (true)
            {
                list.Add((int)s);
                s = s - (int)(s);
                if (s < eps)
                {
                    break;
                }
                s = 1 / s;
            }
            for (int i = 1; i <= list.Count; ++i)
            {
                (int ns, int r) = GetFraction(list, i);
                if (Mpow(a, r, n) == 1)
                {
                    Console.WriteLine($"\t= {ns}/{r}");
                    return r;
                }
            }
            return -1; // not found
        }

        static int OrderFinding(int a, int n)
        {
            // quantum order finding
            double s;
            CircuitMaking(a, n);
            using (var sim = new QuantumSimulator())
            {
                long f = 0;
                long t = 0;
                while (f == 0)
                {
                    (f, t) = OrderFindingPhase.Run(sim, a, n).Result;
                    if (f == 0)
                    {
                        Console.WriteLine($"Got s = 0. Try Again ...");
                    }
                }
                s = ((double)f) / (1L << (int)t);
                Console.WriteLine($"{f}/(2^{t})={s}");
            }
            return ContinuedFraction(s, a, n);

            // classic order finding
            //int r = 1;
            //int s = a % n;
            //while (s != 1)
            //{
            //    s = (int)((long)s * a % n);
            //    r++;
            //}
            //return r;
        }

        // fast mod power : Exp(a, b) % n
        static int Mpow(long a, int b, int n)
        {
            long s;
            for (s = 1; b != 0; b >>= 1, a = a * a % n)
            {
                if ((b & 1) != 0) s = s * a % n;
            }
            return (int)s;
        }

        static void output(int a, int b)
        {
            Console.WriteLine($"Final answer is {a * b} = {a} * {b}");
            //Console.WriteLine($"Press any key to continue ...");
            Environment.Exit(0);
        }

        static void printInt(string s, int a)
        {
            Console.WriteLine(s + " " + $"{a}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Please input the integer:");
            string input = Console.ReadLine();
            int n = Convert.ToInt32(input);
            Random randomObject = new Random();
            Console.WriteLine("");
            while (true)
            {
                Console.WriteLine("A new round started.");
                int a = randomObject.Next(2, n - 1);
                printInt("Let x =", a);
                int d = Gcd(a, n);
                if (d != 1)
                {
                    output(d, n / d);
                }
                int r = OrderFinding(a, n);
                if (r == -1)
                {
                    Console.WriteLine($"No r satifies x^r=1 mod N.\n");
                    continue;
                }
                if (r % 2 == 1)
                {
                    Console.WriteLine($"r = {r}, but r is odd.\n");
                    continue;
                }
                Console.WriteLine($"r = {r}, x^(r/2) = {Mpow(a, r / 2, n)}.");
                if (Mpow(a, r / 2, n) == n - 1)
                {
                    Console.WriteLine($"x^(r/2) mod N is N-1.\n");
                    continue;
                }

                int f = Math.Max(Gcd(Mpow(a, r / 2, n) + 1, n), Gcd(Mpow(a, r / 2, n) - 1, n));
                printInt("One factor f =", f);
                output(f, n / f);
            }
        }

    }

}