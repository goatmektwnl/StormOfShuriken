using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartController : MonoBehaviour
{
    public Image heartImage;
    public Sprite[] breakSprites; // 이제 쪼개진 조각 6개가 여기 들어감!

    public void BreakHeart()
    {
        if (heartImage == null) heartImage = GetComponent<Image>();

        // 조각이 제대로 들어왔는지 확인
        if (breakSprites != null && breakSprites.Length > 0)
        {
            StopAllCoroutines();
            StartCoroutine(PlayAnimation());
        }
        else
        {
            gameObject.SetActive(false); // 조각 없으면 그냥 끄기
        }
    }

    IEnumerator PlayAnimation()
    {
        // 0번부터 5번까지 순서대로 교체
        for (int i = 0; i < breakSprites.Length; i++)
        {
            heartImage.sprite = breakSprites[i];
            yield return new WaitForSeconds(0.05f); // 0.05초면 아주 찰진 속도!
        }

        // 다 깨지면 숨기기
        gameObject.SetActive(false);
    }
}