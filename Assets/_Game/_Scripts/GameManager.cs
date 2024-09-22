using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private CardManager m_cardManager;
    [SerializeField] private PhysicalCard m_cardPrefab;

    [Header("Positions")]
    public Transform CardSpawnPoint;
    public Transform PlayerCardPos;
    public Vector3 PlayerCardOffset;
    public Transform DealerCardPos;
    public Vector3 DealerCardOffset;

    [Header("Flipping settings")]
    public float FlipTime;
    public float FlipHeight;

    [Header("Buttons")]
    public Button StandButton;
    public Button HitButton;
    public Button DealButton;
    public Button RestartButton;

    [Header("Hand Values")]
    public RectTransform PlayerValueRoundel;
    public RectTransform DealerValueRoundel;

    [Header("Win Banner Positions")]
    public RectTransform PlayerBannerPos;
    public RectTransform DealerBannerPos;

    [Header("Statuses")]
    public RectTransform WinBanner;
    public RectTransform LoseBanner;
    public RectTransform TieBanner;
    public RectTransform BustBanner;

    [Header("Statuses Rendering Parent")]
    public GameObject GameUIParent;

    private TextMeshProUGUI m_playerValueText;
    private TextMeshProUGUI m_dealerValueText;

    private List<PhysicalCard> m_playerHand = new();
    private List<PhysicalCard> m_dealerHand = new();

    private Score m_playerScore;
    private Score m_dealerScore;

    public int PlayerHandSize => m_playerHand.Count;
    public int DealerHandSize => m_dealerHand.Count;

    public bool PlayerAt21 => m_playerScore.Value == 21;


    private NewGameState m_newGameState;
    private PlayingState m_playingState;
    private DealerState m_dealerState;
    private EndState m_endState;

    private GameState m_currentState;

    public bool IsInGame => m_currentState != m_newGameState;

    private WinType m_winType;

    public WinType WinType => m_winType;

    public void GoToPlayState() => ChangeState(m_playingState);
    public void GoToDealerState() => ChangeState(m_dealerState);

    public void StartNewGame() => ChangeState(m_newGameState);

    public bool ShouldDealerHit() {
        Debug.Log($"Dealer Score is: {m_dealerScore.Value}");
        return m_dealerScore.Value < 17;
    }

    private void ChangeState(GameState newState)
    {
        m_currentState?.ExitState();
        m_currentState = newState;
        newState.EnterState();
    }

    private void Awake()
    {
        m_newGameState = new(this);
        m_playingState = new(this);
        m_dealerState  = new(this);
        m_endState     = new(this);

        InitValueText();
    }

    private void Start()
    {
        StartNewGame();
    }

    private void InitValueText()
    {
        m_playerValueText = PlayerValueRoundel.GetComponentInChildren<TextMeshProUGUI>();
        m_dealerValueText = DealerValueRoundel.GetComponentInChildren<TextMeshProUGUI>();
        m_playerValueText.text = m_dealerValueText.text = "0";

        Vector3 smooshed = new Vector3(0, 1, 1);
        PlayerValueRoundel.localScale = smooshed;
        DealerValueRoundel.localScale = smooshed;
    }

    public enum BannerType
    {
        WIN,
        LOSE,
        TIE,
        BUST
    }

    public RectTransform MakePlayerBanner(BannerType type) => MakeBanner(PlayerBannerPos, type);

    public RectTransform MakeDealerBanner(BannerType type) => MakeBanner(DealerBannerPos, type);

    private RectTransform MakeBanner(RectTransform rt, BannerType type)
    {
        RectTransform banner = BustBanner;
        switch (type)
        {
            case BannerType.WIN:
                banner = WinBanner;
                break;
            case BannerType.LOSE:
                banner = LoseBanner;
                break;
            case BannerType.TIE:
                banner = TieBanner;
                break;
            case BannerType.BUST:
                banner = BustBanner;
                break;
            default:
                break;
        }
        return Instantiate(banner, rt.position, Quaternion.identity, GameUIParent.transform);
    }

    public void DestroyBanner(RectTransform banner)
    {
        CardAnimations.PopOut(banner, () => Destroy(banner.gameObject));
    }

    public IEnumerator RecallCards()
    {
        StartCoroutine(RecallHand(m_dealerHand));
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(RecallHand(m_playerHand));
    }

    private IEnumerator RecallHand(List<PhysicalCard> hand)
    {
        foreach (PhysicalCard card in hand)
        {
            RecallCard(card);
            yield return new WaitForSeconds(0.2f);
        }
        
    }

    private void RecallCard(PhysicalCard card)
    {
        CardAnimations.MoveCard(card, card.transform.position, CardSpawnPoint.position, c => Destroy(c.gameObject));
    }

    public void ResetGame()
    {
        m_playerScore = new();
        m_dealerScore = new();
        m_playerHand.Clear();
        m_dealerHand.Clear();
        m_winType = null;
        m_cardManager.Shuffle();
        m_playerValueText.text = m_dealerValueText.text = "0";
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void UpdateScore(PhysicalCard card)
    {
        bool bPlayerCard = m_playerHand.Contains(card);

        if (bPlayerCard) {
            UpdateHandValue(ref m_playerScore, m_playerHand);
        }
        else
        {
            UpdateHandValue(ref m_dealerScore, m_dealerHand);
        }

        TextMeshProUGUI scoreText = bPlayerCard ? m_playerValueText : m_dealerValueText;
        int currentScore = bPlayerCard ? m_playerScore.Value : m_dealerScore.Value;

        if (scoreText.text == "0")
        {
            RectTransform rt = bPlayerCard ? PlayerValueRoundel : DealerValueRoundel;
            CardAnimations.PopIn(rt);
        }

        scoreText.text = currentScore.ToString();
    }

    public void ResetRoundels()
    {
        CardAnimations.PopOut(PlayerValueRoundel);
        CardAnimations.PopOut(DealerValueRoundel);
    }

    private void UpdateHandValue(ref Score score, List<PhysicalCard> hand)
    {
        score.Value = hand.Sum(x => Globals.GetCardValue(x));

        int aceCount = hand.Count(c => c.Card.type == Card.Type.ACE);

        // Check for blackjack
        score.BlackJack = hand.Count == 2 && score.Value == 21;

        while (score.Value > 21 && aceCount > 0)
        {
            score.Value -= 10; // Adjust for Aces
            aceCount--;
        }

        CheckBust();
    }

    public void CheckBust()
    {
        bool bPlayerBust = m_playerScore.Busted;
        bool bDealerBust = m_dealerScore.Busted;
        if (!bPlayerBust && !bDealerBust)
        {
            return;
        }

        m_winType = new WinType(
            bPlayerBust ? WinType.Winner.DEALER : WinType.Winner.PLAYER, 
            WinType.WinCondition.BUST);
        ChangeState(m_endState);
    }

    public void CheckWin()
    {
        // if there is already a win don't do anything
        if (m_winType != null)
        {
            return;
        }

        WinType.WinCondition condition = WinType.WinCondition.VALUE;

        bool bPlayerWin = m_playerScore > m_dealerScore;
        bool bDealerWin = m_dealerScore > m_playerScore;

        m_winType = new WinType(
            bPlayerWin ? WinType.Winner.PLAYER : 
            (bDealerWin ? WinType.Winner.DEALER : WinType.Winner.TIE),
            condition);
        ChangeState(m_endState);
    }

    public PhysicalCard GetLastPlayerCard() => m_playerHand[m_playerHand.Count - 1];

    public PhysicalCard GetLastDealerCard() => m_dealerHand[m_dealerHand.Count - 1];

    public PhysicalCard DealPlayer() => Deal(m_playerHand);

    public PhysicalCard DealDealer() => Deal(m_dealerHand);

    private PhysicalCard Deal(List<PhysicalCard> hand) 
    {
        hand.Add(MakeCard());
        return hand[hand.Count - 1];
    }

    private PhysicalCard MakeCard()
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 180);
        return Instantiate(m_cardPrefab, CardSpawnPoint.position, rotation).Initialize(m_cardManager.DrawCard());
    }
}


