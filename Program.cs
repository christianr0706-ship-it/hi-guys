using System.Security.Cryptography;
using System.Linq;


// RNG successfully implemented
// int RollD6 = Random;

// if (RollD6 == 6 || RollD6 == 5 || RollD6 == 4)
// {
//     Console.WriteLine("Success");
// }
// else if (RollD6 == 3 || RollD6 == 2 || RollD6 == 1)
// {
//     Console.WriteLine("Failure");
// }


public class Card
{
    public string Suit {get;set;}
    public string Rank {get;set;}
    public override string ToString() => $"{Rank} of {Suit}";
}

public class Deck
{
    private List<Card> cards;
    private Random random;
    

    public Deck()
    {
        cards = new List<Card>();
        random = new Random();
        CreateDeck();
    }

    private void CreateDeck()
    {
        string[] suits = {"Hearts", "Diamonds", "Clubs", "Spades"};
        string[] ranks = {"Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10"};
        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                cards.Add(new Card {Suit = suit, Rank = rank});
            }
        }
    }

    public Card Draw()
    {
        if (cards.Count == 0) throw new InvalidOperationException("No cards left in deck. Something broke.");
        int index = random.Next(cards.Count);
        Card drawn = cards[index];
        cards.RemoveAt(index);
        return drawn;
    }

    public void Shuffle()
    {
        cards.Clear();
        CreateDeck();
    }

    public List<List<Card>> DrawGroup()
    {
        List<Card> hand = new List<Card>();
        for (int i = 0; i < 15; i++)
        {
            hand.Add(Draw());
        }
        List<List<Card>> groups = new List<List<Card>>();
        for (int g = 0; g < 3; g++)
        {
            groups.Add(new List<Card>());
            for (int c = 0; c < 5; c++)
            {
                groups[g].Add(hand[g * 5 + c]);
            }
        }
        for (int g = 0; g < 3; g++)
        {
            Console.WriteLine($"Group {g + 1}:");
            foreach (var card in groups[g])
            {
                Console.WriteLine($"{card}");
            }
            Console.WriteLine();
        }
        return groups;
    }
}

public class Player
{
    public string Name {get;set;}
    public int Health {get;set;}
    public int MaxHealth {get;set;}
    public int Treasure {get;set;}
    public string? Suit {get;set;} // Determines passive / suit ability
    public string? Face {get;set;} // King, Queen, or Jack. Determines per-floor ability

    public Player(string name, int maxHealth, int treasure, string suit, string face)
    {
        Name = name;
        Health = maxHealth;
        MaxHealth = maxHealth;
        Treasure = treasure;
        Suit = suit;
        Face = face;
    }
    

    private int GetCardLevel(Card card)
    {
        if (card.Rank == "Ace") return 1;
        return int.TryParse(card.Rank, out int value) ? value: 0;
    }

