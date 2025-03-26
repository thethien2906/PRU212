using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public bool facingLeft = true; // Is the enemy facing right
    public float speed = 2.0f; // Speed of the enemy
    public float distance = 1.0f; // Distance the enemy can see
    public Transform checkPointLeft; // The point the enemy will check for the player
   
    public LayerMask playerMask; // The layer the player is on
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.left * Time.deltaTime * speed ); 

       RaycastHit2D hit = Physics2D.Raycast(checkPointLeft.position, Vector2.down, distance, playerMask);

        if (hit == false && facingLeft)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingLeft = false;
        }
        else if (hit == false && !facingLeft)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingLeft = true;
        }

    }

    private void OnDrawGizmos()
    {
        if (checkPointLeft == null)
        {
            return;
        }
        // Draw a ray in the scene view to show the distance the enemy can see
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(checkPointLeft.position, Vector2.down * distance);
    }
   

}
