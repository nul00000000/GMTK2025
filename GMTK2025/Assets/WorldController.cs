using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using TimeThings;

public class WorldController : MonoBehaviour {

    public NPCMovement wandererPrefab;
    public List<NPCMovement> availableWanderers;
    public GhostMovement ghostPrefab;

    public FPScript compass;
    public TextMeshPro dayNumberText;
    public Transform dayNightIndicator;

    public PlayerMovement player;

    public int numWanderers = 10;

    public float loopLength = 60 * 4;

    public List<GhostMovement> ghosts;
    public List<NPCMovement> wanderers;

    public bool generateRandomNPCs;

    private int numGhosts;

    private float startTime;
    private float lastLoopStartTime;
    private static int randomSeed = 123456789;

    private int currentTimekeeper = 0;

    private List<int> timekeeperDeathLoops;
    private List<int> timekeeperIndices;

    public void SetupLoop(List<MovementKeyframe> record, List<ActionKeyframe> actionRecord) {
        GhostMovement ghost = (GhostMovement) Instantiate(ghostPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        ghost.buildings = this;
        ghost.ghostNum = numGhosts;
        ghost.player = player.transform;
        ghost.record = new List<MovementKeyframe>(record);
        ghost.actionRecord = new List<ActionKeyframe>(actionRecord);
        ghost.StartReplay();
        ghosts.Add(ghost);
        for(int i = 0; i < numGhosts; i++) {
            ghosts[i].record = new List<MovementKeyframe>(record);
            ghosts[i].actionRecord = new List<ActionKeyframe>(actionRecord);
        }
        numGhosts++;

        lastLoopStartTime = Time.fixedTime;
        for(int i = 0; i < numWanderers; i++) {
            wanderers[i].DoReset(lastLoopStartTime);
        }

        DoorScripts[] doors = FindObjectsOfType<DoorScripts>();
        for(int i = 0; i < doors.Length; i++) {
            doors[i].ResetTo(false);
        }

        dayNumberText.text = "" + (numGhosts + 1);

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

        lastLoopStartTime = Time.fixedTime;
        for(int i = 0; i < numWanderers; i++) {
            wanderers[i].DoReset(lastLoopStartTime);
        }

        for(int i = 0; i < timekeeperDeathLoops.Count; i++) {
            if(timekeeperDeathLoops[i] >= numLoop) {
                timekeeperDeathLoops[i] = -1;
            }
        }

        UpdateCurrentTimekeeper();
        dayNumberText.text = "" + (numGhosts + 1);

        //reset player pos
        player.SetToTime(newTime, startTime);
    }

    public void RegisterTimekeeperKill(int timekeeperNum) {
        timekeeperDeathLoops[timekeeperNum] = numGhosts;
        UpdateCurrentTimekeeper();
    }

    private void UpdateCurrentTimekeeper() {
        for(int i = 0; i < timekeeperDeathLoops.Count; i++) {
            if(timekeeperDeathLoops[i] == -1) {
                currentTimekeeper = i;
                return;
            }
        }
    }

    void Start() {
        timekeeperDeathLoops = new List<int>();
        timekeeperIndices = new List<int>();

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

        int c = 0;
        for(int i = 0; i < numWanderers; i++) {
            if(wanderers[i].isTimeKeeper) {
                timekeeperDeathLoops.Add(-1);
                timekeeperIndices.Add(i);
                wanderers[i].timekeeperNum = c;
                c++;
            }
        }
        dayNumberText.text = "" + (numGhosts + 1);
    }

    void Update() {
        if((Time.fixedTime - startTime) / loopLength > numGhosts + 1) {
            SetupLoop(player.record, player.actionRecord);
        }
        compass.setCompassTarget(wanderers[timekeeperIndices[currentTimekeeper]].transform.position);
        dayNightIndicator.localEulerAngles = new Vector3((Time.fixedTime - lastLoopStartTime) / loopLength * 360.0f, 10, 0);
    }
}
