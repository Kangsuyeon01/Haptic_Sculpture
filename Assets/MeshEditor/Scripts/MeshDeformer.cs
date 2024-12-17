using System.Collections.Generic;
using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    private Mesh _deformingMesh;
    private Vector3[] _originalVertices, _displacedVertices;
    private MeshCollider _meshCollider;

    private void Start()
    {
        _deformingMesh = GetComponent<MeshFilter>().mesh;
        _originalVertices = _deformingMesh.vertices;
        _displacedVertices = new Vector3[_originalVertices.Length];
        for (int i = 0; i < _originalVertices.Length; i++)
        {
            _displacedVertices[i] = _originalVertices[i];
        }

        _meshCollider = GetComponent<MeshCollider>();
    }

    public Vector3? GetNearVertex(Vector3 worldPosition)
    {
        Vector3 nearVertex = Vector3.zero;
        float minDistance = float.MaxValue;
        foreach (Vector3 vertex in _displacedVertices)
        {
            var pos = this.transform.TransformPoint(vertex);
            var distance = (pos - worldPosition).sqrMagnitude;
            if (minDistance > distance)
            {
                minDistance = distance;
                nearVertex = pos;
            }
        }
        return nearVertex;
    }

    public void DeformMesh(Vector3 targetVertex, Vector3 collisionPoint, Vector3 collisionNormal, float collisionForce, float pointerRadius)
    {
        Debug.Log($"Deforming mesh at collision point: {collisionPoint.ToString("F5")} with force: {collisionForce.ToString("F5")}, radius: {pointerRadius.ToString("F5")}");

        // Pointer 반경 내의 정점만 변형
        int[] vertexIndices = GetVertexIndices(targetVertex, pointerRadius);
        float adjustedStrength = Mathf.Clamp(collisionForce * 0.2f, 0.01f, 0.0f); // 강도 조정
        for (int i = 0; i < vertexIndices.Length; i++)
        {
            int index = vertexIndices[i];
            if (index != -1)
            {
                Vector3 vertexWorldPos = this.transform.TransformPoint(_displacedVertices[index]);
                float distance = Vector3.Distance(vertexWorldPos, collisionPoint);

                float falloff = Mathf.Exp(-distance * 10.0f); // 거리 기반 감쇠
                Vector3 displacement = collisionNormal * -adjustedStrength * falloff;
                _displacedVertices[index] += this.transform.InverseTransformVector(displacement);
            }
        }
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        // 메쉬 업데이트
        _deformingMesh.vertices = _displacedVertices;
        _deformingMesh.RecalculateNormals(); // 법선 재계산
        _deformingMesh.RecalculateBounds(); // 경계 갱신

        // MeshCollider 업데이트
        if (_meshCollider != null)
        {
            _meshCollider.sharedMesh = null; // 기존 콜라이더 비우기
            _meshCollider.sharedMesh = _deformingMesh; // 새로운 메쉬 설정

            // Collider 비활성화 후 재활성화
            _meshCollider.enabled = false;
            _meshCollider.enabled = true;
        }
    }

    private int[] GetVertexIndices(Vector3 targetVertex, float radius)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < _displacedVertices.Length; i++)
        {
            float distance = Vector3.Distance(this.transform.TransformPoint(_displacedVertices[i]), targetVertex);
            if (distance <= radius)
            {
                indices.Add(i);
            }
        }
        return indices.ToArray();
    }
}
