using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingBackground : MonoBehaviour {
    public float minY, maxY, ySpeed, minRot, maxRot, rotSpeed;
    public void Update() {
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;

        pos.y = GetMinMaxProperty(pos.y, Time.deltaTime * ySpeed, minY, maxY);
        rot.y = GetMinMaxProperty(rot.y, Time.deltaTime * rotSpeed, minRot, maxRot);

        transform.position = pos;
        transform.eulerAngles = rot;
    }
    private float GetMinMaxProperty(float currentValue, float change, float min, float max) {
        if (max - min == 0)
            return currentValue;
        return (currentValue + change - min) % (max - min) + min; 
    }
}