using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PersonalFinanceManager
{
    // ======== MODELS (OOP) ========

    // Abstract base class (Abstraction)
    abstract class Transaction
    {
        public string Username { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        protected Transaction(string username, decimal amount, string category, DateTime date, string description)
        {
            Username = username;
            Amount = amount;
            Category = category;
            Date = date;
            Description = description;
        }

        // Polymorphism: overridden in derived classes
        public abstract string GetTypeName();
    }

    class Income : Transaction
    {
        public Income(string username, decimal amount, string category, DateTime date, string description)
            : base(username, amount, category, date, description)
        {
        }

        public override string GetTypeName()
        {
            return "Income";
        }
    }

    class Expense : Transaction
    {
        public Expense(string username, decimal amount, string category, DateTime date, string description)
            : base(username, amount, category, date, description)
        {
        }

        public override string GetTypeName()
        {
            return "Expense";
        }
    }

    class User
    {
        // Encapsulation: private set
        public string Username { get; private set; }
        public string Password { get; private set; }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    class SavingsGoal
    {
        public string Username { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime TargetDate { get; set; }

        public SavingsGoal(string username, decimal targetAmount, DateTime targetDate)
        {
            Username = username;
            TargetAmount = targetAmount;
            TargetDate = targetDate;
        }
    }

    // ======== CORE MANAGER ========

    class FinanceManager
    {
        private readonly string usersFile = "users.csv";
        private readonly string transactionsFile = "transactions.csv";
        private readonly string goalsFile = "goals.csv";

        private List<User> users = new List<User>();
        private List<Transaction> transactions = new List<Transaction>();
        private List<SavingsGoal> goals = new List<SavingsGoal>();

        public FinanceManager()
        {
            LoadUsers();
            LoadTransactions();
            LoadGoals();
        }

        // ---------- USERS ----------

        private void LoadUsers()
        {
            users.Clear();
            if (!File.Exists(usersFile)) return;

            foreach (var line in File.ReadAllLines(usersFile))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length >= 2)
                {
                    users.Add(new User(parts[0], parts[1]));
                }
            }
        }

        private void SaveUsers()
        {
            var lines = users.Select(u => $"{u.Username};{u.Password}");
            File.WriteAllLines(usersFile, lines);
        }

        public bool Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return false;

            users.Add(new User(username, password));
            SaveUsers();
            return true;
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            return users.Any(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        // ---------- TRANSACTIONS ----------

        private void LoadTransactions()
        {
            transactions.Clear();
            if (!File.Exists(transactionsFile)) return;

            foreach (var line in File.ReadAllLines(transactionsFile))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length >= 6)
                {
                    string username = parts[0];
                    string type = parts[1];
                    decimal amount = decimal.Parse(parts[2], CultureInfo.InvariantCulture);
                    string category = parts[3];
                    DateTime date = DateTime.ParseExact(parts[4], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string description = parts[5];

                    if (type == "Income")
                    {
                        transactions.Add(new Income(username, amount, category, date, description));
                    }
                    else if (type == "Expense")
                    {
                        transactions.Add(new Expense(username, amount, category, date, description));
                    }
                }
            }
        }

        private void SaveTransactions()
        {
            var lines = transactions.Select(t =>
                $"{t.Username};{t.GetTypeName()};{t.Amount.ToString(CultureInfo.InvariantCulture)};{t.Category};{t.Date:yyyy-MM-dd};{t.Description}");
            File.WriteAllLines(transactionsFile, lines);
        }

        public void AddIncome(string username, decimal amount, string category, DateTime date, string description)
        {
            var income = new Income(username, amount, category, date, description);
            transactions.Add(income);
            SaveTransactions();
        }

        public void AddExpense(string username, decimal amount, string category, DateTime date, string description)
        {
            var expense = new Expense(username, amount, category, date, description);
            transactions.Add(expense);
            SaveTransactions();
        }

        public List<Transaction> GetUserTransactions(string username)
        {
            return transactions
                .Where(t => t.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.Date)
                .ToList();
        }

        public List<Transaction> GetUserTransactionsForMonth(string username, int year, int month)
        {
            return GetUserTransactions(username)
                .Where(t => t.Date.Year == year && t.Date.Month == month)
                .ToList();
        }

        // ---------- SAVINGS GOALS ----------

        private void LoadGoals()
        {
            goals.Clear();
            if (!File.Exists(goalsFile)) return;

            foreach (var line in File.ReadAllLines(goalsFile))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length >= 3)
                {
                    string username = parts[0];
                    decimal target = decimal.Parse(parts[1], CultureInfo.InvariantCulture);
                    DateTime date = DateTime.ParseExact(parts[2], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    goals.Add(new SavingsGoal(username, target, date));
                }
            }
        }

        private void SaveGoals()
        {
            var lines = goals.Select(g =>
                $"{g.Username};{g.TargetAmount.ToString(CultureInfo.InvariantCulture)};{g.TargetDate:yyyy-MM-dd}");
            File.WriteAllLines(goalsFile, lines);
        }

        public void SetSavingsGoal(string username, decimal targetAmount, DateTime targetDate)
        {
            var existing = goals.FirstOrDefault(g =>
                g.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.TargetAmount = targetAmount;
                existing.TargetDate = targetDate;
            }
            else
            {
                goals.Add(new SavingsGoal(username, targetAmount, targetDate));
            }

            SaveGoals();
        }

        public SavingsGoal? GetSavingsGoal(string username)
        {
            return goals.FirstOrDefault(g =>
                g.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public decimal GetTotalSavings(string username)
        {
            var userTransactions = GetUserTransactions(username);
            decimal totalIncome = userTransactions
                .Where(t => t is Income)
                .Sum(t => t.Amount);
            decimal totalExpense = userTransactions
                .Where(t => t is Expense)
                .Sum(t => t.Amount);
            return totalIncome - totalExpense;
        }

        // ---------- EXPORT ----------

        public void ExportUserTransactionsToCsv(string username, string exportPath)
        {
            var userTransactions = GetUserTransactions(username);
            var lines = new List<string>
            {
                "Type;Amount;Category;Date;Description"
            };

            lines.AddRange(userTransactions.Select(t =>
                $"{t.GetTypeName()};{t.Amount.ToString(CultureInfo.InvariantCulture)};{t.Category};{t.Date:yyyy-MM-dd};{t.Description}"));

            File.WriteAllLines(exportPath, lines);
        }

        // ---------- SUMMARY & CHART ----------

        public void PrintMonthlySummary(string username, int year, int month)
        {
            var list = GetUserTransactionsForMonth(username, year, month);
            decimal totalIncome = list.Where(t => t is Income).Sum(t => t.Amount);
            decimal totalExpense = list.Where(t => t is Expense).Sum(t => t.Amount);
            decimal balance = totalIncome - totalExpense;

            Console.WriteLine($"--- Monthly Summary for {month}/{year} ---");
            Console.WriteLine($"Total Income : {totalIncome:C}");
            Console.WriteLine($"Total Expense: {totalExpense:C}");
            Console.WriteLine($"Net Balance  : {balance:C}");
        }

        public void DisplaySummaryChart(string username, int year, int month)
        {
            var list = GetUserTransactionsForMonth(username, year, month);
            decimal totalIncome = list.Where(t => t is Income).Sum(t => t.Amount);
            decimal totalExpense = list.Where(t => t is Expense).Sum(t => t.Amount);

            Console.WriteLine($"--- Summary Chart for {month}/{year} ---");

            if (totalIncome == 0 && totalExpense == 0)
            {
                Console.WriteLine("No data for this month.");
                return;
            }

            int maxWidth = 40;
            decimal max = Math.Max(totalIncome, totalExpense);

            int incomeBar = max == 0 ? 0 : (int)Math.Round((totalIncome / max) * maxWidth);
            int expenseBar = max == 0 ? 0 : (int)Math.Round((totalExpense / max) * maxWidth);

            Console.WriteLine($"Income : {new string('#', incomeBar)} {totalIncome:C}");
            Console.WriteLine($"Expense: {new string('#', expenseBar)} {totalExpense:C}");
        }
    }

    // ======== PROGRAM (UI) ========

    class Program
    {
        static void Main(string[] args)
        {
            FinanceManager manager = new FinanceManager();
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                SafeClear();
                Console.WriteLine("=== Personal Finance Manager ===");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("0. Exit");
                Console.Write("Choose option: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "1")
                {
                    RegisterFlow(manager);
                }
                else if (choice == "2")
                {
                    if (LoginFlow(manager, out string? username) && username != null)
                    {
                        UserMenu(manager, username);
                    }
                }
                else if (choice == "0")
                {
                    break;
                }
            }
        }

        static void RegisterFlow(FinanceManager manager)
        {
            SafeClear();
            Console.WriteLine("--- Register ---");
            Console.Write("Username: ");
            string username = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            string password = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Username and password cannot be empty.");
            }
            else if (manager.Register(username, password))
            {
                Console.WriteLine("Registration successful!");
            }
            else
            {
                Console.WriteLine("Username already exists.");
            }
            Pause();
        }

        static bool LoginFlow(FinanceManager manager, out string? username)
        {
            SafeClear();
            Console.WriteLine("--- Login ---");
            Console.Write("Username: ");
            username = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            string password = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Username and password cannot be empty.");
                Pause();
                return false;
            }

            if (manager.Login(username, password))
            {
                Console.WriteLine("Login successful!");
                Pause();
                return true;
            }
            else
            {
                Console.WriteLine("Invalid credentials.");
                Pause();
                username = null;
                return false;
            }
        }

        static void UserMenu(FinanceManager manager, string username)
        {
            while (true)
            {
                SafeClear();
                Console.WriteLine($"=== Welcome, {username} ===");
                Console.WriteLine("1. Add Income");
                Console.WriteLine("2. Add Expense");
                Console.WriteLine("3. View Transaction History");
                Console.WriteLine("4. Monthly Summary Report");
                Console.WriteLine("5. Set / View Savings Goal");
                Console.WriteLine("6. Export Transactions to CSV");
                Console.WriteLine("7. Display Monthly Summary Chart");
                Console.WriteLine("0. Logout");
                Console.Write("Choose option: ");
                string? choice = Console.ReadLine();

                if (choice == "1")
                {
                    AddIncomeFlow(manager, username);
                }
                else if (choice == "2")
                {
                    AddExpenseFlow(manager, username);
                }
                else if (choice == "3")
                {
                    ViewTransactionsFlow(manager, username);
                }
                else if (choice == "4")
                {
                    MonthlySummaryFlow(manager, username);
                }
                else if (choice == "5")
                {
                    SavingsGoalFlow(manager, username);
                }
                else if (choice == "6")
                {
                    ExportFlow(manager, username);
                }
                else if (choice == "7")
                {
                    ChartFlow(manager, username);
                }
                else if (choice == "0")
                {
                    break;
                }
            }
        }

        static void AddIncomeFlow(FinanceManager manager, string username)
        {
            SafeClear();
            Console.WriteLine("--- Add Income ---");
            decimal amount = ReadDecimal("Amount: ");
            Console.Write("Category (e.g. Salary, Bonus): ");
            string category = Console.ReadLine() ?? "";
            DateTime date = ReadDate("Date (yyyy-MM-dd): ");
            Console.Write("Description: ");
            string description = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(category))
                category = "Other";
            if (string.IsNullOrWhiteSpace(description))
                description = "";

            manager.AddIncome(username, amount, category, date, description);
            Console.WriteLine("Income added.");
            Pause();
        }

        static void AddExpenseFlow(FinanceManager manager, string username)
        {
            SafeClear();
            Console.WriteLine("--- Add Expense ---");
            decimal amount = ReadDecimal("Amount: ");
            Console.Write("Category (e.g. Food, Rent, Transport): ");
            string category = Console.ReadLine() ?? "";
            DateTime date = ReadDate("Date (yyyy-MM-dd): ");
            Console.Write("Description: ");
            string description = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(category))
                category = "Other";
            if (string.IsNullOrWhiteSpace(description))
                description = "";

            manager.AddExpense(username, amount, category, date, description);
            Console.WriteLine("Expense added.");
            Pause();
        }

        static void ViewTransactionsFlow(FinanceManager manager, string username)
        {
            SafeClear();
            Console.WriteLine("--- Transaction History ---");
            var list = manager.GetUserTransactions(username);

            if (!list.Any())
            {
                Console.WriteLine("No transactions yet.");
            }
            else
            {
                foreach (var t in list)
                {
                    Console.WriteLine($"{t.Date:yyyy-MM-dd} | {t.GetTypeName(),7} | {t.Amount,10:C} | {t.Category,-10} | {t.Description}");
                }
            }
            Pause();
        }

        static void MonthlySummaryFlow(FinanceManager manager, string username)
        {
            SafeClear();
            Console.WriteLine("--- Monthly Summary ---");
            int year = ReadInt("Year (e.g. 2025): ");
            int month = ReadInt("Month (1-12): ");

            manager.PrintMonthlySummary(username, year, month);
            Pause();
        }

        static void SavingsGoalFlow(FinanceManager manager, string username)
        {
            SafeClear();
            Console.WriteLine("--- Savings Goal ---");
            var goal = manager.GetSavingsGoal(username);
            decimal currentSavings = manager.GetTotalSavings(username);

            if (goal != null)
            {
                Console.WriteLine($"Current goal: {goal.TargetAmount:C} by {goal.TargetDate:yyyy-MM-dd}");
                Console.WriteLine($"Current savings (income - expenses): {currentSavings:C}");
                decimal progress = goal.TargetAmount == 0 ? 0 : (currentSavings / goal.TargetAmount) * 100;
                if (progress < 0) progress = 0;
                Console.WriteLine($"Progress: {progress:F2}%");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("No savings goal set yet.");
            }

            Console.Write("Do you want to set/update the goal? (y/n): ");
            string ans = Console.ReadLine() ?? "";
            if (ans?.ToLower() == "y")
            {
                decimal target = ReadDecimal("Target amount: ");
                DateTime date = ReadDate("Target date (yyyy-MM-dd): ");
                manager.SetSavingsGoal(username, target, date);
                Console.WriteLine("Savings goal updated.");
            }

            Pause();
        }

        static void ExportFlow(FinanceManager manager, string username)
        {
            SafeClear();
            Console.WriteLine("--- Export Transactions ---");
            Console.Write("Enter export file name (e.g. mydata.csv): ");
            string fileName = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine("Invalid file name.");
            }
            else
            {
                manager.ExportUserTransactionsToCsv(username, fileName);
                Console.WriteLine($"Exported to {fileName}");
            }
            Pause();
        }

        static void ChartFlow(FinanceManager manager, string username)
        {
            SafeClear();
            Console.WriteLine("--- Monthly Summary Chart ---");
            int year = ReadInt("Year (e.g. 2025): ");
            int month = ReadInt("Month (1-12): ");

            manager.DisplaySummaryChart(username, year, month);
            Pause();
        }

        // ---------- HELPERS ----------

        static void SafeClear()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // Console.Clear() may fail in some environments
            }
        }

        static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }

        static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
                {
                    return value;
                }
                Console.WriteLine("Invalid number, try again.");
            }
        }

        static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime date))
                {
                    return date;
                }
                Console.WriteLine("Invalid date format, use yyyy-MM-dd.");
            }
        }

        static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int value))
                {
                    return value;
                }
                Console.WriteLine("Invalid number, try again.");
            }
        }
    }
}
