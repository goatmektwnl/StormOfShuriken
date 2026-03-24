using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartController : MonoBehaviour
{
    public Image heartImage;
    public Sprite[] breakSprites;

    public void BreakHeart()
    {
        if (heartImage == null) heartImage = GetComponent<Image>();

        if (breakSprites != null && breakSprites.Length > 0)
        {
            StopAllCoroutines();
            StartCoroutine(PlayAnimation());
        }
        else
        {
            // 💡 수정됨: gameObject.SetActive(false) 대신 이미지 렌더링만 끕니다.
            heartImage.enabled = false;
        }
    }

    IEnumerator PlayAnimation()
    {
        for (int i = 0; i < breakSprites.Length; i++)
        {
            heartImage.sprite = breakSprites[i];
            yield return new WaitForSecondsRealtime(0.05f);
        }

        // 💡 수정됨: 다 깨진 후에도 레이아웃(빈자리) 유지를 위해 컴포넌트만 끕니다.
        heartImage.enabled = false;
    }
}