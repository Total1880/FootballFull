using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballFull.ConsoleUI
{
    internal static class ConsoleUIHelper
    {
        public static void Header(string title, bool clear = true)
        {
            if (clear) Console.Clear();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();
        }

        public static void Section(string title)
        {
            Console.WriteLine();
            Console.WriteLine(title.ToUpperInvariant());
            Console.WriteLine(new string('-', 48));
        }

        public static void Value(string label, string? value)
        {
            // Ruime labelkolom, zonder lange toelichtingen af te kappen.
            Console.WriteLine($"  {label,-25} : {value ?? "-"}");
        }

        public static string Toggle(bool enabled) => enabled ? "[AAN]" : "[UIT]";

        public static void InvalidChoice()
        {
            Console.WriteLine();
            Console.WriteLine("Ongeldige keuze. Druk op een toets om opnieuw te proberen...");
            Console.ReadKey(true);
        }

        public static void Pause()
        {
            Console.WriteLine();
            Console.Write("Druk op een toets om verder te gaan...");
            Console.ReadKey(true);
        }

        public static int ReadInt(string prompt, int min, int max)
        {
            if (max < min) throw new ArgumentOutOfRangeException(nameof(max));
            while (true)
            {
                Console.Write($"{prompt} ({min}-{max}): ");
                var input = Console.ReadLine();
                if (input == null) throw new InvalidOperationException("Console-invoer is afgesloten.");
                if (int.TryParse(input, out var value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"Ongeldige keuze. Geef een nummer tussen {min} en {max}.");
            }
        }

        public static string Fit(string? value, int width)
        {
            if (width < 1) return string.Empty;
            var text = value ?? "-";
            if (text.Length <= width) return text.PadRight(width);
            return width == 1 ? "…" : text.Substring(0, width - 1) + "…";
        }

        // Page through long tables without losing the ability to return to the caller.
        public static void Table(string title, IReadOnlyList<string> headers,
            IReadOnlyList<IReadOnlyList<string>> rows, bool waitForUser = true, int pageSize = 18)
        {
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (headers.Count == 0) throw new ArgumentException("Een tabel moet kolommen hebben.", nameof(headers));
            var widths = headers.Select((h, i) =>
                Math.Min(30, Math.Max(h.Length, rows.Count == 0 ? 0 :
                    rows.Max(r => i < r.Count ? (r[i] ?? "").Length : 0)))).ToArray();
            var pages = Math.Max(1, (rows.Count + pageSize - 1) / pageSize);
            var page = 0;
            do
            {
                Header(title);
                Console.WriteLine(string.Join(" | ", headers.Select((h, i) => Fit(h, widths[i]))));
                Console.WriteLine(new string('-', widths.Sum() + 3 * (widths.Length - 1)));
                if (rows.Count == 0) Console.WriteLine("Geen gegevens beschikbaar.");
                foreach (var row in rows.Skip(page * pageSize).Take(pageSize))
                    Console.WriteLine(string.Join(" | ", widths.Select((w, i) => Fit(i < row.Count ? row[i] : "", w))));
                if (!waitForUser) return;
                Console.WriteLine();
                Console.WriteLine($"Pagina {page + 1}/{pages} · [V] Volgende · [P] Vorige · [Enter] Terug");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter || key == ConsoleKey.Escape) return;
                if (key == ConsoleKey.V || key == ConsoleKey.RightArrow) page = Math.Min(pages - 1, page + 1);
                if (key == ConsoleKey.P || key == ConsoleKey.LeftArrow) page = Math.Max(0, page - 1);
            } while (true);
        }
    }
}
