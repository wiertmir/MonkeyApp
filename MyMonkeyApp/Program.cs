namespace MyMonkeyApp;

using System;
using System.Linq;
using System.Threading.Tasks;
using MyMonkeyApp.Helpers;
using MyMonkeyApp.Models;

/// <summary>
/// Console UI for exploring monkeys via <see cref="MonkeyHelper"/>.
/// </summary>
internal static class Program
{
	private static readonly string[] AsciiArts = new[]
	{
		"(\\_/)\n( •_•)\n/ >🐒",
		"  _\n ('_')\n/)>🐵",
		"  .-\"\"-.\n /      \\n|  O  O |\n|  \\__/ |\n \\      /\n  `----`"
	};

	private static async Task Main(string[] args)
	{
		while (true)
		{
			PrintMenu();
			var input = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(input))
			{
				continue;
			}

			switch (input.Trim())
			{
				case "1":
					await ListAllMonkeysAsync();
					break;
				case "2":
					await GetDetailsByNameAsync();
					break;
				case "3":
					await GetRandomMonkeyAsync();
					break;
				case "4":
				case "q":
				case "exit":
					Console.WriteLine("Goodbye!");
					return;
				default:
					Console.WriteLine("Unknown option. Choose 1-4.");
					break;
			}
		}
	}

	private static void PrintMenu()
	{
		Console.WriteLine();
		Console.WriteLine("Monkey Explorer");
		Console.WriteLine("1) List all monkeys");
		Console.WriteLine("2) Get details for a monkey by name");
		Console.WriteLine("3) Get a random monkey");
		Console.WriteLine("4) Exit (or q)");
		Console.Write("Select an option: ");
	}

	private static async Task ListAllMonkeysAsync()
	{
		var monkeys = await MonkeyHelper.GetMonkeysAsync();
		if (monkeys == null || monkeys.Count == 0)
		{
			Console.WriteLine("No monkeys available.");
			return;
		}

		Console.WriteLine();
		Console.WriteLine("{0,-25} {1,-30} {2,10} {3,10} {4,10}", "Name", "Location", "Population", "Latitude", "Longitude");
		Console.WriteLine(new string('-', 95));

		foreach (var m in monkeys)
		{
			Console.WriteLine("{0,-25} {1,-30} {2,10} {3,10:F6} {4,10:F6}", Truncate(m.Name,25), Truncate(m.Location,30), m.Population, m.Latitude, m.Longitude);
		}
	}

	private static async Task GetDetailsByNameAsync()
	{
		Console.Write("Enter monkey name: ");
		var name = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(name))
		{
			Console.WriteLine("Name cannot be empty.");
			return;
		}

		var monkey = await MonkeyHelper.GetMonkeyByNameAsync(name.Trim());
		if (monkey == null)
		{
			Console.WriteLine($"No monkey found with name '{name}'.");
			return;
		}

		PrintMonkeyDetails(monkey);
	}

	private static async Task GetRandomMonkeyAsync()
	{
		var monkey = await MonkeyHelper.GetRandomMonkeyAsync();
		if (monkey == null)
		{
			Console.WriteLine("No monkeys available to pick.");
			return;
		}

		// show ASCII art randomly
		var art = AsciiArts[Random.Shared.Next(AsciiArts.Length)];
		Console.WriteLine();
		Console.WriteLine(art);
		Console.WriteLine();

		PrintMonkeyDetails(monkey);

		var count = MonkeyHelper.GetAccessCount(monkey.Name);
		Console.WriteLine($"(random-picked count for {monkey.Name}: {count})");
	}

	private static void PrintMonkeyDetails(Monkey m)
	{
		Console.WriteLine();
		Console.WriteLine($"Name: {m.Name}");
		Console.WriteLine($"Location: {m.Location}");
		Console.WriteLine($"Population: {m.Population}");
		Console.WriteLine($"Coordinates: {m.Latitude:F6}, {m.Longitude:F6}");
		Console.WriteLine($"Image: {m.Image}");
		Console.WriteLine("Details:");
		Console.WriteLine(m.Details);
	}

	private static string Truncate(string? value, int maxLength)
		=> string.IsNullOrEmpty(value) ? string.Empty : (value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...");
}
