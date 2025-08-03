using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using TimeThings;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour {

    public NPCMovement wandererPrefab;
    public List<NPCMovement> availableWanderers;
    public GhostMovement ghostPrefab;

    public FPScript compass;
    public TextMeshPro dayNumberText;
    public TMP_Text numTimekeepersText;
    public Transform dayNightIndicator;

    public PlayerMovement player;

    public Transform directionalLightTransform;

    [SerializeField] MusicScript musicController;
    public int numWanderers = 10;

    public float loopLength = 60 * 4;

    [System.NonSerialized]
    public List<GhostMovement> ghosts;
    public List<NPCMovement> wanderers;

    public bool generateRandomNPCs;

    public Vector2 centerOfGen;
    public Vector2 boundsOfGen;

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
        musicController.SeekStart();
        player.playClick();
    }

    public void ResetToLoop(int ghostNum, int causeType) {
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
        player.SetToTime(newTime, startTime, causeType);
    }

    private IEnumerator waitForIt(string scene) {
        float startTime = Time.time;
        while (Time.time - startTime < 3) {
            yield return null;
        }
        SceneManager.LoadScene(scene);
    }

    public void RegisterTimekeeperKill(int timekeeperNum) {
        timekeeperDeathLoops[timekeeperNum] = numGhosts;
        UpdateCurrentTimekeeper();
    }

    private void UpdateCurrentTimekeeper() {
        for(int i = 0; i < timekeeperDeathLoops.Count; i++) {
            if(timekeeperDeathLoops[i] == -1) {
                currentTimekeeper = i;
                break;
            }
        }
        int numDead = 0;
        for(int i = 0; i < timekeeperDeathLoops.Count; i++) {
            if(timekeeperDeathLoops[i] != -1) {
                numDead++;
            }
        }
        if(numDead == timekeeperDeathLoops.Count - 1) {
            StartCoroutine(waitForIt("Win"));
        }
        numTimekeepersText.text = numDead + "/10 Timekeepers";
    }

    void Start() {
        timekeeperDeathLoops = new List<int>();
        timekeeperIndices = new List<int>();

        if(generateRandomNPCs) {
            Random.InitState(randomSeed);

            if(wanderers == null || wanderers.Count == 0) wanderers = new List<NPCMovement>();
            ghosts = new List<GhostMovement>();

            for(int i = 0; i < wanderers.Count; i++) {
                wanderers[i].InitKeyframes(this.gameObject, centerOfGen.x, centerOfGen.y, boundsOfGen.x, boundsOfGen.y, true);
            }

            for(int i = wanderers.Count; i < numWanderers; i++) {
                NPCMovement prefab = availableWanderers[(int) (availableWanderers.Count * Random.value)];
                NPCMovement wanda = Instantiate(prefab).GetComponent<NPCMovement>();
                wanda.InitKeyframes(this.gameObject, centerOfGen.x, centerOfGen.y, boundsOfGen.x, boundsOfGen.y);
                // if(i < 5) {
                //     wanda.isTimeKeeper = true;
                // }
                wanderers.Add(wanda);
            }
            startTime = Time.fixedTime;
        } else {
            numWanderers = wanderers.Count;
            for(int i = 0; i < numWanderers; i++) {
                wanderers[i].InitKeyframes(this.gameObject, centerOfGen.x, centerOfGen.y, boundsOfGen.x, boundsOfGen.y, true);
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
        if (EasyGameState.gameLost || EasyGameState.gamePaused) {
            if (EasyGameState.gameLost) {
                FPScript firstPerson = player.GetComponentInChildren<FPScript>();
                
                if (firstPerson != null) { 
                    firstPerson.DoLoseAnimation();

                    if (Time.time - EasyGameState.gameLostStart >= 2.11) {
                        player.GetComponentInChildren<FPScript>().DoLoseExplosion();
                    }
                }
                
                float timeSinceFour = Time.time - EasyGameState.gameLostStart - 4.11f;
                Camera camera = player.GetComponentInChildren<Camera>();
                if (Time.time - EasyGameState.gameLostStart >= 4.11) {


                    if (!(Time.time - EasyGameState.gameLostStart >= 6)) {
                        camera.transform.position = Vector3.Lerp(camera.transform.position, EasyGameState.loseCameraPan, timeSinceFour / 4);
                        Quaternion targetRot = ghosts[EasyGameState.resumeGhostNum].transform.rotation;
                        camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, targetRot, timeSinceFour / 4);
                    }
                    
                }

                if (Time.time - EasyGameState.gameLostStart >= 8) {
                    EasyGameState.gameLost = false;
                    startTime = Time.time;
                    firstPerson.resetToGamePlay();
                    ResetToLoop(EasyGameState.resumeGhostNum, EasyGameState.resumeGhostCause);
                    musicController.SeekStart();
                }
            }


            return;
        }
        if((Time.fixedTime - startTime) / loopLength > numGhosts + 1) {
            SetupLoop(player.record, player.actionRecord);
        }
        compass.setCompassTarget(wanderers[timekeeperIndices[currentTimekeeper]].transform.position);
        dayNightIndicator.localEulerAngles = new Vector3((Time.fixedTime - lastLoopStartTime) / loopLength * 360.0f, 10, 0);

        if (numGhosts == 0) {
            musicController.setLute(true);
            musicController.setElectric(false);
            musicController.setRecorder(false);
            musicController.setAlto(false);
        } else if (numGhosts == 1) {
            musicController.setLute(true);
            musicController.setElectric(true);
            musicController.setRecorder(false);
            musicController.setAlto(false);
        } else if (numGhosts == 2) {
            musicController.setLute(true);
            musicController.setElectric(true);
            musicController.setRecorder(true);
            musicController.setAlto(false);
        } else {
            musicController.setLute(true);
            musicController.setElectric(true);
            musicController.setRecorder(true);
            musicController.setAlto(true);
        }

        float coef = (Time.fixedTime - lastLoopStartTime) / loopLength;
        float angle = 360f * coef - (360 * (int) coef);
        directionalLightTransform.localRotation = Quaternion.Euler(angle + 50, 0, 0);
    }
}
