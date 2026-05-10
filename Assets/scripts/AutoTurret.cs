using UnityEngine;
using System.Collections;

public class AutoTurret : MonoBehaviour
{
    [Header("References")]
    public Transform target;        // 타겟 드론
    public Transform yawPivot;      // 좌우 회전축 (Y축)
    public Transform pitchPivot;    // 상하 회전축 (X축)
    public Transform muzzlePoint;   // 발사구 (Z축 전방)
    public GameObject projectilePrefab; // 발사체 프리팹

    [Header("Rotation Settings")]
    public float yawSpeed = 5f;     // 좌우 회전 속도
    public float pitchSpeed = 5f;   // 상하 회전 속도
    public float minPitch = -45f;   // 포신이 위로 올라가는 최대 각도 (음수)
    public float maxPitch = 20f;    // 포신이 아래로 내려가는 최대 각도 (양수)

    [Header("Combat Settings")]
    public float fireAngleThreshold = 5f; // 조준 완료 허용 각도 (도)
    public float fireInterval = 0.5f;     // 발사 간격 (초)
    private float fireTimer = 0f;

    [Header("이펙트 셋팅")]
    private Vector3 defaultPitchPos; //터렛의 원래 위치를 저장할 변수

    private void Start()
    {
        defaultPitchPos = pitchPivot.localPosition;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindNearestEnemy();
        }

        if (target == null) return;

        UpdateYaw();
        UpdatePitch();
        CheckAndFire();
    }
    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
        target = nearestEnemy;
    }


    private void UpdateYaw()
    {

        Vector3 dirToTarget = target.position - yawPivot.position;

        dirToTarget.y = 0f;

        if (dirToTarget.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);

        yawPivot.rotation = Quaternion.Slerp(yawPivot.rotation, targetRotation, Time.deltaTime * yawSpeed);
    }

    private void UpdatePitch()
    {
        Vector3 localTargetPos = yawPivot.InverseTransformPoint(target.position);

        float angleX = -Mathf.Atan2(localTargetPos.y, localTargetPos.z) * Mathf.Rad2Deg;

        angleX = Mathf.Clamp(angleX, minPitch, maxPitch);

        Quaternion targetPitch = Quaternion.Euler(angleX, 0f, 0f);
        pitchPivot.localRotation = Quaternion.Slerp(pitchPivot.localRotation, targetPitch, Time.deltaTime * pitchSpeed);
    }

    private void CheckAndFire()
    {
        if (target == null || muzzlePoint == null) return;
        Vector3 dirToTarget = (target.position - muzzlePoint.position).normalized;
        float angleToTarget = Vector3.Angle(muzzlePoint.forward, dirToTarget);

        Debug.Log($"현재 오차 각도: {angleToTarget}도 / 허용치: {fireAngleThreshold}");
        fireTimer -= Time.deltaTime;

        if (angleToTarget <= fireAngleThreshold && fireTimer <= 0f)
        {
            FireProjectile();
            fireTimer = fireInterval;
            Debug.Log("발사 조건 충족! 발사!");
        }

    }

    private void FireProjectile()
    {
        Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);

        StopCoroutine(nameof(RecoilRoutine));
        StartCoroutine(RecoilRoutine());

        Debug.DrawRay(muzzlePoint.position, muzzlePoint.forward * 10f, Color.red, 0.5f);
    }

    private void OnDrawGizmos()
    {
        if (muzzlePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(muzzlePoint.position, muzzlePoint.forward * 5f);
        }
    }
    IEnumerator RecoilRoutine()
    {

        Vector3 recoilPos = defaultPitchPos - Vector3.forward * 0.5f;

        pitchPivot.localPosition = recoilPos;
        yield return new WaitForSeconds(0.05f);

        float duration = 0.2f; // 복귀 시간
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            pitchPivot.localPosition = Vector3.Lerp(recoilPos, defaultPitchPos, elapsed / duration);
            yield return null;
        }

        pitchPivot.localPosition = defaultPitchPos;
    }
}