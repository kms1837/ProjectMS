using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using tupleType = System.Collections.Generic.Dictionary<string, object>;

public class Skill :MonoBehaviour
{
    [System.Serializable]
    public struct skillCell
    {
        public int type;
        public int target;
        public float power;
        public int loop; // 스킬 루프
        public float loopTime; // 다음 발동 시간(루프)
        public float duration; // 지속시간
        public int percentage; // 백분율 수치
        public List<buffCell> buffList;
        public string next;
        public string hit;
        public string fail;
    }

    [System.Serializable]
    public struct buffCell
    {
        public string abillity;
        public int percentage;
        public float power;
    }

    private Dictionary<string, skillCell> skillSet;
    private Dictionary<string, int> learnSet; // 습득 조건
    private skillCell currentSkill; // 현재 발동중인 스킬
    private bool activeFlag; // 스킬 사용 가능 여부

    public ArrayList targetBuffList;
    // 만일 이 스킬이 버프 스킬일 경우 자동으로 버프를 해제해야 함으로 그 버프리스트의 주소를 갖기위해 만든변수

    private GameObject allianceGroup;
    private GameObject enemyGroup;

    public Sprite iconSprite;

    private Character user; // 스킬 사용자

    private int loopCount;

    public int id; // 중복검사를 하기 위해 id값을 가짐
    public Ability infomation;
    private float coolTime; // 스킬 재사용 대기시간

    //ui
    public ArrayList coolTimeBar;

    public enum SkillType { Heal, Buff, Physical, Magic, Debuff, Condition }
    private enum SkillTargetType { Single, Broadcast, Self, Projectile }

    public static Skill addSkill(GameObject target, int skillID) {
        DataBase db = new DataBase("MS");
        Skill newSkill = target.AddComponent<Skill>();
        tupleType skillData = db.getTuple("skills", skillID);

        newSkill.infomation.beforeDelay = (float)skillData["before_delay"];
        newSkill.infomation.afterDelay = (float)skillData["after_delay"];
        newSkill.infomation.power = (float)skillData["power"];
        newSkill.infomation.nameStr = (string)skillData["name"];
        newSkill.infomation.manaPoint = (float)skillData["mana"];

        newSkill.id = (int)skillData["id"];
        newSkill.coolTime = (float)skillData["cool_time"];
        newSkill.iconSprite = Resources.Load<Sprite>((string)skillData["icon"]);

        newSkill.skillSet = newSkill.loadScript((string)skillData["script"]);

        db.closeDB();
        db = null;

        return newSkill;
    } // 스킬을 생성하고 설정합니다.

    public void coolTImeBarSetting(ProgressBar progress, Color barColor) {
        progress.init(this.coolTime, new Color(255.0f, 255.0f, 255.0f));
        coolTimeBar.Add(progress);
    }

    private Dictionary<string,int> loadLearnSet(string fileName) {
        JSONObject loadedScript = null;
        // Key값으로 스킬들을 구분하고 있어 외부 라이브러리와 섞어 사용함.
        Dictionary<string,int> learnSet = new Dictionary<string,int>();

        try {
            TextAsset jsonText = Resources.Load(string.Format("json/skill/{0}",fileName)) as TextAsset;
            loadedScript = new JSONObject(jsonText.text);

            if (!object.ReferenceEquals(loadedScript["main"]["learn"],null)) {
                foreach (string key in loadedScript["main"]["learn"].keys) {
                    int learnPoint = int.Parse(loadedScript["main"]["learn"].GetField(key).ToString());
                    learnSet.Add(key,learnPoint);
                }
            }
            else {
                return null;
            }
        }
        catch (System.Exception error) {
            Debug.LogError(string.Format("Load Fail : json/skill/{0}",fileName));
            Debug.LogError(error);
        }

        return learnSet;
    }

    private Dictionary<string, skillCell> loadScript(string fileName) {
        JSONObject loadedScript = null;
        // Key값으로 스킬들을 구분하고 있어 외부 라이브러리와 섞어 사용함.
        Dictionary<string, skillCell> loadSkillset = null;

        try {
            TextAsset jsonText = Resources.Load(string.Format("json/skill/{0}", fileName)) as TextAsset;
            loadedScript = new JSONObject(jsonText.text);
            loadSkillset = new Dictionary<string, skillCell>();

            foreach (string key in loadedScript.keys) {
                string jsonStr = loadedScript.GetField(key).ToString();
                skillCell skill = JsonUtility.FromJson<skillCell>(jsonStr);
                loadSkillset.Add(key, skill);
            }
        }
        catch (System.Exception error) {
            Debug.LogError(string.Format("Load Fail : json/skill/{0}", fileName));
            Debug.LogError(error);
        }

        return loadSkillset;
    }

