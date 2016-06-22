using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ContextSwitching
{
    class AsyncTests
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SyncTest()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static async void AsyncTest()
        {
            
        }
    }
}
