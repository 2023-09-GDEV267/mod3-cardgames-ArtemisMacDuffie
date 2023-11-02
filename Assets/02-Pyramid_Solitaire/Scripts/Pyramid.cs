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

    [SerializeField]
    public Stack<CardPyramid> stock;

    [SerializeField]
    public Stack<CardPyramid> waste;

    public List<CardPyramid> goal;
    public List<CardPyramid> tableau;
    public Transform layoutAnchor;
    public CardPyramid wasteTop;
    public CardPyramid selected;

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
            cp.clickable = true;
            cp.selected = false;
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
                tCP.clickable = false;
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

    public void Draw()
    {
        CardPyramid cd;
        
        if (wasteTop != null)
        {
            MoveCard(wasteTop, layout.waste, pCardState.waste);
            waste.Push(wasteTop);
        }

        if (stock.Count != 0)
        {
            cd = stock.Pop();
            MoveCard(cd, layout.wasteTop, pCardState.wasteTop);
            wasteTop = cd;
        }
    }

    public void WasteToStock()
    {
        if (wasteTop != null)
        {
            MoveCard(wasteTop, layout.stock, pCardState.stock);
            stock.Push(wasteTop);
        }

        CardPyramid cd;

        while (waste.Count != 0)
        {
            cd = waste.Pop();
            MoveCard(cd, layout.stock, pCardState.stock);
            stock.Push(cd);
        }
    }

    public void MoveCard(CardPyramid cd, PyrSlotDef sd, pCardState state)
    {
        cd.state = state;
        cd.transform.localPosition = new Vector3(
            sd.x, sd.y, 0);
        cd.faceUp = sd.faceUp;
        cd.SetSortingLayerName(sd.layerName);
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

    public void CardClicked(CardPyramid cd)
    {
        switch (cd.state)
        {
            case pCardState.goal:
            case pCardState.waste:
                break;

            case pCardState.stock:
                Draw();
                break;

            case pCardState.wasteTop:
                if (cd.rank == 13)
                {
                    MoveCard(cd, layout.goal, pCardState.goal);
                    goal.Add(cd);
                }
                //***
                break;

            case pCardState.tableau:
                if (cd.clickable)
                {
                    if (selected == null)
                    {
                        if (cd.rank == 13)
                        {
                            MoveCard(cd, layout.goal, pCardState.goal);
                            goal.Add(cd);
                        }
                        else
                        {
                            selected = cd;
                        }
                    }
                    else if (ValidPair(cd, selected))
                    {
                        //***
                    }
                    else
                    {
                        selected = null;
                    }
                }
                break;
        }

        //CheckForGameOver();
    }

    public bool ValidPair(CardPyramid cd1,  CardPyramid cd2)
    {
        if (cd1.rank + cd2.rank == 13)
            return true;

        return false;
    }

    public void SelectCard(CardPyramid cd)
    {

    }
}
