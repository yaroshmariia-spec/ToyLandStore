using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ToylandStore
{
    public struct Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public DateTime ArrivalDate { get; set; }

        public Product(string name, double price, int quantity, DateTime arrivalDate)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
            ArrivalDate = arrivalDate;
        }

        public void Display(int index)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"| {index,3} | {Name,-25} | {Price,10:F2} грн | {Quantity,6} шт | {ArrivalDate:dd.MM.yyyy,-12} |");
            Console.ResetColor();
        }

        public string ToCsv() => $"{Name},{Price.ToString(CultureInfo.InvariantCulture)},{Quantity},{ArrivalDate:yyyy-MM-dd}";
    }

    class Program
    {
        static List<Product> products = new List<Product>();
        static string FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "products.csv");
        static Random rnd = new Random();
        
        static string AdminLoginStr = "admin";
        static string AdminPassStr = "1234";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            LoadData(); 
            ShowMainMenu();
        }

        // --- ДИЗАЙН ТА ЕФЕКТИ ---
        static void PrintSparkles()
        {
            string[] sparkles = { "*", "+", ".", "°", "¤", "·" };
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 60; i++)
                sb.Append(rnd.Next(0, 7) == 0 ? sparkles[rnd.Next(sparkles.Length)] : " ");
            PrintColor(sb.ToString(), ConsoleColor.Yellow);
        }

        static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                PrintSparkles();
                PrintColor(@"  ▀█▀ ▄▀▀▄ █░█ █░░ ▄▀▄ █▄░█ █▀▄", ConsoleColor.Magenta);
                PrintColor(@"  ░█░ █░░█ ▀█▀ █░░ █▄█ █▀██ █░█", ConsoleColor.Magenta);
                PrintColor(@"  ░▀░ ░▀▀░ ░▀░ ▀▀▀ ▀░▀ ▀░░▀ ▀▀░", ConsoleColor.Magenta);
                PrintSparkles();
                
                PrintColor("\n   >>> Ласкаво просимо в наш магазин дитячих іграшок! <<<", ConsoleColor.Cyan);
                
                Console.WriteLine("\n  [1] -> Каталог товарів");
                Console.WriteLine("  [2] -> Пошук іграшки");
                PrintColor("  [3] -> ПЕРЕЙТИ ДО ПОКУПОК (КОШИК)", ConsoleColor.Yellow);
                PrintColor("  [4] -> ПАНЕЛЬ АДМІНІСТРАТОРА", ConsoleColor.DarkCyan);
                PrintColor("  [0] -> Вихід", ConsoleColor.Red);

                int choice = GetIntInput("\n  Ваш вибір: ");

                switch (choice)
                {
                    case 1: DisplayProducts(); Console.ReadKey(); break;
                    case 2: SearchProduct(); break;
                    case 3: ProcessClientPurchase(); break;
                    case 4: if (UserLogin()) ShowAdminMenu(); break;
                    case 0: SaveData(); Environment.Exit(0); break;
                }
            }
        }

        // --- МОДУЛЬ ПОКУПКИ ТА КОШИКА ---
        static void ProcessClientPurchase()
        {
            var cart = new List<(int prodIdx, string name, int qty, double total)>();
            while (true)
            {
                Console.Clear();
                DisplayProducts();
                PrintColor("\n--- ВАШ КОШИК ---", ConsoleColor.Yellow);
                if (!cart.Any()) Console.WriteLine("Порожньо");
                else cart.ForEach(c => PrintColor($" {cart.IndexOf(c)+1}. {c.name} x{c.qty} = {c.total:F2} грн", ConsoleColor.Cyan));
                
                PrintColor($"\nСУМА ДО ОПЛАТИ: {cart.Sum(x => x.total):F2} грн", ConsoleColor.Green);
                Console.WriteLine("\n[№] Додати товар | [8] Видалити з кошика | [0] Оплатити чек");

                int choice = GetIntInput("\nДія: ");
                if (choice == 0) break;
                if (choice == 8 && cart.Any())
                {
                    int del = GetIntInput("Який номер з кошика видалити? ") - 1;
                    if (del >= 0 && del < cart.Count)
                    {
                        var item = cart[del];
                        var p = products[item.prodIdx];
                        p.Quantity += item.qty;
                        products[item.prodIdx] = p;
                        cart.RemoveAt(del);
                    }
                    continue;
                }

                int id = choice - 1;
                if (id >= 0 && id < products.Count)
                {
                    int q = GetIntInput($"Кількість {products[id].Name}: ");
                    if (q > 0 && q <= products[id].Quantity)
                    {
                        cart.Add((id, products[id].Name, q, products[id].Price * q));
                        var p = products[id]; p.Quantity -= q; products[id] = p;
                    } else PrintColor("Немає стільки на складі!", ConsoleColor.Red);
                }
            }
            if (cart.Any()) ShowReceipt(cart);
        }

        // --- МОДУЛЬ АДМІНІСТРАТОРА ---
        static void ShowAdminMenu()
        {
            while (true)
            {
                Console.Clear();
                PrintColor("========= ПАНЕЛЬ УПРАВЛІННЯ =========", ConsoleColor.DarkCyan);
                Console.WriteLine("1. Додати новий товар");
                Console.WriteLine("2. Редагувати товар (Ціна/К-сть)");
                Console.WriteLine("3. Видалити товар");
                Console.WriteLine("4. Загальна статистика");
                Console.WriteLine("5. Перевірка залишків (Дефіцит < 5)");
                Console.WriteLine("6. Змінити пароль адміна");
                Console.WriteLine("0. Назад");

                int choice = GetIntInput("\nОберіть дію: ");
                if (choice == 0) break;

                switch (choice)
                {
                    case 1: AddNewProduct(); break;
                    case 2: EditProduct(); break;
                    case 3: RemoveProduct(); break;
                    case 4: ShowAdminStats(); break;
                    case 5: ShowDeficit(); break;
                    case 6: 
                        Console.Write("Новий пароль: "); AdminPassStr = Console.ReadLine();
                        PrintColor("Пароль змінено!", ConsoleColor.Green); Console.ReadKey();
                        break;
                }
                SaveData();
            }
        }

        static void ShowAdminStats()
        {
            Console.Clear();
            double totalValue = products.Sum(p => p.Price * p.Quantity);
            PrintColor("--- СТАТИСТИКА СКЛАДУ ---", ConsoleColor.Yellow);
            Console.WriteLine($"Товарів у базі: {products.Count}");
            Console.WriteLine($"Загальна вартість активів: {totalValue:F2} грн.");
            if (products.Any()) Console.WriteLine($"Найдорожчий товар: {products.OrderByDescending(x => x.Price).First().Name}");
            Console.WriteLine("\nНатисніть клавішу...");
            Console.ReadKey();
        }

        static void ShowDeficit()
        {
            Console.Clear();
            var lowStock = products.Where(p => p.Quantity < 5).ToList();
            PrintColor("--- ТОВАРИ, ЩО ЗАКІНЧУЮТЬСЯ ---", ConsoleColor.Red);
            if (!lowStock.Any()) Console.WriteLine("Дефіциту немає!");
            else lowStock.ForEach(p => Console.WriteLine($"- {p.Name}: залишилося {p.Quantity} шт."));
            Console.ReadKey();
        }

        static void AddNewProduct()
        {
            Console.Write("Назва: "); string n = Console.ReadLine();
            double p = GetDoubleInput("Ціна: ");
            int q = GetIntInput("Кількість: ");
            products.Add(new Product(n, p, q, DateTime.Now));
            PrintColor("Товар успішно додано!", ConsoleColor.Green);
            Console.ReadKey();
        }

        static void EditProduct()
        {
            DisplayProducts();
            int id = GetIntInput("\n№ товару для редагування: ") - 1;
            if (id >= 0 && id < products.Count)
            {
                products[id] = new Product(products[id].Name, GetDoubleInput("Нова ціна: "), GetIntInput("Нова к-сть: "), DateTime.Now);
                PrintColor("Оновлено!", ConsoleColor.Green);
            }
            Console.ReadKey();
        }

        static void RemoveProduct()
        {
            DisplayProducts();
            int id = GetIntInput("\n№ для видалення: ") - 1;
            if (id >= 0 && id < products.Count) { products.RemoveAt(id); PrintColor("Видалено!", ConsoleColor.Red); }
            Console.ReadKey();
        }

        // --- ВАЛІДАЦІЯ ТА СИСТЕМНІ МЕТОДИ ---
        static int GetIntInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int res)) return res;
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                PrintColor("!! Введіть число !!", ConsoleColor.Red);
                System.Threading.Thread.Sleep(600);
            }
        }

        static double GetDoubleInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double res)) return res;
                PrintColor("!! Помилка ціни !!", ConsoleColor.Red);
            }
        }

        static bool UserLogin()
        {
            Console.Clear();
            PrintColor("--- ВХІД ---", ConsoleColor.DarkCyan);
            Console.Write("Логін (0-назад): "); string l = Console.ReadLine();
            if (l == "0") return false;
            Console.Write("Пароль: "); string p = Console.ReadLine();
            if (l == AdminLoginStr && p == AdminPassStr) return true;
            PrintColor("Невірно!", ConsoleColor.Red); Console.ReadKey();
            return false;
        }

        static void ShowReceipt(List<(int prodIdx, string name, int qty, double total)> cart)
        {
            Console.Clear();
            PrintColor("********************************", ConsoleColor.Yellow);
            PrintColor("* TOYLAND ЧЕК           *", ConsoleColor.Yellow);
            PrintColor("********************************", ConsoleColor.Yellow);
            cart.ForEach(i => Console.WriteLine($"{i.name,-15} x{i.qty} = {i.total:F2}"));
            PrintColor("--------------------------------", ConsoleColor.White);
            PrintColor($"ВСЬОГО: {cart.Sum(x => x.total):F2} грн", ConsoleColor.Green);
            PrintColor("********************************", ConsoleColor.Yellow);
            SaveData(); Console.ReadKey();
        }

        static void PrintColor(string t, ConsoleColor c) { Console.ForegroundColor = c; Console.WriteLine(t); Console.ResetColor(); }

        static void LoadData()
        {
            if (File.Exists(FileName))
            {
                var lines = File.ReadAllLines(FileName, Encoding.UTF8);
                foreach (var line in lines.Skip(1))
                {
                    var p = line.Split(',');
                    if (p.Length == 4) products.Add(new Product(p[0], double.Parse(p[1], CultureInfo.InvariantCulture), int.Parse(p[2]), DateTime.Parse(p[3])));
                }
            }
        }

        static void SaveData()
        {
            var lines = new List<string> { "Name,Price,Quantity,ArrivalDate" };
            products.ForEach(p => lines.Add(p.ToCsv()));
            File.WriteAllLines(FileName, lines, Encoding.UTF8);
        }

        static void DisplayProducts()
        {
            PrintColor(new string('=', 70), ConsoleColor.Blue);
            Console.WriteLine($"| {"№",2} | {"Назва",-25} | {"Ціна",-10} | {"К-сть",-5} |");
            PrintColor(new string('-', 70), ConsoleColor.Blue);
            for (int i = 0; i < products.Count; i++) products[i].Display(i + 1);
            PrintColor(new string('=', 70), ConsoleColor.Blue);
        }

        static void SearchProduct()
        {
            Console.Write("Пошук: "); string q = Console.ReadLine().ToLower();
            var found = products.Where(x => x.Name.ToLower().Contains(q)).ToList();
            if (found.Any()) found.ForEach(f => f.Display(products.IndexOf(f) + 1));
            else PrintColor("Не знайдено!", ConsoleColor.Red);
            Console.ReadKey();
        }
    }
}