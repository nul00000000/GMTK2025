using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeThings {
    public class MovementKeyframe {

        //0: translational, 1: jump
        public float time;
        public int type;
        public Vector3 pos;
        public float rotX;
        public float rotY;

        public MovementKeyframe(float time, int type, Vector3 pos, float rotX, float rotY) {
            this.time = time;
            this.type = type;
            this.pos = pos;
            this.rotX = rotX;
            this.rotY = rotY;
        }
        
    }
}