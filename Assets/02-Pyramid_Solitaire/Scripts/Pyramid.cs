using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pyramid : MonoBehaviour
{
    static public Pyramid S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3f;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;

    [Header("Set Dynamically")]
    public Deck deck;
    public LayoutPyramid layout;
    public List<CardPyramid> stockCards;
    public Stack<CardPyramid> stock;
    public Stack<CardPyramid> waste;
    public List<CardPyramid> goal;
    public List<CardPyramid> tableau;
    public Transform layoutAnchor;

    void Awake()
    {
        S = this;
        stock = new Stack<CardPyramid>(24);
        waste = new Stack<CardPyramid>(24);
        goal = new List<CardPyramid>();
        tableau = new List<CardPyramid>();
    }


    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<LayoutPyramid>();
        layout.ReadLayout(layoutXML.text);

        stockCards = ConvertListCardsToListCardPyramid(deck.cards);

        LayoutGame();
    }

    List<CardPyramid> ConvertListCardsToListCardPyramid(List<Card> lCD)
    {
        List<CardPyramid> sCP = new List<CardPyramid>();
        CardPyramid tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardPyramid;
            sCP.Add(tCP);
        }
        return sCP;
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject go = new GameObject("LayoutAnchor");
            layoutAnchor = go.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardPyramid cp;

        foreach(PyrSlotDef tSD in layout.slotDefs)
        {
            cp = SetUpDraw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);

            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = pCardState.tableau;

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }

        foreach (CardPyramid tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        SetUpStock();
    }

    CardPyramid FindCardByLayoutID(int layoutID)
    {
        foreach(CardPyramid tCP in tableau)
        {
            if (tCP.layoutID == layoutID) return tCP;
        }
        return null;
    }

    CardPyramid SetUpDraw()
    {
        CardPyramid cd = stockCards[0];
        stockCards.RemoveAt(0);
        return cd;
    }

    CardPyramid Draw()
    {
        CardPyramid cd = stock.Pop();
        return cd;
    }

    void SetUpStock()
    {
        CardPyramid cd;

        for (int i = 0; i < stockCards.Count; i++)
        {
            cd = stockCards[i];
            cd.transform.parent = layoutAnchor;

            cd.transform.localPosition = new Vector3(
                layout.stock.x, layout.stock.y, 0);

            cd.faceUp = false;
            cd.state = pCardState.stock;
            cd.SetSortingLayerName(layout.stock.layerName);
            stock.Push(cd);
        }
    }
}
