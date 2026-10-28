namespace CacheClient.Menu
{
    public class Menu
    {
        public static int ManageMenu(List<string> options, string header)
        {
            if (options.Count == 0) return -1;

            int select = 0;
            while (true)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(header))
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(header);
                    Console.ResetColor();
                }

                for (int i = 0; i < options.Count; i++)
                {
                    if (i == select)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($">>  {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"    {options[i]}");
                    }
                }

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) return -1;
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    select--;
                    if (select < 0) select = options.Count - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    select++;
                    if (select >= options.Count) select = 0;
                }
                else if (key.Key == ConsoleKey.Enter) return select;
            }
        }
    }
}