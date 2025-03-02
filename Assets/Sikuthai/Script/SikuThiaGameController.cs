using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Sample;
//♠♣♦♥
public class SikuThiaGameController : MonoBehaviour
{
    
    public static SikuThiaGameController instance ;
    public GameObject cardPrefab;
    public List<Sprite> suitsSprite = new List<Sprite>();

    [Header("LOCATION")]
    public GameObject deckPlace;
    public GameObject playerHand;
    public GameObject aiHand;
    public GameObject dropPlace;
    public GameObject[] playerHandCardPos;
    public GameObject[] aiHandCardPos;
    public GameObject endPanel;

    [Header("TEXT")]
    public Text deckCardAmountText;
    public Text gameStateText;

    private string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };//{ "Hearts", "Diamonds", "Clubs", "Spades" };
    private string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };//{ "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private List<Card> deck = new List<Card>();

    [Header("PLAYER-HAND")]
    public List<Card> playerCards = new List<Card>();
    public Card playerCardDrop;

    [Header("AI-HAND")]
    public List<Card> aiCards = new List<Card>();
    public Card dropCard;

    [Header("BUTTON")]
    public Button dropButton;
    public Button drawnButton, eatButton,increasButton,discreasButton;

    [Header("COIN")]
    public int coin = 9999;
    public int flush = 1;
    public int maxFlush = 10;
    public int minFlush = 1;
   private int playerWinCount;
   private int aiWinCount;
    public Text coinText;
    public Text flushText,playerWinCountText,aiWinCoinText;
    bool isAiWin = false;

    private int round = 0;
    public float speed = 1f;

    private static readonly Dictionary<string, int> CardValues = new Dictionary<string, int>
    {
        {"A", 1}, {"2", 2}, {"3", 3}, {"4", 4}, {"5", 5}, {"6", 6}, {"7", 7}, {"8", 8}, {"9", 9},
        {"10", 10}, {"J", 11}, {"Q", 12}, {"K", 13}
    };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        
        coin = PlayerPrefs.GetInt("c", coin);
        coin = AuthInitialization.instance.user.coin;
        if(coin < 0)
        {
            coin = 10;
        }
        playerWinCount = PlayerPrefs.GetInt("p", playerWinCount);
        aiWinCount = PlayerPrefs.GetInt("a", aiWinCount);

        UpdateTextUi();
        
        //GoogleAdsMobManager.instance.interstitialAdController.ShowAd();
        
        yield return InterSecGame();
    }

    IEnumerator InterSecGame()
    {
        flushText.text = $"1FLUSH = {flush}";
        gameStateText.text = "you can change the flush before card deal in 3s";
        dropButton.interactable = drawnButton.interactable = eatButton.interactable = false;
        increasButton.interactable= discreasButton.interactable = true;
        GenerateDeck();
        yield return new WaitForSeconds(3f);
        ShuffleDeck();
        yield return DealCards();

        // Check for pairs after dealing
        CheckForPairs(playerCards, "Player");
        CheckForPairs(aiCards, "AI");

        CheckWin(playerCards, "Player");


        GetUnpairedCards(playerCards);
    }

    void Update() { }

    void UpdateTextUi()
    {
        coinText.text = $"coin = {coin}";

        playerWinCountText.text = $"PLAYER WIN COUNT= {playerWinCount}";
        aiWinCoinText.text = $"AI WIN COUNT = {aiWinCount}";
       
    }

    void GenerateDeck()
    {
        deck.Clear();
        int i = 0;
        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                GameObject newCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
                newCard.transform.SetParent(deckPlace.transform, false);

                Card card = newCard.GetComponent<Card>();
                newCard.name = rank + suit;
                card.name = rank + suit;
                card.suit = suit;
                card.rank = rank;

                Image suitImage = newCard.transform.Find("suit").GetComponent<Image>();
                Image suitImageR = newCard.transform.Find("left-suit").GetComponent<Image>();
                Image suitImageL = newCard.transform.Find("right-suit").GetComponent<Image>();
                Text rankText = newCard.transform.Find("rank").GetComponent<Text>();
                Text r = newCard.transform.Find("r").GetComponent<Text>();

                r.text = i.ToString();
                suitImage.sprite = suitImageR.sprite = suitImageL.sprite = GetElementSprite(suit);
                rankText.text = rank;
                rankText.color = GetRankColor(suit);

                deck.Add(card);

                // Create a random start position for the shuffle animation
                Vector3 randomPosition = new Vector3(
                    Random.Range(-300f, 300f),
                    Random.Range(-300f, 300f),
                    0);

               
                newCard.transform.localPosition = randomPosition;

               
                newCard.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));

              
                newCard.transform.localScale = Vector3.zero;

              
                LeanTween.scale(newCard, new Vector3(1f, 1f, 1f), 0.2f).setDelay(0.02f * i)  
                    .setEase(LeanTweenType.easeOutQuad); 

                LeanTween.moveLocal(newCard, Vector3.zero, 0.3f + 0.02f * i) 
                    .setDelay(0.02f * i)  
                    .setEase(LeanTweenType.easeInOutQuad);  

                LeanTween.rotateZ(newCard, 0, 0.3f + 0.02f * i)  
                    .setDelay(0.02f * i)
                    .setEase(LeanTweenType.easeInOutQuad);  

                i++;
            }
        }

        // Update the deck card amount text after the deck is generated
        deckCardAmountText.text = deck.Count.ToString();
    }



    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }


    IEnumerator DealCards()
    {
        gameStateText.text = "Dealing cards...";
        increasButton.interactable = discreasButton.interactable = false;
        playerCards.Clear();
        aiCards.Clear();
        int j = 0;
        int pPos = 0;

        for (int m = 0; m < 11; m++)
        {
            if (m % 2 == 0)
            {
                // Player's turn to get a card
                GameObject playerCard = deck[m].gameObject;
                playerCard.transform.SetParent(playerHand.transform, true);

                // Animate card movement, scaling, and rotation
                AudioController.Instance.PlaySFX("deal");
                LeanTween.moveLocal(playerCard,
                    new Vector3(
                        playerHandCardPos[pPos].transform.localPosition.x,
                        playerHandCardPos[pPos].transform.localPosition.y,
                        playerHandCardPos[pPos].transform.localPosition.z),
                    speed)
                    .setFrom(new Vector3(
                        playerCard.transform.localPosition.x,
                        playerCard.transform.localPosition.y,
                        0f))
                    .setEase(LeanTweenType.easeInOutExpo)
                    .setOnComplete(() =>
                    {
                        LeanTween.rotateZ(playerCard, Random.Range(-10f, 10f), 0.2f).setEase(LeanTweenType.easeOutQuad); // Rotate slightly for realism
                    LeanTween.scale(playerCard, new Vector3(1.1f, 1.1f, 1f), 0.2f).setEase(LeanTweenType.easeOutBack) // Slight scale up
                            .setOnComplete(() =>
                            {
                                LeanTween.scale(playerCard, Vector3.one, 0.1f).setEase(LeanTweenType.easeInQuad); // Return to normal size

                            // Reset the card rotation back to 0 degrees
                            LeanTween.rotateZ(playerCard, 0, 0.2f).setEase(LeanTweenType.easeInOutQuad); // Smooth rotation back to 0
                        });
                    });

                playerCards.Add(deck[m]);

                yield return new WaitForSeconds(speed);
                pPos++;
            }
            else
            {
                // AI's turn to get a card
                GameObject aiCard = deck[m].gameObject;
                aiCard.transform.SetParent(aiHand.transform, true);
                AudioController.Instance.PlaySFX("deal");
                LeanTween.moveLocal(aiCard,
                    new Vector3(
                        aiHandCardPos[j].transform.localPosition.x,
                        aiHandCardPos[j].transform.localPosition.y,
                        aiHandCardPos[j].transform.localPosition.z),
                    speed)
                    .setFrom(new Vector3(
                        aiCard.transform.localPosition.x,
                        aiCard.transform.localPosition.y,
                        0f))
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() =>
                    {
                        LeanTween.rotateZ(aiCard, Random.Range(-10f, 10f), 0.2f).setEase(LeanTweenType.easeOutQuad); // Random rotation for realism
                    LeanTween.scale(aiCard, new Vector3(1.1f, 1.1f, 1f), 0.2f).setEase(LeanTweenType.easeOutBack) // Slight scale up
                            .setOnComplete(() =>
                            {
                                LeanTween.scale(aiCard, Vector3.one, 0.1f).setEase(LeanTweenType.easeInQuad); // Return to normal size

                            // Reset the card rotation back to 0 degrees
                            LeanTween.rotateZ(aiCard, 0, 0.2f).setEase(LeanTweenType.easeInOutQuad); // Smooth rotation back to 0
                        });
                    });

                aiCards.Add(deck[m]);

                deckCardAmountText.text = deck.Count.ToString();
                yield return new WaitForSeconds(speed);
                j++;
            }
           
        }

        // Remove the dealt cards from the deck
        deck.RemoveRange(0, 15);

        // After all cards have been dealt, flip the player cards
        for (int k = 0; k < playerCards.Count; k++)
        {
            yield return FlipCard(playerCards[k].gameObject, false);
            AudioController.Instance.PlaySFX("opencard");// Flip the card face up
            yield return new WaitForSeconds(0.1f);  // Small delay between flips
        }
        PlayerCardShortPosition(playerCards);
        dropButton.interactable = true;
        deckCardAmountText.text = deck.Count.ToString();
        gameStateText.text = "...";
    }



    public List<Card> GetUnpairedCards(List<Card> hand)
    {
        var usedCards = new HashSet<int>();
        var unpairedCards = new List<Card>();

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

        unpairedCards.ForEach(c =>
        {
            c.gameObject.GetComponent<Image>().color = Color.gray;
        });

        return unpairedCards;
    }

    public void SelectCard(Card c)
    {
        playerCards.ForEach(card =>
        {
            if (card.name != c.name)
            {
                card.s = false;
                LeanTween.moveLocalY(card.gameObject, 0, 0.2f).setEase(LeanTweenType.easeInQuad);  // Move down smoothly
                LeanTween.scale(card.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeInQuad);  // Scale back to original size
                LeanTween.rotateZ(card.gameObject, 0, 0.2f).setEase(LeanTweenType.easeInOutQuad);  // Reset rotation to 0
            }
        });
    }

    public void Drop()
    {
        if (playerCardDrop != null)
        {

            dropButton.interactable  = false;
            PlayerDropTheCard(playerCardDrop);
            StartCoroutine(AIAction(dropCard));
        }
        else
        {
            gameStateText.text = "select card to drop";
        }
      
    }

    public void PlayerDropTheCard(Card dropCard)
    {
        GameObject cardDrop = dropCard.gameObject;
        AudioController.Instance.PlaySFX("drop");
        cardDrop.transform.SetParent(dropPlace.transform, true);

        cardDrop.gameObject.GetComponent<Image>().color = Color.white;
        LeanTween.moveLocalY(cardDrop.gameObject, 0, 0.2f).setEase(LeanTweenType.easeInQuad);  // Move down smoothly
        LeanTween.scale(cardDrop.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeInQuad);  // Scale back to original size
        LeanTween.rotateZ(cardDrop.gameObject, 0, 0.2f).setEase(LeanTweenType.easeInOutQuad);
        // Reset rotation to 0
        LeanTween.moveLocal(cardDrop,
                    //new Vector3(
                    //    dropPlace.transform.localPosition.x,
                    //    dropPlace.transform.localPosition.y * 0,
                    //    dropPlace.transform.localPosition.z)
                    Vector3.zero
                    , 0.3f)
                    .setFrom(new Vector3(
                        cardDrop.transform.localPosition.x,
                        cardDrop.transform.localPosition.y,
                        0f))
                    .setEase(LeanTweenType.easeInOutExpo);
                  


        playerCards.Remove(dropCard);
     
        this.dropCard = dropCard;
        playerCardDrop = null;
        CheckWin(playerCards, "Player");
        GetUnpairedCards(playerCards);
        PlayerCardShortPosition(playerCards);
    }



    //public void  PlayerDrawnCard()
    //{
    //    dropButton.interactable = true;
    //    drawnButton.interactable = eatButton.interactable = false;
    //    Card drawnCard = deck[0];
    //    playerCards.Add(drawnCard);
    //    drawnCard.transform.SetParent(playerHand.transform, true);
    //    LeanTween.moveLocal(drawnCard.gameObject,
    //                new Vector3(
    //                    playerHandCardPos[7].transform.localPosition.x,
    //                    playerHandCardPos[7].transform.localPosition.y * 0,
    //                    playerHandCardPos[7].transform.localPosition.z)
    //                , 0.5f).setFrom(new Vector3(
    //                    drawnCard.transform.localPosition.x,
    //                    drawnCard.transform.localPosition.y,
    //                    0f))
    //                .setEase(LeanTweenType.easeInOutQuad)
    //                .setOnComplete(() =>
    //                {

    //                    deck.Remove(drawnCard);
    //                    deckCardAmountText.text = deck.Count.ToString();
    //                    if (!CheckWin(playerCards, "Player"))
    //                    {
    //                        if (deck.Count <= 0)
    //                        {
    //                            StartCoroutine(OutOfDeck());
    //                        }
    //                    };
    //                });
    //    StartCoroutine( FlipCard(drawnCard.gameObject,false));



    //    print($"drawn - {playerCards.Count}");
    //    List<Card> unPairCard = GetUnpairedCards(playerCards);

    //    unPairCard.ForEach(c => { print("drawn unpair - "+c.name); });

    //}

    public void PlayerDrawnCard()
    {
        dropButton.interactable = true;
        AudioController.Instance.PlaySFX("drawn");
        drawnButton.interactable = eatButton.interactable = false;
        Card drawnCard = deck[0];
        playerCards.Add(drawnCard);
        drawnCard.transform.SetParent(playerHand.transform, true);

        // Scale up and move the card with a slight rotation, then reset everything to default
        LeanTween.moveLocal(drawnCard.gameObject,
                    new Vector3(
                        playerHandCardPos[5].transform.localPosition.x,
                        playerHandCardPos[5].transform.localPosition.y * 0,
                        playerHandCardPos[5].transform.localPosition.z),
                    0.5f)
            .setFrom(new Vector3(
                        drawnCard.transform.localPosition.x,
                        drawnCard.transform.localPosition.y,
                        0f))
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(drawnCard.gameObject, new Vector3(1.1f, 1.1f, 1f), 0.2f).setEase(LeanTweenType.easeOutQuad)
                    .setOnComplete(() =>
                    {
                        LeanTween.scale(drawnCard.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeInQuad); // Return to original scale
                    LeanTween.rotateZ(drawnCard.gameObject, 0, 0.2f).setEase(LeanTweenType.easeInOutQuad); // Reset rotation to 0

                    // After animation, remove card from deck and update UI
                    deck.Remove(drawnCard);
                        deckCardAmountText.text = deck.Count.ToString();

                        if (!CheckWin(playerCards, "Player"))
                        {
                            //if (deck.Count <= 0)
                            //{
                            //    StartCoroutine(OutOfDeck());
                            //}
                        };
                    });
            });

        // Add a slight random rotation during the draw
        LeanTween.rotateZ(drawnCard.gameObject, Random.Range(-10f, 10f), 0.5f).setEase(LeanTweenType.easeOutBack);

        // Flip the card after it moves into position
        StartCoroutine(FlipCard(drawnCard.gameObject, false));

        print($"drawn - {playerCards.Count}");
        List<Card> unPairCard = GetUnpairedCards(playerCards);

        unPairCard.ForEach(c => { print("drawn unpair - " + c.name); });
    }


    public void PlayerEatCardButton() {

        if(dropCard != null) {
            AudioController.Instance.PlaySFX("eat");
            dropButton.interactable = true;
            drawnButton.interactable = eatButton.interactable = false;

            Card eatCart = dropCard;

            playerCards.Add(eatCart);
            eatCart.transform.SetParent(playerHand.transform, true);
            LeanTween.moveLocal(eatCart.gameObject,
                        new Vector3(
                            playerHandCardPos[5].transform.localPosition.x,
                            playerHandCardPos[5].transform.localPosition.y * 0,
                            playerHandCardPos[5].transform.localPosition.z)
                        , 0.5f).setFrom(new Vector3(
                            eatCart.transform.localPosition.x,
                            eatCart.transform.localPosition.y,
                            0f))
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {

                            //deck.Remove(drawnCard);
                            //deckCardAmountText.text = deck.Count.ToString();
                        });
            CheckWin(playerCards, "Player");
        }
        GetUnpairedCards(playerCards);
    }


    void PlayerCardShortPosition(List<Card> hand)
    {
       
        var usedCards = new HashSet<int>();
        var unpairedCards = new List<Card>();
        var newPlayerCard = new List<Card>();

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
                    newPlayerCard.Add(hand[i]);
                    newPlayerCard.Add(hand[j]); ;
                    paired = true;
                    break;
                }
            }

            if (!paired)
            {
                unpairedCards.Add(hand[i]);
            }
        }

        unpairedCards.ForEach(c =>
        {
            c.gameObject.GetComponent<Image>().color = Color.gray;
            newPlayerCard.Add(c);
        });

        newPlayerCard.ForEach(c =>
        {
            print($"new - [{c.name}]");
        });
        // ggggg
       
        for (int i = 0; i < newPlayerCard.Count; i++)
        {
            LeanTween.moveLocal(newPlayerCard[i].gameObject,
                   new Vector3(
                       playerHandCardPos[i].transform.localPosition.x,
                       playerHandCardPos[i].transform.localPosition.y * 0,
                       playerHandCardPos[i].transform.localPosition.z)
                   , 0.5f).setFrom(new Vector3(
                       deckPlace.transform.localPosition.x,
                       -playerHandCardPos[i].transform.localPosition.y,
                       0f))
                   .setEase(LeanTweenType.easeInOutQuad);
            newPlayerCard[i].gameObject.transform.SetSiblingIndex(i);

        }
        newPlayerCard.Clear();
    }



    void SortCard()
    {
        playerCards.ForEach(c => { Destroy(c.gameObject); });
    }


    // ###############################   AI



    IEnumerator AIAction(Card droppedCard)
    {
        gameStateText.text = "AI TURN";
        if (deck.Count <= 0)
        {
            //PlayAgai();
            
           
            yield return OutOfDeck();

        }
        else
        {

            yield return new WaitForSeconds(1f);  // Small delay for realism


            // Check if AI can pair with the dropped card
            bool pairFound = false;
            foreach (Card card in GetUnpairedCards(aiCards))
            {
                if (IsPair(card, droppedCard))
                {
                    AiEatCard();
                    gameStateText.text = "AI eats the dropped card.";
                    pairFound = true;
                    break;
                }
            }

            yield return new WaitForSeconds(1f);
            // If no pair is found, AI draws from the deck
            if (!pairFound)
            {
                AiDrawnCard();
                deckCardAmountText.text = deck.Count.ToString();
                gameStateText.text = "AI draws from the deck.";
            }

            // Now AI has 8 cards, it needs to drop one card
            yield return new WaitForSeconds(1f);

            if (isAiWin)
            {
                print(aiWinCount);
            }
            else
            {
                List<Card> upairCard = GetUnpairedCards(aiCards);
                AiDropTheCard(upairCard[Random.Range(0, upairCard.Count)]);
            }

        }
        
    }

    IEnumerator OutOfDeck() {
        yield return new WaitForSeconds(0.5f);
        gameStateText.text = "Drawn!";
        for (int v = 0; v < aiCards.Count; v++)
        {
            yield return FlipCard(aiCards[v].gameObject, false);

        }
        yield return new WaitForSeconds(2f);
        endPanel.SetActive(true);
        gameStateText.text = "Restarting game ..";
        yield return ResetGameState();
    }



    public void AiEatCard()
    {

        if (dropCard != null)
        {
            Card eatCart = dropCard;
            AudioController.Instance.PlaySFX("eat");
            aiCards.Add(eatCart);
            eatCart.transform.SetParent(aiHand.transform, true);
            LeanTween.moveLocal(eatCart.gameObject,
                        new Vector3(
                            aiHandCardPos[5].transform.localPosition.x,
                            aiHandCardPos[5].transform.localPosition.y * 0,
                            aiHandCardPos[5].transform.localPosition.z)
                        , 0.5f).setFrom(new Vector3(
                            eatCart.transform.localPosition.x,
                            eatCart.transform.localPosition.y,
                            0f))
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {

                            //deck.Remove(drawnCard);
                            //deckCardAmountText.text = deck.Count.ToString();
                        });
            StartCoroutine(FlipCard(eatCart.gameObject, true));
            CheckWin(aiCards, "AI");
        }

    }

    public void AiDropTheCard(Card dropCard)
    {

        //dropButton.interactable = true;
        AudioController.Instance.PlaySFX("drop");
        drawnButton.interactable = eatButton.interactable = true;
        GameObject cardDrop = dropCard.gameObject;

        cardDrop.gameObject.GetComponent<Image>().color = Color.white;

        cardDrop.transform.SetParent(dropPlace.transform, true);
        LeanTween.moveLocal(cardDrop,
                  
                    Vector3.zero
                    , 0.3f)
                    .setFrom(new Vector3(
                        cardDrop.transform.localPosition.x,
                        cardDrop.transform.localPosition.y,
                        0f))
                    .setEase(LeanTweenType.easeInOutExpo);



        aiCards.Remove(dropCard);
        StartCoroutine(FlipCard(cardDrop.gameObject,false));
        this.dropCard = dropCard;
        AiCardShortPosition(aiCards);
        gameStateText.text = "YOUR TURN !";
    }

    Card AIDropCard()
    {
        // AI drops the first card that doesn't form a pair
        foreach (Card card in aiCards)
        {
            bool foundPair = false;
            foreach (Card otherCard in aiCards)
            {
                if (card != otherCard && IsPair(card, otherCard))
                {
                    foundPair = true;
                    break;
                }
            }

            // If no pair is found, drop this card
            if (!foundPair)
            {
                return card;
            }
        }

        // If all cards are paired, just drop the first card
        return aiCards[0];
    }

    //public void AiDrawnCard()
    //{
    //    Card drawnCard = deck[0];
    //    aiCards.Add(drawnCard);
    //    drawnCard.transform.SetParent(aiHand.transform, true);
    //    LeanTween.moveLocal(drawnCard.gameObject,
    //                new Vector3(
    //                    aiHandCardPos[7].transform.localPosition.x,
    //                    aiHandCardPos[7].transform.localPosition.y * 0,
    //                    aiHandCardPos[7].transform.localPosition.z)
    //                , 0.5f).setFrom(new Vector3(
    //                    drawnCard.transform.localPosition.x,
    //                    drawnCard.transform.localPosition.y,
    //                    0f))
    //                .setEase(LeanTweenType.easeInOutQuad)
    //                .setOnComplete(() =>
    //                {

    //                    deck.Remove(drawnCard);
    //                    deckCardAmountText.text = deck.Count.ToString();
    //                    if (!CheckWin(aiCards, "AI"))
    //                    {
    //                        if (deck.Count <= 0)
    //                        {
    //                            StartCoroutine(OutOfDeck());
    //                        }
    //                    };
    //                });


    //}

    public void AiDrawnCard()
    {
        AudioController.Instance.PlaySFX("drawn");
        Card drawnCard = deck[0];
        aiCards.Add(drawnCard);
        drawnCard.transform.SetParent(aiHand.transform, true);

        // Move, scale, and rotate animation for the AI's drawn card
        LeanTween.moveLocal(drawnCard.gameObject,
                    new Vector3(
                        aiHandCardPos[5].transform.localPosition.x,
                        aiHandCardPos[5].transform.localPosition.y * 0,
                        aiHandCardPos[5].transform.localPosition.z),
                    0.5f)
            .setFrom(new Vector3(
                        drawnCard.transform.localPosition.x,
                        drawnCard.transform.localPosition.y,
                        0f))
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(drawnCard.gameObject, new Vector3(1.1f, 1.1f, 1f), 0.2f).setEase(LeanTweenType.easeOutQuad)
                    .setOnComplete(() =>
                    {
                        LeanTween.scale(drawnCard.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeInQuad); // Return to original scale
                    LeanTween.rotateZ(drawnCard.gameObject, 0, 0.2f).setEase(LeanTweenType.easeInOutQuad); // Reset rotation to 0

                    // After animation, remove card from deck and update UI
                    deck.Remove(drawnCard);
                        deckCardAmountText.text = deck.Count.ToString();

                        if (!CheckWin(aiCards, "AI"))
                        {
                            
                            if (deck.Count <= 0)
                            {
                                StartCoroutine(OutOfDeck());
                            }
                        }
                       
                    });
            });

        // Add a slight random rotation during the draw
        LeanTween.rotateZ(drawnCard.gameObject, Random.Range(-10f, 10f), 0.5f).setEase(LeanTweenType.easeOutBack);
    }


    void AiCardShortPosition(List<Card> hand)
    {
        var usedCards = new HashSet<int>();
        var unpairedCards = new List<Card>();
        var newPlayerCard = new List<Card>();

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
                    newPlayerCard.Add(hand[i]);
                    newPlayerCard.Add(hand[j]); ;
                    paired = true;
                    break;
                }
            }

            if (!paired)
            {
                unpairedCards.Add(hand[i]);
            }
        }

        unpairedCards.ForEach(c =>
        {
            c.gameObject.GetComponent<Image>().color = Color.gray;
            newPlayerCard.Add(c);
        });

        newPlayerCard.ForEach(c =>
        {
            print($"new - [{c.name}]");
        });
        for (int i = 0; i < newPlayerCard.Count; i++)
        {
            LeanTween.moveLocal(newPlayerCard[i].gameObject,
                   new Vector3(
                       aiHandCardPos[i].transform.localPosition.x,
                       aiHandCardPos[i].transform.localPosition.y * 0,
                       aiHandCardPos[i].transform.localPosition.z)
                   , 0.5f).setFrom(new Vector3(
                       deckPlace.transform.localPosition.x,
                       -aiHandCardPos[i].transform.localPosition.y,
                       0f))
                   .setEase(LeanTweenType.easeInOutQuad);
            newPlayerCard[i].gameObject.transform.SetSiblingIndex(i);

        }
       
        newPlayerCard.Clear();
    }




    // ############################### Condition

    IEnumerator FlipCard(GameObject card,bool isFlip)
    {
        LeanTween.scaleX(card, 0f, 0.3f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.2f);

        card.transform.Find("back").gameObject.SetActive(isFlip);

        LeanTween.scaleX(card, 1f, 0.3f).setEase(LeanTweenType.easeInOutQuad);
    }

    
    void CheckForPairs(List<Card> hand, string playerName)
    {
        List<Card> checkedCards = new List<Card>();

        for (int i = 0; i < hand.Count; i++)
        {
            for (int j = i + 1; j < hand.Count; j++)
            {
                Card card1 = hand[i];
                Card card2 = hand[j];

                if (IsPair(card1, card2))
                {
                    //Debug.Log($"{playerName} found a pair: {card1.rank} and {card2.rank}");
                    checkedCards.Add(card1);
                    checkedCards.Add(card2);
                }
            }
        }

        if (checkedCards.Count == 0)
        {
           // Debug.Log($"{playerName} found no pairs.");
        }
    }



    bool IsPair(Card card1, Card card2)
    {
        // Check for identical 10, J, Q, K cards
        //if ((card1.rank == "10" || card1.rank == "J" || card1.rank == "Q" || card1.rank == "K") &&
        //    card1.rank == card2.rank)
        //{
        //    return true;
        //}

        if(card1.rank == card2.rank)
        {
            return true;
        }

        // Check for sum of 10 for A to 9
        //if (GetCardValue(card1.rank) + GetCardValue(card2.rank) == 10)
        //{
        //    return true;
        //}

        return false;
    }

    public bool CheckWin(List<Card> hand, string who)
    {
        int pairCount = 0;
        var usedCards = new HashSet<int>();

        //int allRed = 0;
        //int allBlack = 0;
        //hand.ForEach(c =>
        //{

        //    if (c.transform.GetChild(0).GetComponent<Text>().color == Color.red)
        //    {
        //        allRed++;
        //    }
        //    else
        //    {
        //        allBlack++;
        //    }
        //});

        for (int i = 0; i < hand.Count; i++)
        {
            if (usedCards.Contains(i)) continue;

            for (int j = i + 1; j < hand.Count; j++)
            {
                if (usedCards.Contains(j)) continue;

                if (IsPair(hand[i], hand[j]))
                {
                    pairCount++;
                    print($"{pairCount} -{who}- [{hand[i].name}][{hand[j].name}]");
                    hand[i].gameObject.GetComponent<Image>().color = Color.white;
                    hand[j].gameObject.GetComponent<Image>().color = Color.white;
                    usedCards.Add(i);
                    usedCards.Add(j);
                    break;
                }
            }
        }
        print($"pairt = { pairCount}");
        if (pairCount == 3)
        {
            for (int v = 0; v < aiCards.Count; v++)
            {
                StartCoroutine(FlipCard(aiCards[v].gameObject, false));

            }
            int flush = 0;
            switch (who.ToUpper())
            {
                case "PLAYER":
                    playerWinCount++;
                    playerCards.ForEach(c =>
                    {

                        if (c.transform.GetChild(0).GetComponent<Text>().color == Color.red)
                        {
                            flush++;
                        }
                    });
                    if (flush >= 8)
                    {
                        coin += this.flush * 10;
                        gameStateText.text = who + $" win {10} flush \n FullRed you win {this.flush * 10}";
                    }
                    else if (flush == 0)
                    {
                        coin += this.flush * 10;
                        gameStateText.text = who + $" win {10} flush \n FullBlack you win {this.flush * 10}";
                    }
                    else
                    {
                        coin += this.flush * flush;
                        gameStateText.text = who + $" win {flush} flush \n you win {this.flush * flush}";
                    }

                    AudioController.Instance.PlaySFX("win");
                    break;
                case "AI":
                    aiWinCount++;
                    isAiWin = true;
                    AudioController.Instance.PlaySFX("lose");
                    aiCards.ForEach(c =>
                    {

                        if (c.transform.GetChild(0).GetComponent<Text>().color == Color.red)
                        {
                            flush++;
                        }
                    });
                    if (flush >= 8)
                    {

                        if (coin < this.flush * 10)
                        {
                            coin = 0;
                        }
                        else
                        {
                            coin -= this.flush * 10;
                        }
                        gameStateText.text = who + $" win {10} flush \n FullRed you lose {this.flush * 10}";
                    }
                    else if (flush == 0)
                    {

                        if (coin < this.flush * 10)
                        {
                            coin = 0;
                        }
                        else
                        {
                            coin -= this.flush * 10;
                        }
                        gameStateText.text = who + $" win {10} flush \n FullBlack you lose {this.flush * 10}";
                    }
                    else
                    {
                        gameStateText.text = who + $" win {flush} flush \n you lose -{this.flush * flush}";
                        if (coin < this.flush * flush)
                        {
                            coin = 0;
                        }
                        else
                        {
                            coin -= this.flush * flush;
                        }
                    }


                    break;
            }
            AuthInitialization.instance.cloudSaveManager.SaveCoins(coin);
            PlayerPrefs.SetInt("c", coin);
            PlayerPrefs.SetInt("p", playerWinCount);
            PlayerPrefs.SetInt("a", aiWinCount);
            UpdateTextUi();
            StartCoroutine(ResetGameState());
            //gameStateText.text = who + "win";
        }

        //if(allBlack == 8)
        //{
        //    return true;
        //}
        //else if(allRed == 8)
        //{
        //    return true;
        //}
        //else
        //{
        return pairCount == 4;
        //}

    }






    IEnumerator ResetGameState()
    {
      
        yield return new WaitForSeconds(3f);
        gameStateText.text = "new round in 3s . . .";
        endPanel.SetActive(true);
        for (int i = 0; i < dropPlace.transform.childCount; i++)
        {
            Destroy(dropPlace.transform.GetChild(i).gameObject);
        }
        playerCards.ForEach(c => { Destroy(c.gameObject); });
        aiCards.ForEach(c => { Destroy(c.gameObject); });

        for (int i = 0; i < deckPlace.transform.childCount; i++)
        {
            Destroy(deckPlace.transform.GetChild(i).gameObject);
        }

        playerCardDrop = dropCard =null;
       
        yield return new WaitForSeconds(3f);
        endPanel.SetActive(false);
        isAiWin = false;
       yield return new WaitForSeconds(1f);
       //GoogleAdsMobManager.instance.rewardedInterstitialAdController.ShowAd();
        yield return InterSecGame();
    }



















    int GetCardValue(string rank)
    {
        switch (rank)
        {
            case "A": return 1;
            case "K": return 13;
            case "Q": return 12;
            case "J": return 11;
            case "10": return 10;
            case "9": return 9;
            case "8": return 8;
            case "7": return 7;
            case "6": return 6;
            case "5": return 5;
            case "4": return 4;
            case "3": return 3;
            case "2": return 2;
            default: return 0;
        }
    }

    Sprite GetElementSprite(string suit)
    {
        switch (suit)
        {
            case "Hearts": return suitsSprite[0];
            case "Diamonds": return suitsSprite[1];
            case "Clubs": return suitsSprite[2];
            case "Spades": return suitsSprite[3];
            default: return null;
        }
    }

    Color GetRankColor(string suit)
    {
        switch (suit)
        {
            case "Hearts":
            case "Diamonds":
                return Color.red;
            case "Clubs":
            case "Spades":
                return Color.black;
            default:
                return Color.white;
        }
    }

    public void PlayAgai()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void ChangeFlush(string name)
    {
        switch (name)
        {
            case "+":
                flush += 1;
                if (flush > maxFlush)
                {
                    flush = minFlush;
                }



                break;
            default:
                flush -= 1;
                if (flush < minFlush)
                {
                    flush = maxFlush;
                }




                break;
        }
        flushText.text = $"1FLUSH = {flush}";
    }
}
