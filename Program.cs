using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CC7RSCRAPE
{
    struct Enemy
    {
        public Int32 ID;
        public Int32 HP;
        public Int32 MaxHP;
        public Int32 MP;
        public Int32 MaxMP;
        public Int32 AP;
        public Int32 MaxAP;
        public Int32 Unknown1;
        public Int32 Unknown2;

        public override string ToString() => $"{ID},{HP},{MaxHP},{MP},{MaxMP},{AP},{MaxAP},{Unknown1},{Unknown2}";
    }

    public class MemoryManager
    {
        const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        static void Main(string[] args)
        {
            Process process = Process.GetProcessesByName("CCFF7R-Win64-Shipping")[0];
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
            Int32 processHandleID = (int)processHandle;

            var seen = new List<int>();
            while (true)
            {
                for (int i = 0; i < 7; i++)
                {

                    (Enemy enemy, bool success) = MemoryManager.read(process, processHandleID, 0x07194F38 + (i * 0x0740));
                    if (success && !seen.Contains(enemy.ID))
                    {
                        seen.Add(enemy.ID);
                        Console.WriteLine(enemy.ToString());
                    }
                }
            }
        }

        private static (Enemy enemy, bool success) read(Process process, int processHandle, Int32 address)
        {
            var bytesRead = 0;
            byte[] buffer = new byte[720];
            Int64 pointer = IntPtr.Add(process.Modules[0].BaseAddress, address).ToInt64();
            ReadProcessMemory((int)processHandle, pointer, buffer, buffer.Length, ref bytesRead);
            if (bytesRead == 0) { return (new Enemy(), false); }

            Enemy enemy = new Enemy();
            enemy.ID = BitConverter.ToInt32(buffer, 0);
            enemy.HP = BitConverter.ToInt32(buffer, 240);
            enemy.MaxHP = BitConverter.ToInt32(buffer, 244);
            enemy.MP = BitConverter.ToInt32(buffer, 248);
            enemy.MaxMP = BitConverter.ToInt32(buffer, 252);
            enemy.AP = BitConverter.ToInt32(buffer, 256);
            enemy.MaxAP = BitConverter.ToInt32(buffer, 260);
            enemy.Unknown1 = BitConverter.ToInt32(buffer, 264);
            enemy.Unknown2 = BitConverter.ToInt32(buffer, 268);
            if(enemy.ID == 0) { return (new Enemy(), false); }
            return (enemy, true);
        }
    }
}