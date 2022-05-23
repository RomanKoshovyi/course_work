using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWork_C_sharp.NetFramework_Console
{
    class MyDevice
    {
        public MySingleton Singleton { get; set; }
        public void Charge(string MySingletonName)
        {
            Singleton = MySingleton.getInstance(MySingletonName);
        }
    }
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
    class Program
    {
        static void Main(string[] args)
        {
            MyDevice device1 = new MyDevice();
            device1.Charge("Charger 3.1");
            Console.WriteLine($"device#1 - {device1.Singleton.Name}");

            device1.Singleton = MySingleton.getInstance("Fast Charger 4.5");
            Console.WriteLine($"device#1 - {device1.Singleton.Name}");

            MyDevice device2 = new MyDevice();
            device2.Charge("New Charger 10.3");
            Console.WriteLine($"device#2 - {device2.Singleton.Name}");
            Console.WriteLine($"device1 == device2? : {MyDevice.ReferenceEquals(device1, device2)}");
            Console.WriteLine($"device1.Singleton == device2.Singleton? : {MySingleton.ReferenceEquals(device1.Singleton, device2.Singleton)}");
            Console.ReadLine();
        }
    }
}