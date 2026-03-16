using UnityEngine;

public class SpreadShooter : MonoBehaviour
{
    public GameObject centerKunai;   // 가운데로 나갈 기본 쿠나이
    public GameObject sideShuriken;  // 위, 아래(좌,우)로 나갈 수리검
    public float spreadGap = 0.8f;   // 💡 수리검이 위아래로 벌어지는 간격

    void Start()
    {
        // 1. 가운데 쿠나이 발사!
        if (centerKunai != null) Instantiate(centerKunai, transform.position, transform.rotation);

        // 2. 위쪽(좌측) 수리검 발사!
        if (sideShuriken != null) Instantiate(sideShuriken, transform.position + new Vector3(0, spreadGap, 0), transform.rotation);

        // 3. 아래쪽(우측) 수리검 발사!
        if (sideShuriken != null) Instantiate(sideShuriken, transform.position + new Vector3(0, -spreadGap, 0), transform.rotation);

        // 4. 발사 직후 이 껍데기는 흔적도 없이 파괴!
        Destroy(gameObject);
    }
}
