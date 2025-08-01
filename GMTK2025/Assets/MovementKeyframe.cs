using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeThings {
    public class MovementKeyframe {

        //0: movement, 1: kill
        public float time;
        public Vector3 pos;
        public float rotX;
        public float rotY;

        public MovementKeyframe(float time, Vector3 pos, float rotX, float rotY) {
            this.time = time;
            this.pos = pos;
            this.rotX = rotX;
            this.rotY = rotY;
        }
        
    }

    public class ActionKeyframe {

        //0: kill
        public float time;
        public int type;
        public GameObject interacted;

        public ActionKeyframe(float time, int type, GameObject interacted = null) {
            this.time = time;
            this.type = type;
            this.interacted = interacted;
        }
        
    }
}