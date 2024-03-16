using UnityEngine;

public class HoldRotation : MonoBehaviour {

    private Quaternion rotation;

    public void Start() {
        rotation = transform.rotation;
    }

    public void Update() {
        transform.rotation = rotation;
    }
}
