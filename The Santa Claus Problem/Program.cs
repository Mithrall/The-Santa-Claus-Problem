using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace The_Santa_Claus_Problem {
    class Program {
        static Thread[] threads = new Thread[51];

        static int elfCount = 0;
        static int reindeerCount = 0;

        static Semaphore santaSem = new Semaphore(0,1);     //Where Santa waits
        static Semaphore reindeerSem = new Semaphore(0,9);  //Where reindeer wait
        static Semaphore elfSem = new Semaphore(0,10);      //Where elves wait

        static Mutex m = new Mutex();           //Protects the counters
        static Mutex elfMutex = new Mutex();    //blocks other elves while three
                                                //elves are being helped

        static void Main(string[] args) {
            Random rnd = new Random();
            string type;
            threads[0] = new Thread(santaLoop);
            threads[0].Start();

            for (int i = 1; i < threads.Length-1; i++) {

                threads[i] = new Thread(ThreadProcess);
                type = rnd.Next(2).ToString();
                switch (type) {
                    case "0":
                        threads[i].Name = "reindeer";
                        threads[i].Start();
                        break;
                    case "1":
                        threads[i].Name = "elf";
                        threads[i].Start();
                        break;
                    default:
                        break;
                }
            }
            Console.Read();
        }

        private static void santaLoop() {
            while (true) {
                Console.WriteLine("Santa is sleeping");
                santaSem.WaitOne();
                Console.WriteLine("Santa has been woken up");
                m.WaitOne();
                Console.WriteLine("Locked");
                if (reindeerCount == 9) {
                    prepSleigh();
                    reindeerSem.Release(9);
                    reindeerCount = 0;
                } else {
                    elfSem.Release(3);
                    helpElves();
                }
                Console.WriteLine("Unlocked");
                m.ReleaseMutex();
            }
        }

        private static void helpElves() {
            Console.WriteLine("Santa is helping the elves");
        }

        private static void prepSleigh() {
            Console.WriteLine("Santa prepped the sleigh");
        }

        static void ThreadProcess() {
            //if elf
            if (Thread.CurrentThread.Name == "elf") {
                Console.WriteLine("An elf is waiting at the door");
                elfMutex.WaitOne();
                Console.WriteLine("An elf is waiting in line, closing door");
                m.WaitOne();
                Console.WriteLine("locked");
                elfCount++;
                if (elfCount == 3) {
                    Console.WriteLine(elfCount + " elfs are waiting for help, signaling Santa");
                    santaSem.Release();
                } else {                    
                    elfMutex.ReleaseMutex();
                    Console.WriteLine(elfCount + " elfs are waiting for help, opening door");
                }
                Console.WriteLine("unlocked");
                m.ReleaseMutex();
                elfSem.WaitOne();
                getHelp();
                m.WaitOne();
                Console.WriteLine("locked");
                elfCount--;
                Console.WriteLine(elfCount + " elfs still waiting for help");
                if (elfCount == 0) {
                    Console.WriteLine("opening door");
                    elfMutex.ReleaseMutex();
                }
                Console.WriteLine("unlocked");
                m.ReleaseMutex();
            //if reindeer
            } else {
                Console.WriteLine("A reindeer is waiting in line");
                m.WaitOne();
                Console.WriteLine("locked");
                reindeerCount++;
                if (reindeerCount == 9) {
                    Console.WriteLine(reindeerCount + " (all) reindeers are ready, signaling Santa");
                    santaSem.Release();
                } else Console.WriteLine(reindeerCount + " Reindeers are ready");
                Console.WriteLine("unlocked");
                m.ReleaseMutex();
                reindeerSem.WaitOne();
                getHitched();
            }
        }

        private static void getHelp() {
            Console.WriteLine("an elf is getting help");
        }

        private static void getHitched() {
            Console.WriteLine("a reindeer is hitched");
        }
    }
}
