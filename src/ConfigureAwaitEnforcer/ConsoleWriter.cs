namespace ConfigureAwaitEnforcer
{
    using System;

    /// <summary>
    /// Helper to tidy up code when writing to the console in different colours
    /// </summary>
    public static class ConsoleWriter
    {
        public static void Write(string text = null) => Write(text, Console.ForegroundColor);
        public static void Write(string text, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void WriteLine(string text = null) => WriteLine(text, Console.ForegroundColor);
        public static void WriteLine(string text, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}