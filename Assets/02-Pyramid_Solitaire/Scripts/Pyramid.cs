/**
 * To do list:
 * cards removed by tableau allow underlying cards to be selected
 * check for game over
 *      -empty tableau: win
 *      -no possible valid pairs among selectable: lose
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;

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
            wasteTop.clickable = false;
        }

        if (stock.Count != 0)
        {
            cd = stock.Pop();
            MoveCard(cd, layout.wasteTop, pCardState.wasteTop);
            wasteTop = cd;
            wasteTop.clickable = true;
        }
    }

    public void WasteToStock()
    {
        if (wasteTop != null)
        {
            MoveCard(wasteTop, layout.stock, pCardState.stock);
            stock.Push(wasteTop);
            wasteTop.clickable = false;
            wasteTop = null;
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
            sd.x, sd.y, sd.z);
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
                if (selected == null)
                {
                    if (cd.rank == 13)
                    {
                        MoveToGoal(cd);
                        wasteTop = waste.Pop();
                        MoveCard(wasteTop, layout.wasteTop, pCardState.wasteTop);
                        wasteTop.clickable = true;
                    }
                    else
                    {
                        SelectCard(cd);
                    }
                }
                else if (ValidPair(cd, selected))
                {
                    MoveToGoal(cd);
                    MoveToGoal(selected);
                    wasteTop = waste.Pop();
                    MoveCard(wasteTop, layout.wasteTop, pCardState.wasteTop);
                    wasteTop.clickable = true;
                    //allow underlying cards to be selected
                    UnselectCard();
                }
                else
                {
                    UnselectCard();
                }
                break;

            case pCardState.tableau:
                if (cd.clickable)
                {
                    if (selected == null)
                    {
                        if (cd.rank == 13)
                        {
                            MoveToGoal(cd);
                        }
                        else
                        {
                            SelectCard(cd);
                        }
                    }
                    else if (ValidPair(cd, selected))
                    {
                        if (selected.state == pCardState.wasteTop)
                        {
                            wasteTop = waste.Pop();
                            MoveCard(wasteTop, layout.wasteTop, pCardState.wasteTop);
                            wasteTop.clickable = true;
                        }
                        MoveToGoal(cd);
                        MoveToGoal(selected);
                        //allow underlying cards to be selected
                        UnselectCard();
                    }
                    else
                    {
                        UnselectCard();
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
        cd.selected = true;
        selected = cd;
    }

    public void UnselectCard()
    {
        selected.selected = false;
        selected = null;
    }

    public void MoveToGoal(CardPyramid cd)
    {
        cd.selected = false;
        cd.clickable = false;
        goal.Add(cd);
        MoveCard(cd, layout.goal, pCardState.goal);
        if (tableau.Contains(cd))
        {
            tableau.Remove(cd);
        }
        cd.SetSortOrder(-152 + 3*goal.Count);
    }

    public void CheckForGameOver()
    {

    }
}
