using System.Collections.Generic;
using UnityEngine;

public class Cardt
{
    public string Value { get; set; }
    public string Suit { get; set; }

    public Cardt(string value, string suit)
    {
        Value = value;
        Suit = suit;
    }
}

public class CardPairChecker : MonoBehaviour
{
    private static readonly Dictionary<string, int> CardValues = new Dictionary<string, int>
    {
        {"A", 1}, {"2", 2}, {"3", 3}, {"4", 4}, {"5", 5}, {"6", 6}, {"7", 7}, {"8", 8}, {"9", 9},
        {"10", 10}, {"J", 11}, {"Q", 12}, {"K", 13}
    };

    private static readonly Dictionary<string, string> PairMap = new Dictionary<string, string>
    {
        {"2", "8"}, {"8", "2"},
        {"A", "9"}, {"9", "A"},
        {"K", "4"}, {"4", "K"},
        {"6", "5"}, {"5", "6"}
    };

    public static bool IsPair(Cardt card1, Cardt card2)
    {
        // Check for sum of 10 for cards A to 9
        if (CardValues[card1.Value] <= 9 && CardValues[card2.Value] <= 9)
        {
            return CardValues[card1.Value] + CardValues[card2.Value] == 10;
        }

        // Check for same suit for cards 10, J, Q, K
        if (CardValues[card1.Value] >= 10 && CardValues[card2.Value] >= 10)
        {
            return card1.Suit == card2.Suit;
        }

        return false;
    }

    public static bool CheckWin(List<Cardt> hand)
    {
        int pairCount = 0;
        var usedCards = new HashSet<int>();

        for (int i = 0; i < hand.Count; i++)
        {
            if (usedCards.Contains(i)) continue;

            for (int j = i + 1; j < hand.Count; j++)
            {
                if (usedCards.Contains(j)) continue;

                if (IsPair(hand[i], hand[j]))
                {
                    pairCount++;
                    usedCards.Add(i);
                    usedCards.Add(j);
                    break;
                }
            }
        }

        return pairCount == 4;
    }

    public static List<Cardt> SortCards(List<Cardt> cards)
    {
        cards.Sort((card1, card2) =>
        {
            if (PairMap.ContainsKey(card1.Value) && PairMap[card1.Value] == card2.Value)
            {
                return -1; // card1 should come before card2
            }
            if (PairMap.ContainsKey(card2.Value) && PairMap[card2.Value] == card1.Value)
            {
                return 1; // card2 should come before card1
            }
            return 0; // no specific order
        });

        return cards;
    }

    public static List<Cardt> GetUnpairedCards(List<Cardt> hand)
    {
        var usedCards = new HashSet<int>();
        var unpairedCards = new List<Cardt>();

        for (int i = 0; i < hand.Count; i++)
        {
            if (usedCards.Contains(i)) continue;

            bool paired = false;
            for (int j = i + 1; j < hand.Count; j++)
            {
                if (usedCards.Contains(j)) continue;

                if (IsPair(hand[i], hand[j]))
                {
                    usedCards.Add(i);
                    usedCards.Add(j);
                    paired = true;
                    break;
                }
            }

            if (!paired)
            {
                unpairedCards.Add(hand[i]);
            }
        }

        return unpairedCards;
    }

    void Start()
    {
        var hand = new List<Cardt>
        {
            new Cardt("5", "Hearts"),
            new Cardt("5", "Diamonds"),
            new Cardt("Q", "Spades"),
            new Cardt("J", "Spades"),
            new Cardt("2", "Clubs"),
            new Cardt("8", "Clubs"),
            new Cardt("3", "Hearts"),
            new Cardt("7", "Hearts")
        };

        var sortedHand = SortCards(hand);

        foreach (var card in sortedHand)
        {
            Debug.Log($"{card.Value} of {card.Suit}");
        }

        Debug.Log(CheckWin(sortedHand) ? "Game Win!" : "Game Lose!");

        var unpairedCards = GetUnpairedCards(hand);
        Debug.Log("Unpaired Cards:");
        foreach (var card in unpairedCards)
        {
            Debug.Log($"{card.Value} of {card.Suit}");
        }
    }
}
