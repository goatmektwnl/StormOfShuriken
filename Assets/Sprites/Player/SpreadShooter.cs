using UnityEngine;

public class SpreadShooter : MonoBehaviour
{
    [Header("발사체 설정")]
    public GameObject centerKunai;   // 가운데로 나갈 기본 쿠나이
    public GameObject sideShuriken;  // 위, 아래(좌,우)로 나갈 수리검
    public float spreadGap = 0.8f;   // 수리검이 위아래로 벌어지는 간격

    [Header("사거리 설정 (생존 시간)")]
    [Tooltip("수치가 클수록 더 멀리 날아갑니다. (시간 = 거리)")]
    public float projectileLifetime = 1.5f; // 💡 이 수치로 사거리를 조절합니다.

    void Start()
    {
        // 1. 가운데 쿠나이 생성 및 수명 설정
        if (centerKunai != null)
        {
            GameObject kunai = Instantiate(centerKunai, transform.position, transform.rotation);
            Destroy(kunai, projectileLifetime); // 지정된 시간(projectileLifetime) 뒤에 파괴
        }

        // 2. 위쪽(좌측) 수리검 생성 및 수명 설정
        if (sideShuriken != null)
        {
            GameObject topShuriken = Instantiate(sideShuriken, transform.position + new Vector3(0, spreadGap, 0), transform.rotation);
            Destroy(topShuriken, projectileLifetime);
        }

        // 3. 아래쪽(우측) 수리검 생성 및 수명 설정
        if (sideShuriken != null)
        {
            GameObject bottomShuriken = Instantiate(sideShuriken, transform.position + new Vector3(0, -spreadGap, 0), transform.rotation);
            Destroy(bottomShuriken, projectileLifetime);
        }

        // 4. 발사 직후 이 껍데기(슈터)는 흔적도 없이 파괴!
        Destroy(gameObject);
    }
}