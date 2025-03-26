using UnityEngine;

public class RoomController : MonoBehaviour
{
    public Transform entrance;  
    public Transform exit;      

    [SerializeField] private TriggerController roomTrigger; 

    // You can add more functionality as needed
    private void Start()
    {
        // If the room trigger isn't assigned in the editor, try to find it
        if (roomTrigger == null)
            roomTrigger = GetComponentInChildren<TriggerController>();
    }
}