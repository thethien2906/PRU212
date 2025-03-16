using UnityEngine;

public class AutoRunBot : MonoBehaviour
{
    public Transform checkpoint1;
    public Transform checkpoint2;
    public float speed = 0.0001f; // Điều chỉnh tốc độ di chuyển siêu chậm

    private Transform target;
    private bool facingRight = true;

    void Start()
    {
        target = checkpoint2; // Bắt đầu di chuyển đến checkpoint 2
    }

    void Update()
    {
        MoveTowardsTarget();
    }

    void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Khi chạm đến checkpoint, đổi hướng
        if (Vector3.Distance(transform.position, target.position) < 0.005f)
        {
            target = (target == checkpoint1) ? checkpoint2 : checkpoint1;
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }
}
