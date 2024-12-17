using UnityEngine;

public class MeshEditToggle : MonoBehaviour
{
    [SerializeField]
    private MeshEditor meshEditor;
    private bool isMeshEditActive = false; // MeshEdit Ȱ��ȭ ����

    void Update()
    {
        // T Ű �Է� ����
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleMeshEdit(); // MeshEdit ���� ��ȯ
        }
    }

    private void ToggleMeshEdit()
    {
        if (meshEditor != null)
        {
            // Ȱ��ȭ ���� ���
            isMeshEditActive = !isMeshEditActive;
            meshEditor.enabled = isMeshEditActive; // MeshEditor Ȱ��ȭ/��Ȱ��ȭ

            // ��� Mesh Collider�� convex ���� ����
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
        // ��� Mesh Collider ã��
        MeshCollider[] meshColliders = FindObjectsOfType<MeshCollider>();

        foreach (MeshCollider meshCollider in meshColliders)
        {
            // MeshCollider�� convex �Ӽ� ����
            meshCollider.convex = convexState;
            Debug.Log($"MeshCollider '{meshCollider.name}' convex set to {convexState}");
        }
    }
}
