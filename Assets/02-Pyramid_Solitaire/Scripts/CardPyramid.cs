using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum pCardState
{
    stock,
    waste,
    wasteTop,
    goal,
    tableau
}

public class CardPyramid : Card
{
    [Header("Set in Inspector")]
    public GameObject selectIndicator;
    
    [Header("Set Dynamically: CardPyramid")]
    public pCardState state = pCardState.stock;
    public List<CardPyramid> hiddenBy = new List<CardPyramid>();
    public int layoutID;
    public PyrSlotDef slotDef;

    
    public bool _selected;
    public bool clickable;

    public bool selected
    {
        get
        {
            return _selected;
        }
        set
        {
            if (clickable)
            {
                _selected = value;
                selectIndicator.SetActive(_selected);
            }
        }
    }

    public override void OnMouseUpAsButton()
    {
        Pyramid.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
