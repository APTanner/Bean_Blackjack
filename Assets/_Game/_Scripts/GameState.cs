using System.Collections;
using UnityEngine;

public abstract class GameState
{
    protected GameManager m_manager;

    public GameState(GameManager manager)
    {
        m_manager = manager;
    }

    public abstract void EnterState();
    public abstract void ExitState();
}

public class NewGameState : GameState
{
    public NewGameState(GameManager manager) : base(manager) { }
    public override void EnterState()
    {
        m_manager.StandButton.interactable = false;
        m_manager.HitButton.interactable = false;
        m_manager.DealButton.interactable = true;

        m_manager.ResetGame();
        m_manager.DealButton.onClick.AddListener(OnDeal);

        Debug.Log("Started New Game");
    }

    public override void ExitState()
    {
        m_manager.DealButton.onClick.RemoveListener(OnDeal);
    }

    private void OnDeal()
    {
        Debug.Log("Dealing");
        m_manager.DealButton.interactable = false;
        m_manager.RunCoroutine(DealPlayer());
    }

    private IEnumerator DealPlayer()
    {
        Debug.Log("Dealing");
        Vector3 spawnPos = m_manager.CardSpawnPoint.position;
        Vector3 outputPos = m_manager.PlayerCardPos.position;
        Vector3 offset = m_manager.PlayerCardOffset;

        PhysicalCard card1 = m_manager.DealPlayer();
        CardAnimations.FlipAndMoveCard(
            card1,
            spawnPos,
            outputPos,
            flippedCard => m_manager.UpdateScore(flippedCard));

        yield return new WaitForSeconds(Globals.CARD_ANIM_DELAY);

        PhysicalCard card2 = m_manager.DealPlayer();
        CardAnimations.FlipAndMoveCard(
            card2,
            spawnPos,
            outputPos + offset,
            flippedCard => {
                m_manager.UpdateScore(flippedCard);
                m_manager.RunCoroutine(DealDealer());
            });
    }

    private IEnumerator DealDealer()
    {
        Vector3 spawnPos = m_manager.CardSpawnPoint.position;
        Vector3 outputPos = m_manager.DealerCardPos.position;
        Vector3 offset = m_manager.DealerCardOffset;

        PhysicalCard card1 = m_manager.DealDealer();
        CardAnimations.FlipAndMoveCard(
            card1,
            spawnPos,
            outputPos,
            flippedCard => m_manager.UpdateScore(flippedCard));

        yield return new WaitForSeconds(Globals.CARD_ANIM_DELAY);

        PhysicalCard card2 = m_manager.DealDealer();
        CardAnimations.MoveCard(
            card2,
            spawnPos,
            outputPos + offset,
            _ => {
                if (m_manager.PlayerAt21)
                {
                    m_manager.GoToDealerState();
                }
                else m_manager.GoToPlayState();
            }
        );
    }
}

public class PlayingState : GameState
{
    bool m_bGameOver;

    public PlayingState(GameManager manager) : base(manager) { }

    public override void EnterState()
    {
        m_manager.StandButton.interactable = true;
        m_manager.HitButton.interactable = true;
        m_manager.DealButton.interactable = false;

        m_manager.HitButton.onClick.AddListener(OnHit);
        m_manager.StandButton.onClick.AddListener(OnStand);

        m_bGameOver = false;

        Debug.Log("Entered Game");
    }

    private void DisableButtons()
    {
        m_manager.StandButton.interactable = false;
        m_manager.HitButton.interactable = false;
    }

    private void EnableButtons()
    {
        m_manager.StandButton.interactable = true;
        m_manager.HitButton.interactable = true;
    }

    private void OnStand()
    {
        m_manager.GoToDealerState();
    }

    private void OnHit()
    {
        Debug.Log("Player hitting");
        Vector3 spawnPos = m_manager.CardSpawnPoint.position;
        Vector3 outputPos = m_manager.PlayerCardPos.position;
        Vector3 offset = m_manager.PlayerCardOffset;

        DisableButtons();
        PhysicalCard card = m_manager.DealPlayer();
        CardAnimations.FlipAndMoveCard(
            card,
            spawnPos,
            outputPos + offset * (m_manager.PlayerHandSize-1),
            flippedCard => {
                m_manager.UpdateScore(flippedCard);
                if (m_manager.PlayerAt21) OnStand();
                if (!m_bGameOver) EnableButtons();
            });
    }

    public override void ExitState()
    {
        m_manager.HitButton.onClick.RemoveListener(OnHit);
        m_manager.StandButton.onClick.RemoveListener(OnStand);
        m_bGameOver = true;
    }
}

public class DealerState : GameState
{
    private bool m_bGameOver;

    public DealerState(GameManager manager) : base(manager) { }

    public override void EnterState()
    {
        m_manager.StandButton.interactable = false;
        m_manager.HitButton.interactable = false;
        m_manager.DealButton.interactable = false;

        m_bGameOver = false;

        Debug.Log("Entered Dealer Game");

        DealerFlip();
    }

    private void DealerFlip()
    {
        CardAnimations.FlipCard(
            m_manager.GetLastDealerCard(),
            flippedCard => {
                m_manager.UpdateScore(flippedCard);
                CheckPlayerBlackjack();
                DealerHit();
            });
    }