    public bool activation() {
        Debug.Log("스킬-" + this.infomation.nameStr + "발동시도");

        if (!activeFlag) {
            Debug.LogWarning("쿨타임");
            return false;
        }

        bool state = true;

        if (this.infomation.manaPoint > user.currentManaPoint) {
            Debug.LogWarning("마나가 부족합니다.");
            return false;
        }

        user.currentManaPoint -= this.infomation.manaPoint;

        switch (skillSet["main"].target) {
            case (int)SkillTargetType.Single:
                state = singleActivation();
                break;
            case (int)SkillTargetType.Self:
                state = selfActivation();
                break;
            case (int)SkillTargetType.Projectile:
                state = projectileActivation();
                break;
        }
        
        activeFlag = false;
        
        foreach (ProgressBar bar in this.coolTimeBar) {
            bar.setColor(new Color(255.0f, 255.0f, 255.0f));
            bar.runProgress();
        }


        if (PlayerStats.GlobalScore.skillScore.ContainsKey(this.id)) {
            PlayerStats.GlobalScore.skillScore[id] += 1;
        } else {
            PlayerStats.GlobalScore.skillScore.Add(this.id, 1);
        }
        
        learnSkill();

        this.Invoke("recycle", coolTime);

        return state;
    } // 스킬 발동

    private void learnSkill() {
        DataBase db = new DataBase("MS");
        Dictionary<int, tupleType> skillList = db.getList("skills");

        foreach(int key in skillList.Keys) {
            bool learnFlag = true;
            tupleType skillData = skillList[key];
            Dictionary<string, int> learnSet = loadLearnSet((string)skillData["script"]);

            if (!object.ReferenceEquals(learnSet, null) ) {
                foreach (string learnKey in learnSet.Keys) {
                    int skillID = int.Parse(learnKey);
                    if (learnSet[learnKey] > PlayerStats.GlobalScore.skillScore[skillID]) {
                        learnFlag = false;
                        break;
                    }
                }

                if (learnFlag) {
                    Skill newSkill = Skill.addSkill(user.gameObject, key);
                    GameObject quickUIGroup = GameObject.Find("GameSystem").GetComponent<InGameManager>().quickUIGroup;
                    QuickSkills quickSkill = quickUIGroup.transform.Find("QuickSkills").GetComponent<QuickSkills>();
                    quickSkill.setSlotSkill(1, newSkill);
                }
            }
        }

        db.closeDB();
        db = null;
    } // 스킬 습득

    private bool singleActivation() {
        if (user.aggroTarget == null) {
            Debug.Log("target unknown");
            return false;
        } // 스킬 발동가능여부 검사(타겟 탐색)

        return true;
    } // 1인 타켓팅 스킬

    private bool selfActivation() {
        switch (skillSet["main"].type) {
            case (int)SkillType.Heal:
                user.heal(this.infomation.power);
                break;
            case (int)SkillType.Buff:
                break;
        }
        return true;
    } // 자신에게 사용하는 스킬

    private bool projectileActivation() {
        GameObject projectileObj = Resources.Load("prefab/object/Projectile") as GameObject;
        GameObject newProjectile = Instantiate(projectileObj,user.transform.position,Quaternion.identity);
        Projectile projectileSet = newProjectile.GetComponent<Projectile>();
        projectileSet.infomation.power = this.infomation.power;
        projectileSet.direction = user.transform.Find("Object").localScale.x;

        return true;
    } // 투사체 발사 스킬

    private void healSkill() {
        Character targetInfo;
        float healPower = 0;
        if (currentSkill.target == (int)SkillTargetType.Broadcast) { // 광역스킬
            foreach (Transform target in allianceGroup.transform) {
                targetInfo = target.GetComponent<Character>();
                healPower = ((targetInfo.infomation.afterDelay * currentSkill.percentage) / 100);
                healPower += this.infomation.power;
                targetInfo.heal(healPower);
                Debug.Log(string.Format("{0}에게 {1} 만큼 회복시킴", targetInfo.infomation.nameStr, healPower));
            }
        }
        else {
            if (user.aggroTarget != null) {
                targetInfo = user.aggroTarget.GetComponent<Character>();
                healPower = ((targetInfo.infomation.afterDelay * currentSkill.percentage) / 100);
                healPower += this.infomation.power;
                user.aggroTarget.GetComponent<Character>().heal(healPower);
                Debug.Log(string.Format("{0}에게 {1} 만큼 회복시킴", targetInfo.infomation.nameStr, healPower));
            }
        }
    }

    private void physicalSkill() {
        Character targetInfo;
        if (currentSkill.target == (int)SkillTargetType.Broadcast) { // 거리 상관없이 발동됨
            foreach (Transform target in allianceGroup.transform) {
                targetInfo = target.GetComponent<Character>();
                targetInfo.hit(this.infomation.power, targetInfo.direction);
                Debug.Log(string.Format("{0}에게 {1} 만큼 물리 대미지를 입힘", targetInfo.infomation.nameStr, this.infomation.power));
            }
        }
        else {
            if (user.aggroTarget != null) {
                targetInfo = user.aggroTarget.GetComponent<Character>();
                targetInfo.hit(this.infomation.power,targetInfo.direction);
                Debug.Log(string.Format("{0}에게 {1} 만큼 물리 대미지를 입힘", targetInfo.infomation.nameStr, this.infomation.power));
            }
        }
    }

