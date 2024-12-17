using UnityEngine;

public class MeshEditor : MonoBehaviour
{
    public GameObject pointer; // Pointer GameObject
    private MeshDeformer _targetMeshDeformer = null;
    private float currentCollisionForce = 0f; // ���� �浹 ����
    private HapticCollider hapticCollider; // HapticCollider ����
    //[SerializeField]
    //private GameObject go_rock;
    //[SerializeField]
    //private GameObject go_debrics;

    [SerializeField]
    private GameObject go_effect_perfabs; // ä�� ����Ʈ
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip effect_Sound;
    [SerializeField]
    private AudioClip effect_Sound2;

    private bool isActive = false; // Ȱ��ȭ/��Ȱ��ȭ ����
    private float toggleInterval = 0.005f; // 0.01�� ����
    private float timer = 0f; // Ÿ�̸� ����

    private void Start()
    {
        if (pointer != null)
        {
            hapticCollider = pointer.GetComponent<HapticCollider>();
        }
    }


    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (hapticCollider != null)
        {
            currentCollisionForce = hapticCollider.currentCollisionForce; // ���� ���� ������

           //  Debug.Log($"Current Collision Force: {currentCollisionForce}");
        }

        // Debug.Log($"Timer: {timer}, Current Force: {currentCollisionForce}"); // Ÿ�̸ӿ� �浹 ���� ���

        // 0.01�ʰ� ������ Ȱ��ȭ/��Ȱ��ȭ ���¸� ���
        if (timer >= toggleInterval)
        {
            isActive = !isActive;
           //  Debug.Log($"Toggled isActive: {isActive}"); // Ȱ��ȭ ���� ���
            timer = 0f; // Ÿ�̸� �ʱ�ȭ
        }

    }

    private void Update()
    {
        if (!isActive || pointer == null) return;

        Vector3 pointerPosition = pointer.transform.position;
        float pointerRadius = pointer.transform.localScale.x * 0.5f;

        // Debug.Log($"Pointer Position: {pointerPosition}, Pointer Radius: {pointerRadius}"); // ������ ��ġ �� �ݰ� ���

        Collider[] colliders = Physics.OverlapSphere(pointerPosition, pointerRadius);
        foreach (Collider collider in colliders)
        {
            MeshDeformer deformer = collider.GetComponent<MeshDeformer>();
  
            if (deformer != null)
            {
                _targetMeshDeformer = deformer;

                Vector3? targetVertexPos = _targetMeshDeformer.GetNearVertex(pointerPosition);
                if (targetVertexPos.HasValue)
                {
                    Vector3 collisionNormal = (pointerPosition - collider.ClosestPoint(pointerPosition)).normalized;

                    // Debug.Log($"Target Vertex: {targetVertexPos.Value}, Collision Normal: {collisionNormal}"); // ��� ���ؽ��� �浹 ��� ���

                    ApplyDeformation(targetVertexPos.Value, collisionNormal, currentCollisionForce, pointerRadius);
                }
            }
        }


    }
    private void ApplyDeformation(Vector3 targetVertex, Vector3 collisionNormal, float collisionForce, float pointerRadius)
    {
        if (_targetMeshDeformer != null)
        {
            pointerRadius *= 0.5f; // �ݰ� ����
            Vector3 collisionPoint = pointer.transform.position; // �浹 ����

            // DeformMesh ȣ��
            _targetMeshDeformer.DeformMesh(targetVertex, collisionPoint, collisionNormal, collisionForce, pointerRadius);

            Debug.Log($"Deforming mesh at collision point: {collisionPoint} with force: {collisionForce}, radius: {pointerRadius}");

            if (collisionForce == 0)
            {
                var clone = Instantiate(go_effect_perfabs, collisionPoint, Quaternion.identity);
                Destroy(clone, 3f);
                audioSource.clip = effect_Sound;
                audioSource.Play();

                MeshCollider meshCollider = _targetMeshDeformer.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    meshCollider.sharedMesh = null; // ���� MeshCollider ����
                    meshCollider.sharedMesh = _targetMeshDeformer.GetComponent<MeshFilter>().mesh; // ������ Mesh �Ҵ�

                    // Physics ����ȭ
                    Physics.SyncTransforms();
                }
            }
            else if (collisionForce > 2)
            {
                MeshCollider meshCollider = _targetMeshDeformer.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    // MeshCollider�� GameObject�� HandleRockAndDebris�� ����
                    HandleRockAndDebris(meshCollider.gameObject);
                }
            }
        }
    }

    private void HandleRockAndDebris(GameObject collidedObject)
    {
        // �浹�� Rock�� �θ� ������Ʈ ��������
        Transform parent = collidedObject.transform.parent;
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            if (child.CompareTag("Rock"))
            {
                // Rock ��Ȱ��ȭ
                child.gameObject.SetActive(false);
            }
            else if (child.CompareTag("Debris"))
            {
                // Debris Ȱ��ȭ �� ���� �ð� �� ����
                child.gameObject.SetActive(true);
                Destroy(child.gameObject, 5f);
            }
        }
    }



}
