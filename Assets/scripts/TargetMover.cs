using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class TargetMover : MonoBehaviour
{
    [Header("Movement Setting")]
    public float moveSpeed = 5f;

    [Header("Vertical Movement")]
    public float amplitude = 0.5f; 
    public float frequency = 1.0f; 

    private Transform targetTransform;
    private float initialY;

    //풀로 반환하기 위한 델리게이트
    private Action<GameObject> returnToPool;
    private float lifeTime;
    private float timer;

    private void Awake()
    {
        //자식 오브젝트 캐싱은 한 번만 수행
        targetTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; 
        }
    }

    //풀에서 꺼낼 때 호출될 초기화 매서드
    public void Initialize(float speed, float duration, Action<GameObject> onRelease)
    {
        moveSpeed = speed; //기존 속도 개념 적용
        lifeTime = duration;
        returnToPool = onRelease;
        timer = 0;
        initialY = targetTransform.localPosition.y;
    }


    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (targetTransform != null)
        {
            float newY = initialY + Mathf.Sin(Time.time * frequency) * amplitude;
            targetTransform.localPosition = new Vector3(
                targetTransform.localPosition.x,
                newY,
                targetTransform.localPosition.z
            );
        }
        //수명 체크 후 풀 반환(혹시나 안닿았을때를 대비)
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            returnToPool?.Invoke(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {

            Destroy(other.gameObject);
            ReleaseSelf();
        }
        else if (other.CompareTag("Turret"))
        {
            ReleaseSelf();
        }
    }

    private void ReleaseSelf()
    {
        returnToPool?.Invoke(gameObject);
    }
}