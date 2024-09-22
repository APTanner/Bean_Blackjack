using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static readonly Vector3 CAM_DEFAULT_POS = new Vector3(0, 1.58f, 0.4f);
    public static readonly Vector3 CAM_DEFAULT_ROT = new Vector3(-1.5f, 0, 0);

    public static readonly Vector3 CAM_BACKS_POS = new Vector3(0, 1.48f, 1.43f);
    public static readonly Vector3 CAM_BACKS_ROT = new Vector3(90f, 0, 0);

    public static readonly Vector3 CAM_PLAY_POS = new Vector3(0, 1.58f, 1.2f);
    public static readonly Vector3 CAM_PLAY_ROT = new Vector3(85.6f, 0, 0);

    public static readonly Vector3 CAM_RULES_POS = new Vector3(-2.82f, 1.58f, 0.4f);
    public static readonly Vector3 CAM_RULES_ROT = new Vector3(-1.5f, 86.677f, 0);

    public static readonly float MOVE_TRANSITION_TIME = 1f;
    public static readonly float FADE_TRANSITION_TIME = 0.2f;

    public static readonly float CARD_MOVE_TIME = 0.8f;
    public static readonly float CARD_ANIM_DELAY = 0.2f;

    public static readonly float ROUNDEL_POP_TIME = 0.5f;

    public static readonly Dictionary<string, Card.Type> VALID_CARD_TYPES = new()
    {
        { "2", Card.Type.TWO },
        { "3", Card.Type.THREE },
        { "4", Card.Type.FOUR },
        { "5", Card.Type.FIVE },
        { "6", Card.Type.SIX },
        { "7", Card.Type.SEVEN },
        { "8", Card.Type.EIGHT },
        { "9", Card.Type.NINE },
        { "10", Card.Type.TEN },
        { "jack", Card.Type.JACK },
        { "queen", Card.Type.QUEEN },
        { "king", Card.Type.KING },
        { "ace", Card.Type.ACE }
    };

    public static readonly Dictionary<string, Card.Suit> VALID_SUIT_TYPES = new()
    {
        { "clubs", Card.Suit.CLUBS },
        { "diamonds", Card.Suit.DIAMONDS },
        { "spades", Card.Suit.SPADES },
        { "hearts", Card.Suit.HEARTS }
    };

    public static int GetCardValue(PhysicalCard card)
    {
        if (!card.Flipped)
        {
            return 0;
        }

        switch (card.Card.type)
        {
            case Card.Type.TWO:
                return 2;
            case Card.Type.THREE:
                return 3;
            case Card.Type.FOUR:
                return 4;
            case Card.Type.FIVE:
                return 5;
            case Card.Type.SIX:
                return 6;
            case Card.Type.SEVEN:
                return 7;
            case Card.Type.EIGHT:
                return 8;
            case Card.Type.NINE:
                return 9;
            case Card.Type.TEN:
                return 10;
            case Card.Type.JACK:
                return 10;
            case Card.Type.QUEEN:
                return 10;
            case Card.Type.KING:
                return 10;
            case Card.Type.ACE:
                return 11;
        }
        return -1000;
    }
}
