using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pontoon
{
    class Card
    {
        // This identifies each card in relation 
        // to a whole deck
        private int cardReference;

        // Constructor for a card object needed 
        // during shuffle
        public Card()
        {
            cardReference = 0;
        }

        public Card(int n)
        {
            cardReference = n;
        }

        public int Rank()
        {
            // Use the full range of cardReference
            // to calculate an in-suit rank 1-13
            return 1 + cardReference % 13;
        }

        public String Suit()
        {
            string suitName = "";

            // Calculate suit based on groups of
            // 13 cards
            int suitValue = cardReference / 13;

            // Change the suitValue to an actual 
            // suit name
            switch (suitValue)
            {
                case 0:
                    suitName = "Hearts";
                    break;
                case 1:
                    suitName = "Clubs";
                    break;
                case 2:
                    suitName = "Diamonds";
                    break;
                case 3:
                    suitName = "Spades";
                    break;
            }

            return suitName;
        }
    }

    class Deck
    {
        // Declare deck of cards
        // (initialise in constructor)
        private Card[] deck;
        private int topCard;

        public Deck()
        {
            deck = new Card[52];
            topCard = 0;

            // Loop through deck
            for (int i = 0; i < 52; i++)
            {
                // For each Card object, initialise 
                // with the loop's count variable,
                // (note this calls the class's constructor)
                deck[i] = new Card(i);
            }
        }

        // Display card in a user-friendly way
        public void DisplayCard(int n)
        {
            String cardRankName;
            int rankValue;

            // Determine the name of the card for the 
            // benefit of the user when playing
            rankValue = deck[n].Rank();

            switch (rankValue)
            {
                case 1:
                    cardRankName = "Ace";
                    break;
                case 11:
                    cardRankName = "Jack";
                    break;
                case 12:
                    cardRankName = "Queen";
                    break;
                case 13:
                    cardRankName = "King";
                    break;
                default:
                    cardRankName = rankValue.ToString();
                    break;
            }

            Console.WriteLine(cardRankName + " of " + deck[n].Suit());
        }

        public void Shuffle()
        {
            Random rnd = new Random();
            Card swapCard = new Card();
            int swapRef;

            for (int i = 0; i < 52; i++)
            {
                // Generate and store a random number
                swapRef = rnd.Next(0, 52);

                // Swap cards in order to new random
                // postion
                swapCard = deck[i];
                deck[i] = deck[swapRef];
                deck[swapRef] = swapCard;
            }
        }

        public int GetCardValue(int cardNumber)
        {
            return deck[cardNumber].Rank();
        }

        public int DrawCard()
        {
            int drawCard = topCard;
            topCard += 1;

            // As a shuffle should only happen after
            // a Pontoon, the Deck should be reused -
            // wrap around from end back to start
            if (topCard >51)
            {
                topCard = 0;
            }

            return drawCard;
        }
    }

    class Player
    {
        private string playerName;
        private int chips;

        // Create player
        public Player(int n)
        {
            chips = 1000;

            Console.Write("Player " + (n + 1) + ", what is your name? ");
            playerName = Console.ReadLine();
        }

        public string Balance()
        {
            return "£" + chips;
        }

        public string Name()
        {
            return playerName;
        }

        public int CommitBet(int amountBet)
        {
            if (amountBet > chips || amountBet <= 0)
            {
                Console.WriteLine("You don't have enough chips.");
                amountBet = 0;
            }
            else
            {
                chips -= amountBet;
                Console.WriteLine("Balance is now: " + Balance());
            }

            // Makes sure the calling program knows how
            // much has actually been committed to bet
            // even if it is zero!
            return amountBet;
        }

        public bool HasFunds()
        {
            bool canBet = true;

            if (chips == 0)
            {
                canBet = false;
            }

            return canBet;
        }

        public void UpdateBalance(int amountWon)
        {
            chips += amountWon;
        }
    }

    class Hand
    {
        protected List<int> cardsFromDeck;

        public Hand()
        {
            cardsFromDeck = new List<int>();
        }

        public int HandValue(Deck deck)
        {
            int handValue = 0;
            int cardValue;
            bool aceUsed = false;

            foreach (int cardRef in cardsFromDeck)
            {
                cardValue = deck.GetCardValue(cardRef);
                
                // Court cards are limited to 10
                if (cardValue >= 10)
                {
                    handValue += 10;
                }
                // Ace used as 11 twice would make player bust
                // so use it as one
                else if (cardValue == 1 && aceUsed == true)
                {
                    handValue += 1;
                }
                // First time Ace drawn try to use it as 11
                else if (cardValue == 1 && aceUsed == false)
                {
                    handValue += 11;
                    aceUsed = true;
                }
                // All other cards count their face value
                else
                {
                    handValue += cardValue;
                }
            }

            // Try lowest value for an Ace if hand bust
            if (handValue > 21 && aceUsed == true)
            {
                handValue -= 10;
            }

            return handValue;
        }

        public void GetCard(Deck deck)
        {
            cardsFromDeck.Add(deck.DrawCard());
        }

        public void DisplayHand(Deck deck)
        {
            foreach (int cardRef in cardsFromDeck)
            {
                deck.DisplayCard(cardRef);
            }
        }

        public int CardsLeft()
        {
            return 5 - cardsFromDeck.Count();
        }
    }

    class PlayerHand : Hand
    {
        // Derived class needs betting and player information
        // along with protected attributes in parent class
        private Player holder;
        private int bet;

        public PlayerHand(Player player)
        {
            bet = 0;
            // Hand is an aggregate of itself 
            // and the player that owns it
            // (used to funding the bets made)
            holder = player;
        }

        public bool PlaceBet(int amount)
        {
            bool betPlaced = false;
            int betIncrease = holder.CommitBet(amount);

            // Only update bet if there were funds
            if (betIncrease > 0)
            {
                bet += betIncrease;
                Console.WriteLine("Current bet value: £" + bet);
                betPlaced = true;
            }

            // Calling program will take care of if 
            // they can't place bet
            return betPlaced;
        }

        public int Payout()
        {
            // Update player's balance with original bet 
            // and same again for winning
            holder.UpdateBalance(bet * 2);
            // Confirm amount won for display in calling program
            return bet;
        }

        public int PayDouble()
        {
            // Update player's balance with original bet 
            // and double for Pontoon
            holder.UpdateBalance(bet * 3);
            // Confirm amount won for display in calling program
            return bet * 2;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Player> players = new List<Player>();
            List<PlayerHand> hands = new List<PlayerHand>();
            Hand bankerHand = new Hand();
            int numberPlayers, betAmount;
            string drawAgain = "";

            Deck cardDeck = new Deck();
            // Shuffle the deck once created 
            // ready for the game
            cardDeck.Shuffle();

            // Setup players
            Console.Write("How many players will be playing? ");
            numberPlayers = Int32.Parse(Console.ReadLine());

            // Add the required number of players by name
            // and create a hand for them
            for (int i = 0; i < numberPlayers; i++)
            {
                players.Add(new Player(i));
                hands.Add(new PlayerHand(players[i]));
            }

            // Let each player complete their hand in full
            for (int currentPlayer = 0; currentPlayer < numberPlayers; currentPlayer++)
            {
                Console.WriteLine("----------------------------");

                // Can't join in round if player doesn't have chips
                if (players[currentPlayer].HasFunds())
                {
                    Console.WriteLine(players[currentPlayer].Name() + "'s first card...");
                    hands[currentPlayer].GetCard(cardDeck);
                    hands[currentPlayer].DisplayHand(cardDeck);
                    Console.WriteLine("The value of hand is: " + hands[currentPlayer].HandValue(cardDeck));

                    do
                    {
                        // Stop further play in hand if they run out of chips
                        if (players[currentPlayer].HasFunds())
                        {
                            // Keep looping if the bet cannot be funded (not enough chips)
                            do
                            {
                                Console.WriteLine("You have: " + players[currentPlayer].Balance());
                                Console.Write("How much do you want to bet? ");
                                betAmount = Int32.Parse(Console.ReadLine());
                            } while (!hands[currentPlayer].PlaceBet(betAmount));

                            Console.WriteLine(players[currentPlayer].Name() + " draws and hand is...");
                            hands[currentPlayer].GetCard(cardDeck);
                            hands[currentPlayer].DisplayHand(cardDeck);
                            Console.WriteLine("The value of hand is: " + hands[currentPlayer].HandValue(cardDeck));

                            // As long as the player hasn't drawn 5 cars, has funds 
                            // and the hand is less than 21 give them the choice
                            // to draw another card
                            if (hands[currentPlayer].CardsLeft() > 0 && hands[currentPlayer].HandValue(cardDeck) < 21 && players[currentPlayer].HasFunds())
                            {
                                Console.WriteLine("Do you want to draw another card? (y)");
                                drawAgain = Console.ReadLine().ToLower();
                            }
                        }
                        else
                        {
                            drawAgain = "n";
                        }

                    } while (drawAgain == "y" && hands[currentPlayer].CardsLeft() > 0 && hands[currentPlayer].HandValue(cardDeck) < 21);
                }
                else
                {
                    Console.WriteLine(players[currentPlayer].Name() + " is broke and can't play.");
                }
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Turn finished.");
            }

            // Banker's turn does not need to progress in the same
            // way as the players and sticks when 16+
            do
            {
                bankerHand.GetCard(cardDeck);
            } while (bankerHand.CardsLeft() > 0 && bankerHand.HandValue(cardDeck) < 16);

            Console.WriteLine("-----------------------------");
            Console.WriteLine("Banker plays.");
            bankerHand.DisplayHand(cardDeck);
            Console.WriteLine("The value of hand is: " + bankerHand.HandValue(cardDeck));
            Console.WriteLine("-----------------------------");

            // Banker Pontoon beats all other hands
            if (bankerHand.CardsLeft() == 2 && bankerHand.HandValue(cardDeck) == 21)
            {
                Console.WriteLine("Banker got Pontoon - no one wins!");
            }
            // otherwise check for other winning conditions
            else
            {
                for (int player = 0; player < numberPlayers; player++)
                {
                    // Player has Pontoon
                    if (hands[player].HandValue(cardDeck) == 21 && hands[player].CardsLeft() == 3)
                    {
                        Console.Write(players[player].Name() + " got Pontoon and won £");
                        Console.WriteLine(hands[player].PayDouble() + "!");
                    }
                    // Player has five card trick (and banker doesn't)
                    else if ((hands[player].CardsLeft() == 0 && hands[player].HandValue(cardDeck) <= 21) && !(bankerHand.CardsLeft() == 0 && bankerHand.HandValue(cardDeck) <= 21) )
                    {
                        Console.Write(players[player].Name() + " got five card trick and the banker didn't so won £");
                        Console.WriteLine(hands[player].PayDouble() + "!");
                    }
                    // Player hand out-scores banker's
                    else if ((hands[player].HandValue(cardDeck) > bankerHand.HandValue(cardDeck)) && hands[player].HandValue(cardDeck) <= 21)
                    {
                        Console.Write(players[player].Name() + " won £");
                        Console.WriteLine(hands[player].Payout() + "!");
                    }

                    // Update player balance
                    Console.WriteLine(players[player].Name() + " now has: " + players[player].Balance());
                    Console.WriteLine("-----------------------------");
                }
            }

            Console.Read();
        }
    }
}