using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject startRoomPrefab;  // Phòng bắt đầu
    public GameObject bossRoomPrefab;   // Phòng Boss
    public GameObject[] normalRooms;    // Danh sách phòng bình thường
    public int roomCount = 5;           // Số lượng phòng thường

    private List<RoomController> generatedRooms = new List<RoomController>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        Vector2 currentPosition = Vector2.zero;
        RoomController previousRoom = null;

        // Tạo phòng bắt đầu
        RoomController startRoom = Instantiate(startRoomPrefab, currentPosition, Quaternion.identity).GetComponent<RoomController>();
        generatedRooms.Add(startRoom);
        previousRoom = startRoom;

        // Tạo các phòng thường
        for (int i = 0; i < roomCount; i++)
        {
            GameObject randomRoomPrefab = normalRooms[Random.Range(0, normalRooms.Length)];
            RoomController newRoom = Instantiate(randomRoomPrefab, Vector2.zero, Quaternion.identity).GetComponent<RoomController>();

            ConnectRooms(previousRoom, newRoom);

            generatedRooms.Add(newRoom);
            previousRoom = newRoom;
        }

        // Tạo phòng Boss
        RoomController bossRoom = Instantiate(bossRoomPrefab, Vector2.zero, Quaternion.identity).GetComponent<RoomController>();
        ConnectRooms(previousRoom, bossRoom);
        generatedRooms.Add(bossRoom);
    }

    void ConnectRooms(RoomController previous, RoomController next)
    {
        Vector2 exitPos = previous.exit.position;
        Vector2 entrancePos = next.entrance.position;

        Vector2 offset = exitPos - entrancePos;
        next.transform.position += (Vector3)offset;
    }
}
