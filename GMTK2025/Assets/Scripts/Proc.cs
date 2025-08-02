using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

enum LastRoad {
    Straight,
    Intersection,

}
public class Proc : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] List<GameObject> housePrefabs;
    [SerializeField] GameObject roadStraight;
    [SerializeField] GameObject roadIntersect;
    [SerializeField] GameObject roadT;
    [SerializeField] GameObject roadMac;
    [SerializeField] Transform start;
    [SerializeField] Terrain terrain;
    const int seed = 1234567890;
    const int maxDepth = 4;
    private List<Bounds> bounds = new List<Bounds>();
    private void CreateRoad(GameObject road, Vector3 position, Quaternion rotation) {
        Instantiate(road, position, rotation);
    }

    private Vector3 getIntersectionToRoadPosition(GameObject inter, GameObject road, Vector3 direction) {
        Bounds interBounds = inter.GetComponent<MeshCollider>().bounds;
        Vector3 edge = interBounds.ClosestPoint(inter.transform.position + direction * 40);

        Bounds roadBounds = road.GetComponent<MeshCollider>().bounds;
        return roadBounds.size.z * direction + inter.transform.InverseTransformPoint(edge);
    }

    private Vector3 getNextIntersectionPoint(GameObject road, GameObject inter, Vector3 direction) {
        Bounds roadBounds = road.GetComponent<MeshCollider>().bounds;

        return Vector3.zero;
    }

    private void addFlattenScript(GameObject obj) {
        FlattenToTerrain script = obj.AddComponent<FlattenToTerrain>();
        script.meshFilter = obj.GetComponent<MeshFilter>();
        script.terrain = terrain;
    }   

    void workNode(Vector3 currentPosition, Vector3 direction, Boolean forceIntersection, int depth, LastRoad lastRoad, GameObject lastGameObject) {
        if (depth == maxDepth) {
            return;
        }

        int rand = 0;

        
        GameObject newGameObject = null;
        LastRoad last = LastRoad.Straight;

        if (rand == 0) { // continue the road;
            if (lastRoad == LastRoad.Straight) {
                newGameObject = Instantiate(roadStraight);
                addFlattenScript(newGameObject);
                if (lastGameObject == null) {
                    newGameObject.transform.position = currentPosition;
                } else {
                    newGameObject.transform.position = getIntersectionToRoadPosition(lastGameObject, newGameObject, direction);
                    Debug.Log(depth + " " + newGameObject.transform.position);
                }
            }
        } 

        
        

        
        workNode(currentPosition, direction, forceIntersection, depth + 1, last, newGameObject);
    }
    void Start()
    {
        bounds.Clear();
        workNode(start.position, Vector3.forward, true, 0, LastRoad.Intersection, null);
    }

    // Update is called once per frame
    
    void Update()
    {
    }
}
