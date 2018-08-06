using System;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

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

        static int ContinuedFraction(double s)
        {
            const double eps = 0.1;
            double ss = s;
            Stack st = new Stack();
            while (true)
            {
                st.Push(((int)(s + eps)));
                s = s - (int)(s + eps);
                if (s < eps)
                {
                    break;
                }
                s = 1 / s;
            }
            int ns = 1;
            int r = (int)st.Pop();
            while (st.Count != 0)
            {
                ns = ns + (int)st.Pop() * r;
                Swap(ref ns, ref r);
            }
            Swap(ref ns, ref r);
            Console.WriteLine($"\t= {ns}/{r}");
            return r;
        }

        static int OrderFinding(int a, int n)
        {
            double s;
            using (var sim = new QuantumSimulator())
            {
                long f = 0;
                long t = 0;
                while (f == 0)
                {
                    (f, t) = OrderFindingPhase.Run(sim, a, n).Result;
                    if (f == 0)
                    {
                        Console.WriteLine($"Got s == 0. Try Again ...");
                    }
                }
                s = ((double)f) / (1L << (int)t);
                Console.WriteLine($"{f}/(2^{t})={s}");
            }
            return ContinuedFraction(s);

            //int r = 1;
            //int s = a % n;
            //while (s != 1)
            //{
            //    s = (int)((long)s * a % n);
            //    r++;
            //}
            //return r;
        }

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
            Console.WriteLine($"{a * b} = {a} * {b}");
            Console.ReadKey();
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
            while (true)
            {
                int a = randomObject.Next(2, n-1);
                printInt("a = ", a);
                int d = Gcd(a, n);
                if (d != 1)
                {
                    output(d, n / d);
                }
                int r = OrderFinding(a, n);
                printInt("r = ", r);
                if (r % 2 == 1)
                    continue;
                printInt("a^{r/2} = ", Mpow(a, r / 2, n));
                if (Mpow(a, r / 2, n) == n - 1)
                    continue;

                int f = Math.Max(Gcd(Mpow(a, r / 2, n) + 1, n), Gcd(Mpow(a, r / 2, n) - 1, n));
                printInt("f = ", f);
                output(f, n / f);
            }
        }

    }

}