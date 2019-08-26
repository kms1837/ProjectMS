using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[System.Serializable]
public class Map
{
    [System.Serializable]
    public struct threatNode
    {
        public string title;
        public string desc;
        public List<mapNode> trigger;
        public List<mapNode> result;
    }

    [System.Serializable]
    public class mapNode
    {
        public string type;
        public List<int> monsterList;
        public int count;
        public float cycle;
        public int produce;
    }

    public struct threatTriggerNode
    {
        public float progress;
        public bool trigger;
        public List<string> logs;
    }

    public string filed;
    public List<mapNode> regen;
    public List<threatNode> threat;
    public threatNode goal;

    public static Map loadMap(string fileName) {
        Map loadedMap = null;

        try {
            TextAsset jsonText = Resources.Load(fileName) as TextAsset;
            loadedMap = JsonUtility.FromJson<Map>(jsonText.text);
        }
        catch (System.Exception error) {
            Debug.LogError(error);
        }

        return loadedMap;
    }

    public void backgroundBatch(GameObject backgroundObject, float basePositionX) {
        int otherObjectWidth = Screen.width;
        const int cloudWidth = 270;
        RectTransform backgroundSize = backgroundObject.GetComponent<RectTransform>();

        Debug.Log("batch SIze :" + basePositionX);

        Sprite cloudSprite = Resources.Load<Sprite>("imgs/dummy/cloud");
        Sprite otherObjectSprite = Resources.Load<Sprite>("imgs/dummy/mountain");

        for (int index = 0; index < 5; index++) {
            GameObject newCloudObject = new GameObject("cloud");
            RectTransform newCloudSize = newCloudObject.AddComponent<RectTransform>();
            float cloudHeight = cloudSprite.rect.height * (cloudWidth / cloudSprite.rect.width);
            newCloudObject.transform.SetParent(backgroundObject.transform);
            newCloudSize.sizeDelta = new Vector2(cloudWidth, cloudHeight);
            newCloudObject.transform.localPosition = new Vector2(basePositionX - Random.Range(-1 * (Screen.width / 2), (Screen.width / 2)), (Random.Range(0, backgroundSize.rect.height / 2)));

            Image newCloudImage = newCloudObject.AddComponent<Image>();
            newCloudImage.sprite = cloudSprite;
        }

        GameObject newOtherObject = new GameObject("backObject");
        RectTransform newOtherObjectSize = newOtherObject.AddComponent<RectTransform>();
        newOtherObject.transform.SetParent(backgroundObject.transform);
        float otherObjectHeight = otherObjectSprite.rect.height * (otherObjectWidth / otherObjectSprite.rect.width);
        newOtherObjectSize.sizeDelta = new Vector2(otherObjectWidth, otherObjectHeight);
        newOtherObject.transform.localPosition = new Vector2(basePositionX, (-1 * (backgroundSize.rect.height / 2)) + (otherObjectHeight / 2));

        Image newOtherObjectImage = newOtherObject.AddComponent<Image>();
        newOtherObjectImage.sprite = otherObjectSprite;

        //BackGround
    } // 배경 오브젝트들을 설정한 위치를 기준으로 생성합니다.

    public threatTriggerNode triggerCheck(mapNode trigerObj, Score checkScore) {
        threatTriggerNode result = new threatTriggerNode();
        result.progress = 0;
        result.trigger = false;
        result.logs = new List<string>();
        switch (trigerObj.type) {
            case "hunt":
                int huntedCount = 0;
                foreach (int huntedMonsterNumber in trigerObj.monsterList) {
                    if (checkScore.killScore.ContainsKey(huntedMonsterNumber)) {
                        huntedCount += checkScore.killScore[huntedMonsterNumber];

                        result.logs.Add(huntedMonsterNumber + ": (" + checkScore.killScore[huntedMonsterNumber] + "/" + trigerObj.count + ") 사냥됨.");
                    }
                }

                result.progress = huntedCount == 0 ? 0 : (huntedCount / (float)trigerObj.count);

                if (huntedCount >= trigerObj.count) {
                    result.trigger = true;
                }
                break;
        }

        return result;
    } // 위협 조건하나가 성립됐는지 확인
}
