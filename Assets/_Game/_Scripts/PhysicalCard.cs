using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PhysicalCard : MonoBehaviour
{
    public Card Card { get; private set; }

    public bool Flipped = false;

    private MeshRenderer m_mr;

    public PhysicalCard Initialize(Card card)
    {
        Card = card;

        m_mr = GetComponent<MeshRenderer>();
        Material mat = m_mr.materials[1];
        mat.mainTexture = Card.texture;
        // flip the textures vertically
        mat.mainTextureScale = new Vector2(-1, -1);

        return this;
    }
}
