using System;
using System.Threading;
using System.Collections.Concurrent;

namespace FractalTerrainGen
{
    public static class NonBlockingConsole
    {
        private static BlockingCollection<string> m_Queue = new BlockingCollection<string>();

        static NonBlockingConsole()
        {
            var thread = new Thread(
              () =>
              {
                  while (true) Console.WriteLine(m_Queue.Take());
              });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void WriteLine(string value, params object[] args)
        {
            m_Queue.Add( String.Format(value, args) );
        }
    }
}
