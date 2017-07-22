using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace savesave
{
    class Program
    {

       
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world!");

            var directory = System.IO.Directory.GetCurrentDirectory();

            Console.WriteLine(directory);
        }


    }
}
