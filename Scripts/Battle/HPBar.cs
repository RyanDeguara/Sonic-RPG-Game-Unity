using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    // coroutine - decrease hp bar smoothly
    public IEnumerator SetHPSmooth(float newHp)
    {
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newHp;
        // loop that will run until difference between curhp and newhp is a small value
        while (curHp - newHp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime; // reduce by small amount
            health.transform.localScale = new Vector3(curHp, 1f); // set curHp as scale of the healthbar in the ui
            yield return null; // after reducing hp by small amount stop the coroutine and continue in the next frame
        }
        health.transform.localScale = new Vector3(newHp, 1f);

    }
}
