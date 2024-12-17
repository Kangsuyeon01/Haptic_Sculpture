using UnityEngine;

public class MeshEditToggle : MonoBehaviour
{
    [SerializeField]
    private MeshEditor meshEditor;
    private bool isMeshEditActive = false; // MeshEdit 활성화 상태

    void Update()
    {
        // T 키 입력 감지
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleMeshEdit(); // MeshEdit 상태 전환
        }
    }

    private void ToggleMeshEdit()
    {
        if (meshEditor != null)
        {
            // 활성화 상태 토글
            isMeshEditActive = !isMeshEditActive;
            meshEditor.enabled = isMeshEditActive; // MeshEditor 활성화/비활성화

            // 모든 Mesh Collider의 convex 상태 변경
            ToggleAllMeshColliders(isMeshEditActive);

            Debug.Log($"MeshEdit is now {(isMeshEditActive ? "Enabled" : "Disabled")}");
        }
        else
        {
            Debug.LogWarning("MeshEditor reference is missing!");
        }
    }

    private void ToggleAllMeshColliders(bool convexState)
    {
        // 모든 Mesh Collider 찾기
        MeshCollider[] meshColliders = FindObjectsOfType<MeshCollider>();

        foreach (MeshCollider meshCollider in meshColliders)
        {
            // MeshCollider의 convex 속성 변경
            meshCollider.convex = convexState;
            Debug.Log($"MeshCollider '{meshCollider.name}' convex set to {convexState}");
        }
    }
}
