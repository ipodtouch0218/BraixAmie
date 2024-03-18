using UnityEngine;
using UnityEngine.Networking;

public class YoutubeController : MonoBehaviour {

    public void OnEnable() {

    }

    public void OnDisable() {

    }

    public void Initialize() {

    }

    public void FindLivestream() {
        const string url = "https://youtube.googleapis.com/youtube/v3/liveBroadcasts?part=snippet&broadcastStatus=active&mine=true&key=[YOUR_API_KEY]";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "");
        request.SetRequestHeader("Accept", "application/json");
    }

    public void GetChatMessages() {
        const string url = "https://www.googleapis.com/youtube/v3/liveChat/messages";

    }
}
