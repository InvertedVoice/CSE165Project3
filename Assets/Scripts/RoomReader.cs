using UnityEngine;
using Meta.XR.MRUtilityKit;
using Debug = UnityEngine.Debug;

public class RoomReader : MonoBehaviour
{
    void OnEnable()
    {
        MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        var room = MRUK.Instance.GetCurrentRoom();

        var floor = room.FloorAnchor;
        Debug.Log("Floor anchor: " + floor.transform.position);

        foreach (var wall in room.WallAnchors)
        {
            Debug.Log("Wall anchor: " + wall.transform.position);
        }
    }
}