    private void CheckPlayerBlackjack()
    {
        if (m_manager.PlayerAt21 && m_manager.PlayerHandSize == 2)
        {
            m_manager.CheckWin();
        }
    }

    private void DealerHit()
    {
        Vector3 spawnPos = m_manager.CardSpawnPoint.position;
        Vector3 outputPos = m_manager.DealerCardPos.position;
        Vector3 offset = m_manager.DealerCardOffset;

        if (m_manager.ShouldDealerHit() && !m_bGameOver)
        {
            Debug.Log("Dealer Hitting");
            PhysicalCard card = m_manager.DealDealer();
            CardAnimations.FlipAndMoveCard(
                card,
                spawnPos,
                outputPos + offset * (m_manager.DealerHandSize - 1),
                flippedCard => {
                    m_manager.UpdateScore(flippedCard);
                    DealerHit();
                });
            return;
        }

        m_manager.CheckWin();
    }

    public override void ExitState()
    {
        m_bGameOver = true;
    }
}

public class EndState : GameState
{
    private RectTransform m_restartRectTransform;
    private RectTransform m_playerBanner;
    private RectTransform m_dealerBanner;

    public EndState(GameManager manager) : base(manager) 
    {
        m_restartRectTransform = m_manager.RestartButton.gameObject.GetComponent<RectTransform>();
    }

    public override void EnterState()
    {
        m_manager.StandButton.interactable = false;
        m_manager.HitButton.interactable = false;
        m_manager.DealButton.interactable = false;

        Debug.Log("Enter Game Over");

        WinType winType = m_manager.WinType;
        if (winType == null)
        {
            Debug.LogError("Something wrong with code flow -- winType should be set when entering the end state");
        }

        Debug.Log($"{winType.WhoWon.ToString()} by {winType.Condition.ToString()}");

        if (winType.Condition == WinType.WinCondition.BUST)
        {
            if (winType.WhoWon == WinType.Winner.PLAYER)
            {
                DealerBusted();
            }
            else
            {
                PlayerBusted();
            }
        }
        else if (winType.WhoWon == WinType.Winner.TIE)
        {
            Tie();
        }
        else
        {
            WinByValue(winType.WhoWon == WinType.Winner.PLAYER);
        }
    }

    private void PlayerBusted()
    {
        m_playerBanner = m_manager.MakePlayerBanner(GameManager.BannerType.BUST);
        CardAnimations.BannerPopIn(m_playerBanner, () => DealerWinBanner());
    }

    private void DealerBusted()
    {
        m_dealerBanner = m_manager.MakeDealerBanner(GameManager.BannerType.BUST);
        CardAnimations.BannerPopIn(m_dealerBanner, () => PlayerWinBanner());
    }

    private void Tie()
    {
        m_dealerBanner = m_manager.MakeDealerBanner(GameManager.BannerType.TIE);
        m_playerBanner = m_manager.MakePlayerBanner(GameManager.BannerType.TIE);
        CardAnimations.BannerPopIn(m_dealerBanner);
        CardAnimations.BannerPopIn(m_playerBanner, () => ShowRestart());
    }

    private void WinByValue(bool bPlayerWon)
    {
        GameManager.BannerType win = GameManager.BannerType.WIN;
        GameManager.BannerType lose = GameManager.BannerType.LOSE;

        m_dealerBanner = m_manager.MakeDealerBanner(bPlayerWon ? lose : win);
        m_playerBanner = m_manager.MakePlayerBanner(bPlayerWon ? win : lose);
        CardAnimations.BannerPopIn(m_dealerBanner);
        CardAnimations.BannerPopIn(m_playerBanner, () => ShowRestart());
    }
    private void PlayerWinBanner()
    {
        m_playerBanner = m_manager.MakePlayerBanner(GameManager.BannerType.WIN);
        CardAnimations.BannerPopIn(m_playerBanner, () => ShowRestart());
    }

    private void DealerWinBanner()
    {
        m_dealerBanner = m_manager.MakeDealerBanner(GameManager.BannerType.WIN);
        CardAnimations.BannerPopIn(m_dealerBanner, () => ShowRestart());
    }

    private void ShowRestart()
    {
        m_manager.RestartButton.gameObject.SetActive(true);
        m_manager.RestartButton.onClick.AddListener(Restart);
        CardAnimations.BannerPopIn(m_restartRectTransform);
    }

    private void Restart()
    {
        m_manager.RunCoroutine(DoRestart());
    }

    private IEnumerator DoRestart()
    {
        m_manager.DestroyBanner(m_playerBanner);
        yield return new WaitForSeconds(0.25f);
        m_manager.DestroyBanner(m_dealerBanner);
        yield return new WaitForSeconds(0.25f);

        m_manager.ResetRoundels();
        CardAnimations.PopOut(m_restartRectTransform);

        yield return new WaitForSeconds(0.25f);
        m_manager.RunCoroutine(m_manager.RecallCards());

        // This is really janky but I don't have a callback for all the cards getting back into
        // the deck. I'll just wait for a bit and assume it's taken care of
        yield return new WaitForSeconds(2f);
        m_manager.StartNewGame();
    }

    public override void ExitState()
    {
        m_manager.RestartButton.onClick.RemoveListener(Restart);
        m_manager.RestartButton.gameObject.SetActive(false);
    }
}

