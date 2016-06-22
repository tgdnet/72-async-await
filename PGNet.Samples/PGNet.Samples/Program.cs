using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PGNet.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Program progz = new Program();


            progz.DoTheJob().Wait();

            Console.Read();
        }

        
    }
}
