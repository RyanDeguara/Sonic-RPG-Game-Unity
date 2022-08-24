using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;
    

    public void onPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject); // make sure portal isnt destroyed

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad); // Async is like a coroutine, waits until a scene is completely loaded
        
        // update position of the player of the new scene to the destination portal spawn point
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal); // returns all portals in current scene, returned first one, destination portal should be the one with the same destination identifier
        
        //player.transform.position = destPortal.SpawnPoint.position;
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
        
        Destroy(gameObject); // after its been loaded destroy it
    }

    public Transform SpawnPoint => spawnPoint; // property
}

public enum DestinIdentifier { A, B, C, D, E}
