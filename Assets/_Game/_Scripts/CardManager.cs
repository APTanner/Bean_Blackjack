using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] private GameObject m_deckObject;
    [SerializeField] private Texture[] m_cardBacks;
    [SerializeField] private CardData m_cardData;

    private int m_cardBackIdx = 0;

    private Material m_cardBack;

    private Card[] m_cards;
    private int m_cardsInDeck;

    private void Awake()
    {
        if (m_deckObject == null)
        {
            Debug.LogError("CardManager needs a card");
        }

        if (m_cardBacks.Length == 0)
        {
            Debug.LogError("There needs to be at least one card back");
        }

        MeshRenderer mr = m_deckObject.GetComponent<MeshRenderer>();
        m_cardBack = mr.sharedMaterials[0];
        m_cardBack.mainTexture = m_cardBacks[m_cardBackIdx];

        m_cards = m_cardData.Cards;
        m_cardsInDeck = m_cards.Length;
    }

    public void SwitchCardBacks(bool increment)
    {
        m_cardBackIdx = (m_cardBackIdx + (increment ? 1 : -1) + m_cardBacks.Length) % m_cardBacks.Length;
        m_cardBack.mainTexture = m_cardBacks[m_cardBackIdx];
    }

    public Card DrawCard()
    {
        int idx = Random.Range(0, m_cardsInDeck--);
        Card res = m_cards[idx];
        // swap the newly drawn card to the end of the deck array so it 
        // isn't randomly chosen again
        m_cards[idx] = m_cards[m_cardsInDeck];
        m_cards[m_cardsInDeck] = res;

        return res;
    }

    public void Shuffle()
    {
        m_cardsInDeck = m_cards.Length;
    }

#if UNITY_EDITOR
    [ContextMenu("Load Card Backs")]
    public void LoadCardBacks()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Game/Textures/Cards" });
        List<Card> cards = new List<Card>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            string name = texture.name;
            string[] parts = name.Split('_');
            string valueString = parts[0];
            string suitString = parts.Length > 2 ? parts[2] : null;

            if (Globals.VALID_CARD_TYPES.TryGetValue(valueString, out var type) && 
                Globals.VALID_SUIT_TYPES.TryGetValue(suitString, out var suit))
            {
                Card newCard = new(suit, type, texture);
                cards.Add(newCard);
            }
        }

        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.Cards = cards.ToArray();

        string assetPath = "Assets/CardData.asset";
        AssetDatabase.CreateAsset(cardData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif // UNITY_EDITOR
}
