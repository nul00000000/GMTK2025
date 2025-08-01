using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;

public class WorldController : MonoBehaviour {

    public NPCMovement wandererPrefab;
    public GhostMovement ghostPrefab;

    public PlayerMovement player;

    public int numWanderers = 10;

    public float loopLength = 60 * 4;

    private List<GhostMovement> ghosts;
    private List<NPCMovement> wanderers;

    private int numGhosts;

    private float startTime;

    public void SetupLoop(List<MovementKeyframe> record, List<ActionKeyframe> actionRecord) {
        GhostMovement ghost = (GhostMovement) Instantiate(ghostPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        ghost.record = new List<MovementKeyframe>(record);
        ghost.actionRecord = new List<ActionKeyframe>(actionRecord);
        ghost.StartReplay();
        ghosts.Add(ghost);
        for(int i = 0; i < numGhosts; i++) {
            ghosts[i].record = new List<MovementKeyframe>(record);
        }
        numGhosts++;

        float t = Time.fixedTime;
        for(int i = 0; i < numWanderers; i++) {
            wanderers[i].startTime = t;
            wanderers[i].gameObject.SetActive(true);
        }
    }

    void Start() {
        wanderers = new List<NPCMovement>();
        ghosts = new List<GhostMovement>();

        for(int i = 0; i < numWanderers; i++) {
            NPCMovement wanda = Instantiate(wandererPrefab).GetComponent<NPCMovement>();
            wanda.InitKeyframes(this.gameObject);
            wanderers.Add(wanda);
        }
        startTime = Time.fixedTime;
    }

    void Update() {
        if((Time.fixedTime - startTime) / loopLength > numGhosts + 1) {
            SetupLoop(player.record, player.actionRecord);
        }
    }
}
