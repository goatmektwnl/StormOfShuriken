using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartController : MonoBehaviour
{
    public Sprite[] breakSprites; // 6장의 스프라이트 등록
    private Image img;

    void Awake() => img = GetComponent<Image>();

    public void BreakHeart() => StartCoroutine(PlayAnim());

    IEnumerator PlayAnim()
    {
        for (int i = 0; i < breakSprites.Length; i++)
        {
            img.sprite = breakSprites[i];
            yield return new WaitForSeconds(0.05f);
        }
    }
}