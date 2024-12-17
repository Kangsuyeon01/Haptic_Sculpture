using UnityEngine;
using System.Collections.Generic;

public class MaxSubdivision : MonoBehaviour
{
    [SerializeField] private int subdivisionLevel = 4; // ����ȭ ���� ���� (�� ���̸� �� ���� ����)

    public void SubdivideMax()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        // ���� ���ؽ��� �ﰢ�� ������ ��������
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<Vector3> newVertices = new List<Vector3>(vertices);
        List<int> newTriangles = new List<int>();

        Dictionary<string, int> midPointCache = new Dictionary<string, int>();

        for (int level = 0; level < subdivisionLevel; level++) // ����ȭ�� �ݺ������� ����
        {
            newVertices.Clear();
            newVertices.AddRange(vertices);
            newTriangles.Clear();

            midPointCache.Clear(); // ĳ�� �ʱ�ȭ

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];

                // �� �ﰢ���� �߰����� ���
                int a = GetMidPointIndex(v1, v2, newVertices, midPointCache);
                int b = GetMidPointIndex(v2, v3, newVertices, midPointCache);
                int c = GetMidPointIndex(v3, v1, newVertices, midPointCache);

                // ���ο� �ﰢ�� �߰�
                newTriangles.AddRange(new int[] { v1, a, c });
                newTriangles.AddRange(new int[] { v2, b, a });
                newTriangles.AddRange(new int[] { v3, c, b });
                newTriangles.AddRange(new int[] { a, b, c });
            }

            // Mesh ������Ʈ
            vertices = newVertices.ToArray();
            triangles = newTriangles.ToArray();

            if (vertices.Length > 65535)
            {
                Debug.LogWarning("���� �Ѱ踦 �ʰ��Ͽ����ϴ�. �� �̻� ����ȭ�� �� �����ϴ�.");
                break;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        Debug.Log($"���� ����ȭ �Ϸ�: ���� {vertices.Length}, �ﰢ�� {triangles.Length / 3}");
    }

    private int GetMidPointIndex(int v1, int v2, List<Vector3> vertices, Dictionary<string, int> cache)
    {
        string key = v1 < v2 ? $"{v1}_{v2}" : $"{v2}_{v1}";

        if (cache.TryGetValue(key, out int index))
        {
            return index;
        }

        Vector3 midPoint = (vertices[v1] + vertices[v2]) * 0.5f;
        vertices.Add(midPoint);
        int newIndex = vertices.Count - 1;
        cache[key] = newIndex;

        return newIndex;
    }
}