    public string ApplyRoomEffect(Card card)
    {
        int level = GetCardLevel(card);
        string message;

        switch (card.Suit)
        {
            case "Hearts":
                int healAmount = Math.Min(level, MaxHealth - Health);
                // Double healing if player chose Heart during character creation
                if (Suit == "HEARTS")
                {
                    healAmount = Math.Min(level * 2, MaxHealth - Health);
                }
                Health += healAmount;
                if (healAmount > 0)
                {
                    message = $"You found a good spot to rest and healed for {healAmount}.";
                }
                else
                {
                    message = $"You found a good spot to rest, but you're already fully healed.";
                }
                break;

            case "Diamonds":
                int treasureGain = level;
                // Double treasure if player chose Diamond during character creation
                if (Suit == "DIAMONDS")
                {
                    treasureGain = level * 2;
                }
                Treasure += treasureGain;
                message = $"You found a room with {treasureGain} treasure inside.";
                break;

            case "Clubs":
                Console.WriteLine("You found a room with a trap inside.");
                Console.WriteLine("Press any key to roll one D6 to avoid the trap.");
                Console.ReadKey(true);
                Random trapRoll = new Random();
                int roll = trapRoll.Next(1, 7);
                // Add +1 bonus if player chose Club during character creation
                int clubBonus = 0;
                if (Suit == "CLUBS")
                {
                    clubBonus = 1;
                }
                int finalClubRoll = roll + clubBonus;
                string clubBonusDisplay = "";
                if (clubBonus > 0)
                {
                    clubBonusDisplay = $" +{clubBonus} bonus = {finalClubRoll}";
                }
                Console.WriteLine($"You rolled a {roll}.{clubBonusDisplay}");
                if (finalClubRoll <= 4)
                {
                    Health -= level;
                    Health = Math.Max(Health, 0);
                    message = $"FAILURE: Took {level} damage.";
                }
                else
                {
                    message = $"SUCCESS: Took no damage.";
                }
                break;

            case "Spades":
                Console.WriteLine("You found a room with an enemy inside. Combat ensues.");
                Console.WriteLine("Press any key to roll one D6.");
                Console.ReadKey(true);
                Random dieRoll = new Random();
                int roll2 = dieRoll.Next(1, 7);
                // Add +1 bonus if player chose Spade during character creation
                int spadeBonus = 0;
                if (Suit == "SPADES")
                {
                    spadeBonus = 1;
                }
                int finalSpadeRoll = roll2 + spadeBonus;
                string spadeBonusDisplay = "";
                if (spadeBonus > 0)
                {
                    spadeBonusDisplay = $" +{spadeBonus} bonus = {finalSpadeRoll}";
                }
                Console.WriteLine($"You rolled a {roll2}.{spadeBonusDisplay}");
                if (finalSpadeRoll <= 4)
                {
                    Treasure -= level;
                    Treasure = Math.Max(Treasure, 0);
                    message = $"FAILURE: The enemy beat you up, ran your pockets, stole your Jordans, and cursed your bloodline. Lost {level} treasure.";
                }
                else
                {
                    message = $"You defeated the enemy.";
                }
                break;

            default:
                message = "something has gone terribly wrong if you're getting this message.";
                break;
        }

        return message;
    }
}

static partial class Program
{



    public static void Main()
    {
        Player p1 = new Player("???", 10, 0, "none", "none");
        Console.Clear();
        Console.WriteLine("Input your name.");
        p1.Name = Console.ReadLine();
        Console.WriteLine($"You are {p1.Name}");
        Console.WriteLine();
        bool validSuitChosen = false;
        do
        {
            Console.WriteLine("Choose a suit. S for SPADE, H for HEART, C for CLUB, D for DIAMOND");
            Console.WriteLine("Spades perform better in combat, adding +1 to dice rolls in Spade rooms.");
            Console.WriteLine("Hearts have improved healing, and heal for double in Heart rooms.");
            Console.WriteLine("Clubs avoid traps easier, adding +1 to dice rolls in Club rooms.");
            Console.WriteLine("Diamonds find more treasure, and gain double treasure in Diamond rooms.");
            Console.WriteLine();
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.S:
                    Console.WriteLine($"You chose SPADE");
                    p1.Suit = "SPADES";
                    validSuitChosen = true;
                    break;
                case ConsoleKey.H:
                    Console.WriteLine($"You chose HEART");
                    p1.Suit = "HEARTS";
                    validSuitChosen = true;
                    break;
                case ConsoleKey.C:
                    Console.WriteLine($"You chose CLUB");
                    p1.Suit = "CLUBS";
                    validSuitChosen = true;
                    break;
                case ConsoleKey.D:
                    Console.WriteLine($"You chose DIAMOND");
                    p1.Suit = "DIAMONDS";
                    validSuitChosen = true;
                    break;
                default:
                    Console.WriteLine("Invalid input. Please choose S, H, C, or D.");
                    Console.WriteLine();
                    break;
            }
        } while (!validSuitChosen);
        Console.WriteLine();
        Console.WriteLine("Finally, choose your Face Card and we can begin the Expedition.");
        Console.WriteLine("Press K for KING, Q for QUEEN, or J for JACK.");
        Console.WriteLine();
        bool validFaceChosen = false;
        do
        {
            ConsoleKey key2 = Console.ReadKey(true).Key;
            switch (key2)
            {
                case ConsoleKey.K:
                    Console.WriteLine($"You chose KING");
                    p1.Face = "KING";
                    validFaceChosen = true;
                    break;
                case ConsoleKey.Q:
                    Console.WriteLine($"You chose QUEEN");
                    p1.Face = "QUEEN";
                    validFaceChosen = true;
                    break;
                case ConsoleKey.J:
                    Console.WriteLine($"You chose JACK");
                    p1.Face = "JACK";
                    validFaceChosen = true;
                    break;
                default:
                    Console.WriteLine("Invalid input. Please choose K, Q, or J.");
                    Console.WriteLine();
                    break;
            }
        } while (!validFaceChosen);
        Console.Clear();
        Console.WriteLine($"Your name is {p1.Name}. You are the {p1.Face} of {p1.Suit}.");
        Console.WriteLine();