public struct Score : IComparable<Score>
{
    public bool BlackJack;
    public int Value;

    public bool Busted => Value > 21;

    public int CompareTo(Score other)
    {
        int valueComparison = Value.CompareTo(other.Value);
        if (valueComparison != 0)
        {
            return valueComparison;
        }

        return BlackJack == other.BlackJack ? 0 : (BlackJack ? 1 : -1);
    }

    public override bool Equals(object obj)
    {
        return obj is Score score &&
               BlackJack == score.BlackJack &&
               Value == score.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BlackJack, Value);
    }

    // Overload '==' operator
    public static bool operator ==(Score c1, Score c2)
    {
        return c1.CompareTo(c2) == 0;
    }

    // Overload '!=' operator
    public static bool operator !=(Score c1, Score c2)
    {
        return c1.CompareTo(c2) != 0;
    }

    // Overload '>' operator
    public static bool operator >(Score c1, Score c2)
    {
        return c1.CompareTo(c2) > 0;
    }

    // Overload '<' operator
    public static bool operator <(Score c1, Score c2)
    {
        return c1.CompareTo(c2) < 0;
    }

    // Overload '>=' operator
    public static bool operator >=(Score c1, Score c2)
    {
        return c1.CompareTo(c2) >= 0;
    }

    // Overload '<=' operator
    public static bool operator <=(Score c1, Score c2)
    {
        return c1.CompareTo(c2) <= 0;
    }
}

public class WinType
{
    public readonly Winner WhoWon;
    public readonly WinCondition Condition;

    public WinType(Winner whoWon, WinCondition condition)
    {
        WhoWon = whoWon;
        Condition = condition;
    }

    public enum Winner
    {
        PLAYER,
        DEALER,
        TIE
    }

    public enum WinCondition
    {
        BUST,
        VALUE
    }
}
