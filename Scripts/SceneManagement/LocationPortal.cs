using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] DestinIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    PlayerController player;
    

    public void onPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);
        
        // update position of the player of the new scene to the destination portal spawn point
        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal); // returns all portals in current scene, returned first one, destination portal should be the one with the same destination identifier
        
        //player.transform.position = destPortal.SpawnPoint.position;
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);

    }

    public Transform SpawnPoint => spawnPoint; // property
}
