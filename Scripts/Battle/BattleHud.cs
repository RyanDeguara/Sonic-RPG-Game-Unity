using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    Hedgehog _hedgehog;

    public void SetData(Hedgehog hedgehog)
    {
        _hedgehog = hedgehog;

        nameText.text = hedgehog.Base.Name;
        SetLevel();
        hpBar.SetHP((float) hedgehog.HP / hedgehog.MaxHp);
        SetExp();
    }

    public void SetExp()
    {
        if (expBar == null) return;
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break; // yield break instead of return because its a coroutine
        
        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1 ,1);
        }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _hedgehog.Level;
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _hedgehog.Base.GetExpForLevel(_hedgehog.Level);
        int nextLevelExp = _hedgehog.Base.GetExpForLevel(_hedgehog.Level + 1);
        
        float normalizedExp = (float) (_hedgehog.Exp - currLevelExp) / (nextLevelExp - currLevelExp); //formula used to normalize exp
        return Mathf.Clamp01(normalizedExp); //make sure always between 0 and 1
    }

    public IEnumerator UpdateHP()
    {
        if (_hedgehog.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float) _hedgehog.HP / _hedgehog.MaxHp); // since coroutine put in yield return
            _hedgehog.HpChanged = false;
        }
    }
}
