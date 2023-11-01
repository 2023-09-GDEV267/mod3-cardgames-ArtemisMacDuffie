using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum pCardState
{
    stock,
    waste,
    goal,
    tableau
}

public class CardPyramid : Card
{
    [Header("Set Dynamically: CardPyramid")]
    public pCardState state = pCardState.stock;
    public List<CardPyramid> hiddenBy = new List<CardPyramid>();
    public int layoutID;
    public PyrSlotDef slotDef;

    public override void OnMouseUpAsButton()
    {
        //Pyramid.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
