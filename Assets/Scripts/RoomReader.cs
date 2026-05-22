using UnityEngine;
using Meta.XR.MRUtilityKit;
using Debug = UnityEngine.Debug;

public class RoomReader : MonoBehaviour
{
    public GameObject planePrefab;

    void Start()
    {
        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK Instance is null!");
            return;
        }
        MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("Room is null!");
            return;
        }

        // Spawn floor
        var floor = room.FloorAnchor;
        if (floor != null)
        {
            SpawnPlane(floor.transform, floor.PlaneRect.Value.size);
            Debug.Log("Floor spawned at: " + floor.transform.position);
        }

        // Spawn walls
        foreach (var wall in room.WallAnchors)
        {
            SpawnPlane(wall.transform, wall.PlaneRect.Value.size);
            Debug.Log("Wall spawned at: " + wall.transform.position);
        }
    }

    void SpawnPlane(Transform anchor, Vector2 size)
    {
        var plane = Instantiate(planePrefab, anchor.position, anchor.rotation);
        plane.transform.SetParent(null);
        plane.transform.Rotate(0, 180f, 0); // flip to face inward
        plane.transform.localScale = new Vector3(size.x, size.y, 0.01f);

        var renderer = plane.GetComponent<MeshRenderer>();
        renderer.material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
    }
}