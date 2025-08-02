using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;

public class WorldController : MonoBehaviour {

    public NPCMovement wandererPrefab;
    public List<NPCMovement> availableWanderers;
    public GhostMovement ghostPrefab;

    public PlayerMovement player;

    public int numWanderers = 10;

    public float loopLength = 60 * 4;

    public List<GhostMovement> ghosts;
    public List<NPCMovement> wanderers;

    public bool generateRandomNPCs;

    private int numGhosts;

    private float startTime;
    private static int randomSeed = 123456789;

    public void SetupLoop(List<MovementKeyframe> record, List<ActionKeyframe> actionRecord) {
        GhostMovement ghost = (GhostMovement) Instantiate(ghostPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        ghost.buildings = this;
        ghost.ghostNum = numGhosts;
        ghost.record = new List<MovementKeyframe>(record);
        ghost.actionRecord = new List<ActionKeyframe>(actionRecord);
        ghost.StartReplay();
        ghosts.Add(ghost);
        for(int i = 0; i < numGhosts; i++) {
            ghosts[i].record = new List<MovementKeyframe>(record);
            ghosts[i].actionRecord = new List<ActionKeyframe>(actionRecord);
        }
        numGhosts++;

        float t = Time.fixedTime;
        for(int i = 0; i < numWanderers; i++) {
            wanderers[i].startTime = t;
            wanderers[i].gameObject.SetActive(true);
            wanderers[i].dead = false;
        }

    }

    public void ResetToLoop(int ghostNum) {
        int numLoop = numGhosts - ghostNum - 1;

        Debug.Log("Resetting to loop " + numLoop);

        float newTime = numLoop * loopLength;

        float timeTime = Time.fixedTime;
        startTime = timeTime - newTime;

        for(int i = numGhosts - 1; i >= numLoop; i--) {
            Destroy(ghosts[i].gameObject);
            ghosts.RemoveAt(i);
        }
        numGhosts = numLoop;
        for(int i = 0; i < numGhosts; i++) {
        //i think this is good? test thouroughly
            ghosts[i].startTime = startTime + (i + 1) * loopLength;
            ghosts[i].record = new List<MovementKeyframe>(player.record);
            ghosts[i].actionRecord = new List<ActionKeyframe>(player.actionRecord);
            ghosts[i].SetLastActionTime(newTime);
        }

        float t = Time.fixedTime;
        for(int i = 0; i < numWanderers; i++) {
            wanderers[i].startTime = t;
            wanderers[i].gameObject.SetActive(true);
            wanderers[i].dead = false;
        }

        //reset player pos
        player.SetToTime(newTime, startTime);
    }

    void Start() {
        if(generateRandomNPCs) {
            Random.InitState(randomSeed);

            wanderers = new List<NPCMovement>();
            ghosts = new List<GhostMovement>();

            for(int i = 0; i < numWanderers; i++) {
                NPCMovement prefab = availableWanderers[(int) (availableWanderers.Count * Random.value)];
                NPCMovement wanda = Instantiate(prefab).GetComponent<NPCMovement>();
                wanda.InitKeyframes(this.gameObject);
                if(i < 5) {
                    wanda.isTimeKeeper = true;
                }
                wanderers.Add(wanda);
            }
            startTime = Time.fixedTime;
        } else {
            numWanderers = wanderers.Count;
            for(int i = 0; i < numWanderers; i++) {
                wanderers[i].InitKeyframes(this.gameObject, true);
            }
        }
    }

    void Update() {
        if((Time.fixedTime - startTime) / loopLength > numGhosts + 1) {
            SetupLoop(player.record, player.actionRecord);
        }
    }
}
