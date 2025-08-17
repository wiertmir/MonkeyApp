namespace MyMonkeyApp.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MyMonkeyApp.Models;

/// <summary>
/// Static helper that manages an in-memory collection of monkeys seeded from the MCP sample data.
/// Provides methods to get all monkeys, get a random monkey, find by name and track how often random picks occur.
/// </summary>
public static class MonkeyHelper
{
    private static readonly List<Monkey> _monkeys;
    private static readonly Dictionary<string, int> _accessCounts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _lock = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Embedded JSON copied from the MCP sample data.
    private const string _embeddedJson = @"[{""Name"":""Baboon"",""Location"":""Africa \u0026 Asia"",""Details"":""Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/baboon.jpg"",""Population"":10000,""Latitude"":-8.783195,""Longitude"":34.508523},{""Name"":""Capuchin Monkey"",""Location"":""Central \u0026 South America"",""Details"":""The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/capuchin.jpg"",""Population"":23000,""Latitude"":12.769013,""Longitude"":-85.602364},{""Name"":""Blue Monkey"",""Location"":""Central and East Africa"",""Details"":""The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia"",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/bluemonkey.jpg"",""Population"":12000,""Latitude"":1.957709,""Longitude"":37.297204},{""Name"":""Squirrel Monkey"",""Location"":""Central \u0026 South America"",""Details"":""The squirrel monkeys are the New World monkeys of the genus Saimiri. They are the only genus in the subfamily Saimirinae. The name of the genus Saimiri is of Tupi origin, and was also used as an English name by early researchers."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/saimiri.jpg"",""Population"":11000,""Latitude"":-8.783195,""Longitude"":-55.491477},{""Name"":""Golden Lion Tamarin"",""Location"":""Brazil"",""Details"":""The golden lion tamarin also known as the golden marmoset, is a small New World monkey of the family Callitrichidae."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/tamarin.jpg"",""Population"":19000,""Latitude"":-14.235004,""Longitude"":-51.92528},{""Name"":""Howler Monkey"",""Location"":""South America"",""Details"":""Howler monkeys are among the largest of the New World monkeys. Fifteen species are currently recognised. Previously classified in the family Cebidae, they are now placed in the family Atelidae."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/alouatta.jpg"",""Population"":8000,""Latitude"":-8.783195,""Longitude"":-55.491477},{""Name"":""Japanese Macaque"",""Location"":""Japan"",""Details"":""The Japanese macaque, is a terrestrial Old World monkey species native to Japan. They are also sometimes known as the snow monkey because they live in areas where snow covers the ground for months each"",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/macasa.jpg"",""Population"":1000,""Latitude"":36.204824,""Longitude"":138.252924},{""Name"":""Mandrill"",""Location"":""Southern Cameroon, Gabon, and Congo"",""Details"":""The mandrill is a primate of the Old World monkey family, closely related to the baboons and even more closely to the drill. It is found in southern Cameroon, Gabon, Equatorial Guinea, and Congo."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/mandrill.jpg"",""Population"":17000,""Latitude"":7.369722,""Longitude"":12.354722},{""Name"":""Proboscis Monkey"",""Location"":""Borneo"",""Details"":""The proboscis monkey or long-nosed monkey, known as the bekantan in Malay, is a reddish-brown arboreal Old World monkey that is endemic to the south-east Asian island of Borneo."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/borneo.jpg"",""Population"":15000,""Latitude"":0.961883,""Longitude"":114.55485},{""Name"":""Sebastian"",""Location"":""Seattle"",""Details"":""This little trouble maker lives in Seattle with James and loves traveling on adventures with James and tweeting @MotzMonkeys. He by far is an Android fanboy and is getting ready for the new Google Pixel 9!"",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/sebastian.jpg"",""Population"":1,""Latitude"":47.606209,""Longitude"":-122.332071},{""Name"":""Henry"",""Location"":""Phoenix"",""Details"":""An adorable Monkey who is traveling the world with Heather and live tweets his adventures @MotzMonkeys. His favorite platform is iOS by far and is excited for the new iPhone Xs!"",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/henry.jpg"",""Population"":1,""Latitude"":33.448377,""Longitude"":-112.074037},{""Name"":""Red-shanked douc"",""Location"":""Vietnam"",""Details"":""The red-shanked douc is a species of Old World monkey, among the most colourful of all primates. The douc is an arboreal and diurnal monkey that eats and sleeps in the trees of the forest."",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/douc.jpg"",""Population"":1300,""Latitude"":16.111648,""Longitude"":108.262122},{""Name"":""Mooch"",""Location"":""Seattle"",""Details"":""An adorable Monkey who is traveling the world with Heather and live tweets his adventures @MotzMonkeys. Her favorite platform is iOS by far and is excited for the new iPhone 16!"",""Image"":""https://raw.githubusercontent.com/jamesmontemagno/app-monkeys/master/Mooch.PNG"",""Population"":1,""Latitude"":47.608013,""Longitude"":-122.335167}]";

    static MonkeyHelper()
    {
        try
        {
            _monkeys = JsonSerializer.Deserialize<List<Monkey>>(_embeddedJson, _jsonOptions) ?? new();

            // initialize access counts (no lock required here; static ctor is thread-safe)
            foreach (var m in _monkeys)
            {
                if (!string.IsNullOrWhiteSpace(m.Name) && !_accessCounts.ContainsKey(m.Name))
                {
                    _accessCounts[m.Name] = 0;
                }
            }
        }
        catch (Exception ex)
        {
            // If parsing fails, fall back to an empty list but keep the helper usable.
            _monkeys = new List<Monkey>();
            _accessCounts.Clear();
            Console.Error.WriteLine($"Failed to initialize MonkeyHelper: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns all monkeys as a read-only list.
    /// </summary>
    public static Task<IReadOnlyList<Monkey>> GetMonkeysAsync()
        => Task.FromResult((IReadOnlyList<Monkey>)_monkeys);

    /// <summary>
    /// Returns a monkey by case-insensitive name match, or null if not found.
    /// </summary>
    /// <param name="name">Name to look up.</param>
    public static Task<Monkey?> GetMonkeyByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult<Monkey?>(null);
        }

        var found = _monkeys.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(found);
    }

    /// <summary>
    /// Returns a random monkey from the collection and increments its random-access counter.
    /// </summary>
    public static Task<Monkey?> GetRandomMonkeyAsync()
    {
        if (_monkeys.Count == 0)
        {
            return Task.FromResult<Monkey?>(null);
        }

        var index = Random.Shared.Next(_monkeys.Count);
        var selected = _monkeys[index];

        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(selected.Name))
            {
                // do nothing for unnamed entries
                return Task.FromResult<Monkey?>(selected);
            }

            if (_accessCounts.ContainsKey(selected.Name))
            {
                _accessCounts[selected.Name]++;
            }
            else
            {
                _accessCounts[selected.Name] = 1;
            }
        }

        return Task.FromResult<Monkey?>(selected);
    }

    /// <summary>
    /// Returns how many times <see cref="GetRandomMonkeyAsync"/> returned the named monkey.
    /// </summary>
    public static int GetAccessCount(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return 0;
        }

        lock (_lock)
        {
            _accessCounts.TryGetValue(name, out var count);
            return count;
        }
    }
}
