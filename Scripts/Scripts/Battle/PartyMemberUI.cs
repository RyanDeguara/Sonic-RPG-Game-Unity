using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Color highlightedColor;
    Hedgehog _hedgehog;

    public void SetData(Hedgehog hedgehog)
    {
        _hedgehog = hedgehog;

        nameText.text = hedgehog.Base.Name;
        levelText.text = "Lvl " + hedgehog.Level;
        hpBar.SetHP((float) hedgehog.HP / hedgehog.MaxHp); 
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = highlightedColor;
        }
        else
        {
            nameText.color = Color.black;
        }
    }
}