    private void magicSkill() {
        Character targetInfo;
        if (currentSkill.target == (int)SkillTargetType.Broadcast) { // 거리 상관없이 발동됨
            foreach (Transform target in allianceGroup.transform) {
                targetInfo = target.GetComponent<Character>();
                targetInfo.hit(this.infomation.power,targetInfo.direction);
                Debug.Log(string.Format("{0}에게 {1} 만큼 마법 대미지를 입힘", targetInfo.infomation.nameStr, this.infomation.power));
            }
        }
        else {
            if (user.aggroTarget != null) {
                targetInfo = user.aggroTarget.GetComponent<Character>();
                targetInfo.hit(this.infomation.power,targetInfo.direction);
                Debug.Log(string.Format("{0}에게 {1} 만큼 마법 대미지를 입힘", targetInfo.infomation.nameStr, this.infomation.power));
            }
        }
    }

    private void createBuff(Transform target) {
        Character targetInfo = target.GetComponent<Character>();
        ArrayList targetBuffList = targetInfo.buffList;

        foreach (Skill buffTest in targetBuffList) {
            if (buffTest.id == this.id) {
                buffTest.autoBuffRelease(currentSkill.duration);
                Debug.Log(string.Format("{0}의 {1} 버프를 갱신", targetInfo.infomation.nameStr, this.infomation.power));
                return;
            } // 버프 갱신
        }
        
        Skill newBuff = target.gameObject.AddComponent<Skill>();
        newBuff.id = this.id;
        // 후에 json에 id를 부여해서 버프별 파일을 따로 만들어 두어도 좋을것 같음

        foreach (buffCell buffInfo in currentSkill.buffList) {
            switch (buffInfo.abillity) {
                case "afterDelay":
                    newBuff.infomation.afterDelay = ((targetInfo.infomation.afterDelay * buffInfo.percentage) / 100) * -1;
                    newBuff.infomation.afterDelay -= buffInfo.power;
                    break;
                case "beforeDelay":
                    newBuff.infomation.beforeDelay = ((targetInfo.infomation.afterDelay * buffInfo.percentage) / 100) * -1;
                    newBuff.infomation.beforeDelay -= buffInfo.power;
                    break; // 딜레이 감소
                case "power":
                    newBuff.infomation.power = ((targetInfo.infomation.power * buffInfo.percentage) / 100);
                    newBuff.infomation.power += buffInfo.power;
                    break;
            }
        }
        
        newBuff.targetBuffList = targetBuffList;
        newBuff.targetBuffList.Add(newBuff);
        newBuff.autoBuffRelease(currentSkill.duration);

        Debug.Log(string.Format("{0}에게 {1} 버프를 줌", targetInfo.infomation.nameStr, this.infomation.power));
        Debug.Log("버프 발동");
    }

    private void buffSkill() {
        if (currentSkill.target == (int)SkillTargetType.Broadcast) { // 거리 상관없이 발동됨
            foreach (Transform target in allianceGroup.transform) {
                createBuff(target);
            }
        }
        else if (currentSkill.target == (int)SkillTargetType.Self) {
            createBuff(user.transform);
        }
        else {
            if (user.aggroTarget != null) {
                createBuff(user.aggroTarget.transform);
            }
        }
    }

    private void skillLoop() {
        int skillType = currentSkill.type;

        switch (skillType) {
            case (int)SkillType.Heal:
                healSkill();
                break;
            case (int)SkillType.Buff:
                buffSkill();
                break;
            case (int)SkillType.Physical:
                physicalSkill();
                break;
            case (int)SkillType.Magic:
                break;
            
        }

        loopCount--;

        if (loopCount <= 0 || user == null) {
            CancelInvoke("skillLoop");
        }
    }

    private void skillCellActive(skillCell skill) {
        loopCount = skill.loop;
        currentSkill = skill;
        Debug.Log(loopCount);
        if (loopCount == 0) {
            skillLoop();
        } else {
            InvokeRepeating("skillLoop", this.infomation.beforeDelay, skill.loopTime);
        }
        
    }

    private void recycle() {
        activeFlag = true;
    }

    public void autoBuffRelease(float duration) {
        this.CancelInvoke("buffRelease");
        this.Invoke("buffRelease", duration);
        // object적으로 분리시키기 위해 메서드를 따로 만듦
        // string으로 메서드이름을 받아서 그런건지 외부에서 invoke를 걸어도 외부오브젝트만의 변수가아닌 invoke를 건 쪽의 주소를 사용하는 문제가 있었음.
    }

    private void buffRelease() {
        this.targetBuffList.Remove(this);
        Destroy(this);

        Debug.Log("buffRelease");
    } // 버프를 해제합니다.

    private void Awake() {
        infomation = new Ability();
        coolTimeBar = new ArrayList();
    }

    private void Start () {
        activeFlag = true;
        user = this.gameObject.GetComponent<Character>();
    }
}
