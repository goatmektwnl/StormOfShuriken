using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffController : MonoBehaviour
{
    [Header("분신 설정 (최대 2쌍)")]
    public GameObject clonePrefab;
    public float verticalYOffset = 1.5f; // 위아래 간격
    public float verticalXOffset = 0.5f; // 두 번째 쌍이 뒤로 밀리는 간격

    [Header("피격 연출")]
    public GameObject hitEffectPrefab;

    // 💡 [핵심] 분신을 각각이 아닌 '한 쌍(Pair)'으로 묶어서 관리하는 클래스입니다.
    private class ClonePair
    {
        public GameObject topClone;
        public GameObject bottomClone;
    }

    // 소환된 쌍들을 담아두는 리스트
    private List<ClonePair> activePairs = new List<ClonePair>();

    void Update()
    {
        // 💡 [운명 공동체 감시자] 매 프레임마다 분신 쌍들의 상태를 확인합니다.
        // 리스트를 뒤에서부터 검사하여, 삭제 시 순서가 꼬이는 것을 방지합니다.
        for (int i = activePairs.Count - 1; i >= 0; i--)
        {
            ClonePair pair = activePairs[i];

            // 1. 이미 둘 다 파괴된 경우 (찌꺼기 청소)
            if (pair.topClone == null && pair.bottomClone == null)
            {
                activePairs.RemoveAt(i);
                continue;
            }

            // 2. 위쪽만 터지고 아래쪽이 남은 경우 -> 아래쪽도 연출과 함께 파괴!
            if (pair.topClone == null && pair.bottomClone != null)
            {
                SpawnEffect(pair.bottomClone.transform.position);
                Destroy(pair.bottomClone);
                activePairs.RemoveAt(i);
            }
            // 3. 아래쪽만 터지고 위쪽이 남은 경우 -> 위쪽도 연출과 함께 파괴!
            else if (pair.bottomClone == null && pair.topClone != null)
            {
                SpawnEffect(pair.topClone.transform.position);
                Destroy(pair.topClone);
                activePairs.RemoveAt(i);
            }
        }
    }

    public void ActivateCloneBuff()
    {
        // 💡 최대 2쌍(총 4기)까지만 허용합니다.
        if (activePairs.Count < 2)
        {
            SpawnVerticalPair(activePairs.Count == 0); // 0개면 첫 쌍(true), 1개면 두 번째 쌍(false)
        }
        else
        {
            Debug.Log("분신이 최대치(2쌍)에 도달했습니다!");
            // (옵션) 여기에 점수 추가나 쉴드 회복 코드를 넣으실 수 있습니다.
        }
    }

    void SpawnVerticalPair(bool isFirstPair)
    {
        if (clonePrefab != null)
        {
            // 첫 번째 쌍은 본체와 같은 X선상(0), 두 번째 쌍은 살짝 뒤로(-offset)
            float xOffset = isFirstPair ? 0f : -verticalXOffset;

            ClonePair newPair = new ClonePair();

            // 위쪽 소환
            Vector3 topPos = transform.position + new Vector3(xOffset, verticalYOffset, 0);
            newPair.topClone = Instantiate(clonePrefab, topPos, transform.rotation);
            newPair.topClone.transform.SetParent(this.transform);

            // 아래쪽 소환
            Vector3 bottomPos = transform.position + new Vector3(xOffset, -verticalYOffset, 0);
            newPair.bottomClone = Instantiate(clonePrefab, bottomPos, transform.rotation);
            newPair.bottomClone.transform.SetParent(this.transform);

            // 리스트에 쌍으로 추가
            activePairs.Add(newPair);
        }
    }

    // 본체가 맞았을 때 대신 희생하는 함수
    public bool SacrificeClone()
    {
        if (activePairs.Count > 0)
        {
            // 가장 바깥쪽(나중에 생성된) 쌍부터 희생
            int lastIndex = activePairs.Count - 1;
            ClonePair pairToSacrifice = activePairs[lastIndex];

            // 💡 본체가 맞았으므로, 위/아래 분신을 '동시에' 연출과 함께 파괴합니다!
            if (pairToSacrifice.topClone != null)
            {
                SpawnEffect(pairToSacrifice.topClone.transform.position);
                Destroy(pairToSacrifice.topClone);
            }
            if (pairToSacrifice.bottomClone != null)
            {
                SpawnEffect(pairToSacrifice.bottomClone.transform.position);
                Destroy(pairToSacrifice.bottomClone);
            }

            activePairs.RemoveAt(lastIndex);
            return true;
        }

        return false;
    }

    void SpawnEffect(Vector3 pos)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, pos, Quaternion.identity);
        }
    }
}