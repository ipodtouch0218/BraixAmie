using UnityEngine;

public class Billboard : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        transform.up = -Camera.main.transform.forward;
    }
}
