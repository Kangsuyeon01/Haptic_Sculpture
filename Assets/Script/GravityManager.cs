using UnityEngine;

public class GravityManager : MonoBehaviour
{
    void Start()
    {
        // Terrain Collider 감지
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out hit, Mathf.Infinity))
        {
            // Terrain 위에 물체 배치
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }
}
