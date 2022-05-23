using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWork_C_sharp.NetFramework_Console
{
    class MySingleton
    {
        private static MySingleton instance;
        public string Name { get; private set; }
        protected MySingleton(string name)
        {
            this.Name = name;
        }
        public static MySingleton getInstance(string name)
        {
            if (instance == null)
                instance = new MySingleton(name);
            return instance;
        }
    }
    class MyDevice
    {
        public MySingleton Singleton { get; set; }
        public void Charge(string MySingletonName)
        {
            Singleton = MySingleton.getInstance(MySingletonName);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"");
            Console.WriteLine($"123123");
            Console.WriteLine($"");
            Console.WriteLine($"");

            device1.Singleton = MySingleton.getInstance("");
            Console.WriteLine($"");

            MyDevice device2 = new MyDevice();
            device2.Charge("");
            device1.Charge("");
            Console.ReadLine();
        }
    }
}