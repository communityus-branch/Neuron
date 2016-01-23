using System;
using Static_Interface.ExtensionSandbox;

namespace Static_Interface.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program(args);
        }

        public Program(string[] args)
        {
            string illegalType;
            string failedInstruction;
            ApiUtils.AddWhitelist(GetType().Assembly);
            bool isSafe = ApiUtils.IsSafeAssembly(GetType().Assembly, out illegalType, out failedInstruction);
            Console.WriteLine("IsSafeAssembly: " + isSafe);
            if(!isSafe) Console.WriteLine("Verify failed on " + failedInstruction + ", illegal call detected: " + illegalType);
            Console.ReadKey();
        }
    }
}
