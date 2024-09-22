using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    public Suit suit;
    public Type type;
    public Texture texture;

    public Card(Suit _suit, Type _type, Texture _texture)
    {
        suit = _suit;
        type = _type;
        texture = _texture;
    }

    public enum Suit
    {
        CLUBS,
        HEARTS,
        DIAMONDS,
        SPADES
    }

    public enum Type
    {
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING,
        ACE
    }
}


