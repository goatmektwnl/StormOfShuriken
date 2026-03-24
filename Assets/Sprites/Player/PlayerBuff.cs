using System.Collections;
using UnityEngine;

public class PlayerBuff : MonoBehaviour
{
    [Header("💣 폭발 버프 설정 (일정 시간 유지)")]
    public float bombBuffDuration = 5f;
    public GameObject explosiveKunaiPrefab;
    public float bombFireRate = 0.1f;

    [Header("🌀 수리검 버프 설정 (영구 지속)")]
    public GameObject shurikenSpreadPrefab;
    public float shurikenFireRate = 0.2f;

    [Header("획득 이펙트 설정")]
    public GameObject pickupEffectPrefab;

    [Header("쉴드 시스템")]
    public bool hasShield = false;       // 쉴드 장착 여부
    public GameObject shieldVisualObject;      // (선택) 쉴드 시각 효과 오브젝트

    private GameObject originalKunaiPrefab;
    private float originalFireRate;

    private PlayerController playerController;

    // 💡 버프 상태를 기억하는 똑똑한 스위치들!
    private bool isBombActive = false;
    private bool isShurikenPermanent = false;
    private Coroutine bombCoroutine;

    public void ActivateShield()
    {
        hasShield = true;

        // 💡 2. 파괴신님의 몸을 감싸는 시각 효과 오브젝트를 활성화(ON)합니다!!
        if (shieldVisualObject != null)
        {
            shieldVisualObject.SetActive(true);
            // Debug.Log("방어막 시각 효과 가동!!");
        }
    }

    // 💡 쉴드가 깨질 때 호출될 함수!
    public void BreakShield()
    {
        hasShield = false;

        // 💡 3. 시각 효과 오브젝트를 즉시 비활성화(OFF)합니다!!
        if (shieldVisualObject != null)
        {
            shieldVisualObject.SetActive(false);
        }
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        // 게임 시작 시, 원래 쓰던 기본 쿠나이와 발사 속도를 안전하게 백업합니다.
        originalKunaiPrefab = playerController.kunaiPrefab;
        originalFireRate = playerController.fireRate;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 폭발 버프 획득! (타이머 작동)
        if (other.CompareTag("BombBuff"))
        {
            if (pickupEffectPrefab != null) Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(other.gameObject);

            // 기존 폭발 타이머가 있다면 초기화하고 처음부터 다시 잽니다.
            if (bombCoroutine != null) StopCoroutine(bombCoroutine);
            bombCoroutine = StartCoroutine(BombBuffRoutine());
        }
        // 2. 수리검 버프 획득! (영구 지속 스위치 ON)
        else if (other.CompareTag("ShurikenBuff"))
        {
            if (pickupEffectPrefab != null) Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(other.gameObject);

            // 💡 코루틴(타이머) 없이 스위치만 켭니다! (죽을 때까지 안 꺼짐)
            isShurikenPermanent = true;
            UpdateWeaponState();


        }
    }

    // 폭발 버프 타이머
    IEnumerator BombBuffRoutine()
    {
        isBombActive = true;
        UpdateWeaponState(); // 장비 업데이트!

        yield return new WaitForSeconds(bombBuffDuration);

        isBombActive = false;
        UpdateWeaponState(); // 시간 끝나면 다시 업데이트!
    }

    // 💡 핵심: 현재 켜져 있는 버프 스위치들을 보고, 가장 강력한 조합을 알아서 장착해 주는 함수!
    void UpdateWeaponState()
    {
        // 1. 가운데로 나갈 메인 무기 결정 (폭발 버프가 켜져 있으면 폭발 쿠나이, 꺼졌으면 일반 쿠나이)
        GameObject currentCenterWeapon = isBombActive ? explosiveKunaiPrefab : originalKunaiPrefab;

        // 2. 발사 속도 결정 (폭발이 최우선, 그다음 수리검, 둘 다 없으면 원래 속도)
        float currentFireRate = originalFireRate;
        if (isBombActive) currentFireRate = bombFireRate;
        else if (isShurikenPermanent) currentFireRate = shurikenFireRate;

        // 3. 최종 무기 장착!
        if (isShurikenPermanent)
        {
            // 수리검 모드일 때는, 투명 발사대(SpreadShooter)의 '가운데 무기'를 현재 상황에 맞게 싹 갈아 끼웁니다!
            SpreadShooter shooterScript = shurikenSpreadPrefab.GetComponent<SpreadShooter>();
            if (shooterScript != null)
            {
                shooterScript.centerKunai = currentCenterWeapon;
            }
            playerController.kunaiPrefab = shurikenSpreadPrefab;
        }
        else
        {
            // 수리검 모드가 아닐 때는 그냥 메인 무기(일반 or 폭발)만 장착합니다.
            playerController.kunaiPrefab = currentCenterWeapon;
        }

        // 발사 속도 덮어쓰기
        playerController.fireRate = currentFireRate;
    }
}
