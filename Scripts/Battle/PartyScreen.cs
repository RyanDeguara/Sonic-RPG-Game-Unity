using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText; 
    
    PartyMemberUI[] memberSlots;
    List <Hedgehog> hedgehogs;
    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Hedgehog> hedgehogs)
    {
        this.hedgehogs = hedgehogs;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < hedgehogs.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(hedgehogs[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Choose a hedgehog";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < hedgehogs.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }


}
