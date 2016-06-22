using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ASM
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(CosDelayAsync2().Result);
            Console.ReadKey();
        }

        static async void CosAsync()
        {
            return;
        }

        static void CosAsync2()
        {
            var sm = new CosAsyncStateMachine();
            sm.mb = new AsyncVoidMethodBuilder(); // struktura, nie płacimy za sterte :)
            sm.mb.Start(ref sm); //Start nie alokuje - super bezpieczne, w razie potrzeby referencja pojdzie na Sterte
        }

        static async Task<string> CosDelayAsync()
        {
            await Task.Delay(10000);
            return "asdasd";
        }

        static Task<string> CosDelayAsync2()
        {
            var sm = new CosWithResultAsyncStateMachine();
            sm.mb = new AsyncTaskMethodBuilder<string>();
            sm.mb.Start(ref sm);
            return sm.mb.Task;
        }

        struct CosAsyncStateMachine: IAsyncStateMachine
        {
            public AsyncVoidMethodBuilder mb;
            public void MoveNext()
            {
                //return, throw await -> methodBuilder
                //Tutaj również zyje metoda stanowa :)
                
                mb.SetResult(); //trigger na zakończenie pracy
            }

            //przekazanie referencji do siebie samej: case heap vs stack
            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                //tutaj znajdujemy się w momencie ewakuacji
            }
        }

        struct CosWithResultAsyncStateMachine: IAsyncStateMachine
        {
            public AsyncTaskMethodBuilder<string> mb;
            private int state;
            private TaskAwaiter awaiter;

            public void MoveNext()
            {
                if (state == 0)
                {
                    awaiter = Task.Delay(5000).GetAwaiter();
                    if (awaiter.IsCompleted)
                    {
                        state = 1;
                        goto state1;
                    }
                    else
                    {
                        state = 1;
                        mb.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                    }
                    return;
                }

state1:
                if (state == 1)
                {
                    awaiter.GetResult();
                    mb.SetResult("asdasd");
                    return;

                }
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                mb.SetStateMachine(stateMachine);
            }
        }


    }
}