        Console.WriteLine("HOW THE GAME WORKS:");
        Console.WriteLine("In Expedition, you control a character moving through a series of floors in a dungeon.");
        Console.WriteLine("You progress through a floor by moving through rooms.");
        Console.WriteLine("There are 15 rooms per floor, separated into 3 directions, but you only need to complete two directions to move on to the next floor.");
        Console.WriteLine("You have two stats: HEALTH and TREASURE. You want both to be high.");
        Console.WriteLine("You lose Health in Trap Rooms (CLUB Cards) and gain it in Camp Rooms (HEARTS).");
        Console.WriteLine("You gain Treasure in Treasure Rooms (DIAMONDS) and lose it in Enemy Rooms (SPADES)");
        Console.WriteLine("Outcomes of Trap and Enemy rooms are dependent on virtual dice rolls.");
        Console.WriteLine("The game ends when you've explored three floors in total. Good luck.");

        Console.WriteLine();
        Console.WriteLine("Level One:");
        Console.WriteLine();

        Deck deck = new Deck();
        var groups = deck.DrawGroup();
        Random rand = new Random();
        int currentFloor = 1;

        if (p1.Health <= 0)
        {
            Console.Clear();
            Console.WriteLine("You died.");
        }

        bool continueGame = true;
        while (continueGame)
        {
            Console.WriteLine("Press any key to begin exploring...");
            Console.ReadKey(true);
            Console.Clear();

            do
            {
                Console.WriteLine("A path splits before you. Choose a direction to travel. (1, 2, or 3)");
                if (!int.TryParse(Console.ReadLine(), out int input))
                {
                    Console.WriteLine("Invalid input. Enter 1, 2, or 3.");
                    continue;
                }
                if (input < 1 || input > 3 && input != 9)
                {
                    Console.WriteLine("Invalid input. Enter 1, 2, or 3.");
                    continue;
                }
                // Adding this so I can see the cards drawn for dev reasons
                if (input == 9)
                {
                    Console.WriteLine("this is a secret dev thing. dont read these.");
                    foreach (var group in groups)
                    {
                        foreach (var card in group)
                        {
                            Console.WriteLine(card);
                        }
                    }
                    continue;
                }
                input--;
                if (groups[input].Count == 0)
                {
                    Console.WriteLine("You've hit a wall. Try going another direction.");
                    continue;
                }
                
                int index = rand.Next(groups[input].Count);
                Card drawn = groups[input][index];
                groups[input].RemoveAt(index);
                Console.Clear();
                Console.WriteLine($"You drew: {drawn}");
                string effectMessage = p1.ApplyRoomEffect(drawn);
                Console.WriteLine(effectMessage);
                Console.WriteLine($"Health: {p1.Health}/{p1.MaxHealth}, Treasure: {p1.Treasure}");
                Console.WriteLine();
                
                if (p1.Health <= 0)
                {
                    Console.Clear();
                    Console.WriteLine("You died. Your expedition ends here.");
                    Console.WriteLine($"You fell on Floor {currentFloor}.");
                    continueGame = false;
                    break;
                }
                
                Console.WriteLine("Cards remaining in groups:");
                for (int i = 0; i < 3; i++)
                {
                    Console.WriteLine($"Group {i + 1}: {groups[i].Count} cards");
                }
            } while (groups.Count(g => g.Count == 0) < 2 && continueGame);

            if (continueGame)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                Console.Clear();
                Console.WriteLine($"Floor {currentFloor} Complete!");
                Console.WriteLine();
                
                if (currentFloor >= 3)
                {
                    Console.WriteLine("You have survived the expedition.");
                    Console.WriteLine($"You made it out with {p1.Treasure} treasure.");
                    continueGame = false;
                }
                else
                {
                    Console.WriteLine("Shuffling the deck...");
                    deck.Shuffle();
                    groups = deck.DrawGroup();
                    currentFloor++;
                    Console.WriteLine($"Level {currentFloor}:");
                    Console.WriteLine();
                }
            }
        }



    }

}

