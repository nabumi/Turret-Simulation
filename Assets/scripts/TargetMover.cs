using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class TargetMover : MonoBehaviour
{
    [Header("Movement Setting")]
    public float moveSpeed = 5f;
    private float currentSpeed; //워프 중에는 멈추게 하기 위한 변수

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
        currentSpeed = speed; //기본 속도 저장
        lifeTime = duration;
        returnToPool = onRelease;
        timer = 0;
        initialY = targetTransform.localPosition.y;

        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(WarpInRoutine());
        }
    }


    void Update()
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

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
            ReleaseSelf();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {

            Destroy(other.gameObject);
            StartCoroutine(HitEffectRoutine());
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

    IEnumerator HitEffectRoutine()
    {
        currentSpeed = 0f; // 맞으면 멈춤
        Renderer rd = targetTransform.GetComponent<Renderer>();
        if (rd != null)
        {
            Color originalColor = rd.material.color;
            for (int i = 0; i < 2; i++)
            {
                rd.material.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                rd.material.color = originalColor;
                yield return new WaitForSeconds(0.05f);
            }
        }
        ReleaseSelf();
    }

    IEnumerator WarpInRoutine()
    {
        transform.localScale = Vector3.zero;
        currentSpeed = 0;

        float duration1 = 0.15f;
        float elapsed1 = 0f;
        while (elapsed1 < duration1)
        {
            elapsed1 += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, elapsed1 / duration1);
            yield return null;
        }
        transform.localScale = Vector3.one * 1.2f;

        float duration2 = 0.1f;
        float elapsed2 = 0f;
        while (elapsed2 < duration2)
        {
            elapsed2 += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, elapsed2 / duration2);
            yield return null;
        }
        transform.localScale = Vector3.one;

        currentSpeed = moveSpeed;
    }
}