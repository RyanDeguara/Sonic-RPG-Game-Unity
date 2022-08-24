using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFOV : MonoBehaviour, IPlayerTriggerable
{
    public void onPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OnEnterTraintersView(GetComponentInParent<TrainerController>()); // in parent becuse trainer controller script is not inside FOV but is parent to it
    }
}
