using UnityEngine;

public class MeshEditor : MonoBehaviour
{
    public GameObject pointer; // Pointer GameObject
    private MeshDeformer _targetMeshDeformer = null;
    private float currentCollisionForce = 0f; // 현재 충돌 강도
    private HapticCollider hapticCollider; // HapticCollider 참조
    //[SerializeField]
    //private GameObject go_rock;
    //[SerializeField]
    //private GameObject go_debrics;

    [SerializeField]
    private GameObject go_effect_perfabs; // 채굴 이펙트
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip effect_Sound;
    [SerializeField]
    private AudioClip effect_Sound2;

    private bool isActive = false; // 활성화/비활성화 상태
    private float toggleInterval = 0.005f; // 0.01초 간격
    private float timer = 0f; // 타이머 변수

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
            currentCollisionForce = hapticCollider.currentCollisionForce; // 값을 직접 가져옴

           //  Debug.Log($"Current Collision Force: {currentCollisionForce}");
        }

        // Debug.Log($"Timer: {timer}, Current Force: {currentCollisionForce}"); // 타이머와 충돌 강도 출력

        // 0.01초가 지나면 활성화/비활성화 상태를 토글
        if (timer >= toggleInterval)
        {
            isActive = !isActive;
           //  Debug.Log($"Toggled isActive: {isActive}"); // 활성화 상태 출력
            timer = 0f; // 타이머 초기화
        }

    }

    private void Update()
    {
        if (!isActive || pointer == null) return;

        Vector3 pointerPosition = pointer.transform.position;
        float pointerRadius = pointer.transform.localScale.x * 0.5f;

        // Debug.Log($"Pointer Position: {pointerPosition}, Pointer Radius: {pointerRadius}"); // 포인터 위치 및 반경 출력

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

                    // Debug.Log($"Target Vertex: {targetVertexPos.Value}, Collision Normal: {collisionNormal}"); // 대상 버텍스와 충돌 노멀 출력

                    ApplyDeformation(targetVertexPos.Value, collisionNormal, currentCollisionForce, pointerRadius);
                }
            }
        }


    }
    private void ApplyDeformation(Vector3 targetVertex, Vector3 collisionNormal, float collisionForce, float pointerRadius)
    {
        if (_targetMeshDeformer != null)
        {
            pointerRadius *= 0.5f; // 반경 조정
            Vector3 collisionPoint = pointer.transform.position; // 충돌 지점

            // DeformMesh 호출
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
                    meshCollider.sharedMesh = null; // 기존 MeshCollider 비우기
                    meshCollider.sharedMesh = _targetMeshDeformer.GetComponent<MeshFilter>().mesh; // 변형된 Mesh 할당

                    // Physics 동기화
                    Physics.SyncTransforms();
                }
            }
            else if (collisionForce > 2)
            {
                MeshCollider meshCollider = _targetMeshDeformer.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    // MeshCollider의 GameObject를 HandleRockAndDebris로 전달
                    HandleRockAndDebris(meshCollider.gameObject);
                }
            }
        }
    }

    private void HandleRockAndDebris(GameObject collidedObject)
    {
        // 충돌한 Rock의 부모 오브젝트 가져오기
        Transform parent = collidedObject.transform.parent;
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            if (child.CompareTag("Rock"))
            {
                // Rock 비활성화
                child.gameObject.SetActive(false);
            }
            else if (child.CompareTag("Debris"))
            {
                // Debris 활성화 및 일정 시간 후 삭제
                child.gameObject.SetActive(true);
                Destroy(child.gameObject, 5f);
            }
        }
    }



}
