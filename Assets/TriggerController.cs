using System.Collections.Generic;
using UnityEngine;

public class TriggerController : MonoBehaviour
{
    public List<GameObject> enemies; // Danh sách quái trong phòng
    public List<GameObject> doors;   // Danh sách cửa trong phòng
    private bool isTriggered = false; // Đánh dấu trigger chỉ kích hoạt một lần
    private bool doorsOpened = false; // Đánh dấu cửa đã mở để tránh lặp lại

    private void Start()
    {
        // Vô hiệu hóa quái lúc đầu
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) enemy.SetActive(false);
        }

        // Giữ cửa mở lúc đầu
        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered) // Khi nhân vật vào phòng
        {
            isTriggered = true; // Đánh dấu đã kích hoạt
            ActivateEnemies();
            CloseDoors();
            //gameObject.SetActive(false); // Vô hiệu hóa trigger sau khi kích hoạt
        }
    }

    void ActivateEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) enemy.SetActive(true);
        }
    }

    void CloseDoors()
    {
        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(true); // Đóng cửa
        }
    }

    void OpenDoors()
    {
        foreach (GameObject door in doors)
        {
            if (door != null) door.SetActive(false); // Mở cửa
        }
        Debug.Log("Tất cả quái bị tiêu diệt, cửa đã mở!");
        doorsOpened = true; // Đánh dấu cửa đã mở để không chạy lại
    }

    private void Update()
    {
        if (!isTriggered || doorsOpened) return; // Nếu chưa kích hoạt hoặc cửa đã mở, không cần check

        // Xóa quái đã bị Destroy khỏi danh sách
        enemies.RemoveAll(enemy => enemy == null);

        // Kiểm tra nếu tất cả quái đã chết, mở cửa
        if (enemies.Count == 0)
        {
            OpenDoors();
        }
    }
}
