using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Hedgehog _hedgehog;

    public void SetData(Hedgehog hedgehog)
    {
        _hedgehog = hedgehog;

        nameText.text = hedgehog.Base.Name;
        levelText.text = "Lvl " + hedgehog.Level;
        hpBar.SetHP((float) hedgehog.HP / hedgehog.MaxHp); 
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float) _hedgehog.HP / _hedgehog.MaxHp); // since coroutine put in yield return
    }
}
