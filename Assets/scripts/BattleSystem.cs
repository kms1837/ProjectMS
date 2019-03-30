﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

using UnityEngine.UI;

using tupleType = System.Collections.Generic.Dictionary<string, object>;

public class BattleSystem :MonoBehaviour {
    public GameObject HeroGroup;
    public GameObject EnemyGroup;
    public GameObject UserSpecGroup;
    public GameObject Player; // 조작 하는 케릭터
    public GameObject BackGround; // 배경
    public GameObject TopView;
    public GameObject TopStatusUI; // 상태관련 UI들
    public Transform TopBlackPanel;
    public Transform SceneFaderImage;

    public Text ThreatLabel;

    public GameObject CharacterObject; // 복제 오브젝트
    public GameObject UserSpecPanel; // 유저 정보 패널 복제 오브젝트(bottom view)

    public GameObject BottomView;
    public Transform InfomationGroup;

    private Map map;

    private Score totalScore; // 누적이 되는 상황기록
    private Score localScore; // 갱신이 되는 상황기록
    private ArrayList threatScore; // 위협발생 체크용 상황기록
    private bool goalFlag; // 최종 단계인지 체크(최종단계일경우 몬스터 생성과 위협이 중단된다.)

    private DataBase db;
    private RoomState.roomData roomInfo;

    private List<Coroutine> regenCoroutines;
    private const float UIGAP = 10.0f;
    private Vector2 userStartPoint = new Vector2(-1000, 0);

    private Rect topviewSize;

    void Start() {
        // battle infomation display
        Text PlaceLabel = InfomationGroup.Find("PlaceLabel").GetComponent<Text>();
        Text TargetLabel = InfomationGroup.Find("TargetLabel").GetComponent<Text>();
        Text ThreatLabel = TopStatusUI.transform.Find("ThreatLabel").GetComponent<Text>();

        topviewSize = TopView.GetComponent<RectTransform>().rect;

        StatusBar goalProgressBar = TopStatusUI.transform.Find("GoalProgressBar").GetComponent<StatusBar>();
        goalProgressBar.init(100.0f, new Color(0, 255.0f, 0));
        goalProgressBar.setCurrent(0);

        db = new DataBase("DT");
        roomInfo = RoomState.loadRoomData("json/quest_board");

        // 더미
        RoomState.playerID = 1;
        RoomState.place = 1;
        RoomState.threat = 1;
        RoomState.orderUser = 1;

        /*
        RoomState.addUser(2);
        RoomState.addUser(3);
        RoomState.addUser(4);
        RoomState.addUser(5);
        */
        tupleType place = db.getTuple("places", RoomState.place);
        tupleType threat = db.getTuple("threats", RoomState.threat);

        PlaceLabel.text = string.Format("{0} - {1}", place["name"] as string, roomInfo.golaList[RoomState.gola]);
        TargetLabel.text = threat["name"] as string;

        Debug.Log(place["name"] as string + ", " + roomInfo.golaList[RoomState.gola] + ", " + threat["name"] as string + ", ");

        map = Map.loadMap(string.Format("json/threat/{0}", threat["file"] as string));
        runRegen(); // 설정한 대로 몬스터들을 재 매 설정 시간마다 몬스터를 생성시킵니다.

        if (map == null) {
            Debug.Log("map load error");
        }

        totalScore = new Score();
        localScore = new Score();
        threatScore = new ArrayList();

        for (int index = 0; index < map.threat.Count + 1; index++) {
            threatScore.Add(new Score());
        }// goal의 score포함

        playerInit();
        usersInit();
        map.backgroundBatch(BackGround, 0);

        SceneFaderImage.GetComponent<Fader>().fadeOutStart(() => {
            SceneFaderImage.gameObject.SetActive(false);
        });
    }

    /* ui positioning function */
    private float getTopviewGroundPositionY(GameObject target) {
        Rect targetSize = target.GetComponent<RectTransform>().rect;
        return (-1 * (topviewSize.height / 2)) + (targetSize.height / 2);
    }

    /* logics */
    private void gameOver() {
        Debug.Log("~ game over ~");
        this.stopRegen();
        Common.showBackPanel(TopView.transform, TopBlackPanel);

        GameObject gameOverUI = TopStatusUI.transform.Find("GameOver").gameObject;
        TopStatusUI.transform.SetAsLastSibling();
        gameOverUI.SetActive(true);
    }

    IEnumerator regenCycle(Map.mapNode regenInfo) {
        while (true) {
            regen(regenInfo.monsterList);
            yield return new WaitForSeconds(regenInfo.cycle);
        }
    } // 몬스터를 설정 시간마다 생성함

    private void monsterRegen(int monsterID) {
        tupleType monsterData = db.getTuple("monsters", monsterID);
        // issue - 빈 정보가 오면? 예외처리
        GameObject regenMonster = Instantiate(CharacterObject, EnemyGroup.transform);
        Character monsterInfo = regenMonster.GetComponent<Character>();
        Transform monsterVisual = regenMonster.transform.Find("Sprite");
        Image monsterSprite = regenMonster.GetComponent<Image>();
        float setPositionY = getTopviewGroundPositionY(regenMonster);

        monsterInfo.setSprite((string)monsterData["sprite"]);

        monsterVisual.localScale = new Vector3(-1, 1, 1);

        regenMonster.transform.localPosition = new Vector2(1000, setPositionY);

        monsterInfo.setting(monsterData, -1);

        Debug.Log(string.Format("regen [{0}]: (hp: {1}, )", (string)monsterData["name"], (float)monsterData["health_point"]));

        StatusBar headHpBar = regenMonster.transform.Find("HpBar").GetComponent<StatusBar>();
        headHpBar.init((float)monsterData["health_point"], new Color(255.0f, 0, 0));
        monsterInfo.hpBar.Add(headHpBar);

        StatusBar headDelayBar = regenMonster.transform.Find("DelayBar").GetComponent<StatusBar>();
        headDelayBar.init(0, new Color(0, 0, 255.0f));
        monsterInfo.delayBar.Add(headDelayBar);

        monsterInfo.destroyCallback = (() => {

            if (localScore.killPoint.ContainsKey(monsterID)) {
                totalScore.killPoint[monsterID] += 1;
                localScore.killPoint[monsterID] += 1;
                foreach (Score score in threatScore) {
                    score.killPoint[monsterID] += 1;
                }
            }
            else {
                totalScore.killPoint.Add(monsterID, 1);
                localScore.killPoint.Add(monsterID, 1);
                foreach (Score score in threatScore) {
                    score.killPoint.Add(monsterID, 1);
                }
            }

            Debug.Log("사망[" + monsterID + "]:" + localScore.killPoint[monsterID]);

            ThreatLabel.text = "";

            threat();
        });
    } // 몬스터 생성

    private void regen(List<int> monsterList) {
        foreach (int monsterNumber in monsterList) {
            monsterRegen(monsterNumber);
        }
    } // 몬스터 생성

    private void runRegen() {
        regenCoroutines = new List<Coroutine>();
        foreach (Map.mapNode regenObj in map.regen) {
            regenCoroutines.Add(StartCoroutine(regenCycle(regenObj)));
        }
    } // 몬스터를 일정시간마다 생성시키는것을 시작함

    private void stopRegen() {
        foreach (Coroutine regenObj in regenCoroutines) {
            StopCoroutine(regenObj);
        }
    } // 몬스터 생성 중지

    private void threatProgress(Map.threatNode threatObj, Score currentThreatScore) {
        StatusBar goalProgressBar = TopStatusUI.transform.Find("GoalProgressBar").GetComponent<StatusBar>();
        float totalProgress = 0;
        foreach (Map.mapNode trigerObj in threatObj.trigger) {
            totalProgress += map.triggerCheck(trigerObj, currentThreatScore).progress;
        }

        totalProgress = (totalProgress / threatObj.trigger.Count);

        goalProgressBar.setCurrent(totalProgress * 100);
    }
   
    private bool threatProcess(Map.threatNode threatObj, Score currentThreatScore) {
        bool trigger = true;
        Map.threatTriggerNode trigerNode;

        ThreatLabel.text += "\n[" + threatObj.title + "]\n";

        foreach (Map.mapNode trigerObj in threatObj.trigger) {
            trigerNode = map.triggerCheck(trigerObj, currentThreatScore);
            trigger = trigger && trigerNode.trigger;

            foreach (string log in trigerNode.logs) {
                ThreatLabel.GetComponent<Text>().text += log;
            }
        }

        if (trigger) {
            Debug.Log("[" + threatObj.title + "위협발생" + "]");

            foreach (Map.mapNode trigerObj in threatObj.trigger) {
                foreach (int monsterNum in trigerObj.monsterList) {
                    currentThreatScore.killPoint[monsterNum] = 0;
                }// 위협 초기화
            }

            foreach (Map.mapNode resultObj in threatObj.result) {
                if (!goalFlag) {
                    switch (resultObj.type) {
                        case "produce":
                            foreach (int monster in resultObj.monsterList) {
                                // 몬스터 생성
                                for (int monsterCount = 0; monsterCount < resultObj.produce; monsterCount++) {
                                    monsterRegen(monster);
                                }
                            }
                            break;
                    }
                }
            }
        }

        return trigger;
    } // 위협의 설정에 맞게 처리함

    private void threat() {
        int index = 0;
        foreach (Map.threatNode threatObj in map.threat) {
            threatProcess(threatObj, (Score)threatScore[index]);
            index++;
        } // 위협들 체크

        if (!this.goalFlag) {
            threatProgress(map.goal, (Score)threatScore[index]);
        }

        if (threatProcess(map.goal, (Score)threatScore[index])) {
            this.goalFlag = true;
            this.stopRegen();
            Debug.Log("Boss");
        };
    }

    private void playerInit() {
        Character playerObj = Player.GetComponent<Character>();

        Transform ultimateSkillBtn = BottomView.transform.Find("UltimateSkillButton");
        Transform subvSkillBtn1 = BottomView.transform.Find("SubSkillButton1");
        Transform subvSkillBtn2 = BottomView.transform.Find("SubSkillButton2");

        GameObject userSpecPanel = UserSpecGroup.transform.GetChild(0).gameObject;
        tupleType userInfo = db.getTuple("users", RoomState.playerID);
        float originPositionY = getTopviewGroundPositionY(Player);

        userObjectSetting(Player, userInfo);
        panelSetting(userSpecPanel, userInfo, Player);

        StatusBar ultimateSkillCollTimeBar = ultimateSkillBtn.Find("CoolTimeBar").GetComponent<StatusBar>();
        playerObj.ultimateSkillObj.coolTImeBarSetting(ultimateSkillCollTimeBar, new Color(255.0f, 255.0f, 255.0f));
        skillBtnSetting(ultimateSkillBtn.Find("CoolTimeBar"), playerObj.ultimateSkillObj);
        playerObj.ultimateSkillObj.coolTimeBar.Add(ultimateSkillCollTimeBar);

        StatusBar subSkillCollTimeBar1 = subvSkillBtn1.Find("CoolTimeBar").GetComponent<StatusBar>();
        playerObj.subSkillObj1.coolTImeBarSetting(subSkillCollTimeBar1, new Color(255.0f, 255.0f, 255.0f));
        skillBtnSetting(subvSkillBtn1.Find("CoolTimeBar"), playerObj.subSkillObj1);
        playerObj.subSkillObj1.coolTimeBar.Add(subSkillCollTimeBar1);

        StatusBar subSkillCollTimeBar2 = subvSkillBtn2.Find("CoolTimeBar").GetComponent<StatusBar>();
        playerObj.subSkillObj2.coolTImeBarSetting(subSkillCollTimeBar2, new Color(255.0f, 255.0f, 255.0f));
        skillBtnSetting(subvSkillBtn2.Find("CoolTimeBar"), playerObj.subSkillObj2);
        playerObj.subSkillObj2.coolTimeBar.Add(subSkillCollTimeBar2);

        /*
        playerObj.equipments[0] = new Ability();
        playerObj.equipments[0].energyPower = 40; // 공격력 40 장비 장착 더미
        */
        Player.transform.localPosition = new Vector2(userStartPoint.x, originPositionY);
    } // 플레이어 셋팅

    private void userPanelSetting(GameObject setUserPanel, tupleType userInfo) {
        RectTransform panelRect = setUserPanel.GetComponent<RectTransform>();
        int thisIndex = UserSpecGroup.transform.childCount - 1;
        GameObject thisUserObject = HeroGroup.transform.GetChild(HeroGroup.transform.childCount - 1).gameObject;
        float baseY = UserSpecGroup.transform.GetChild((thisIndex - 1)).localPosition.y;

        panelSetting(setUserPanel, userInfo, thisUserObject);

        panelRect.localPosition = new Vector2(0, baseY - UIGAP - panelRect.rect.height);
    }

    private void panelSetting(GameObject setUserPanel, tupleType userInfo, GameObject userObject) {
        int userID = userObject.GetComponent<Character>().infomation.id;
        Transform specInfo = setUserPanel.transform.Find("SpecInfo");
        GameObject targetingBtn = specInfo.Find("TargetingBtn").gameObject;
        Button targetingBtnObj = targetingBtn.GetComponent<Button>();
        Text nameLabel = specInfo.Find("NameLabel").GetComponent<Text>();
        Text levelLabel = specInfo.Find("LevelLabel").GetComponent<Text>();

        Transform ultimateSkillBtn = specInfo.transform.Find("UltimateSkillButton");

        nameLabel.text = (string)userInfo["name"];
        levelLabel.text = string.Format("Lv.{0}", ((int)userInfo["level"]).ToString());

        if (Player.GetComponent<Character>().attackType == (int)Character.CharacterAttackType.Heal) {
            targetingBtn.SetActive(true);
            targetingBtnObj.onClick.AddListener(() => { userTargetingBtn(userObject); });
        } else {
            targetingBtn.SetActive(false);
        }

        Character setUser = userObject.GetComponent<Character>();
        Transform uiHpBar = specInfo.Find("HpBar");
        StatusBar hpBar = uiHpBar.GetComponent<StatusBar>();
        hpBar.init((float)userInfo["health_point"], new Color(255.0f, 0, 0));
        setUser.hpBar.Add(hpBar);

        Skill ultimateSkill = userObject.AddComponent<Skill>();
        ultimateSkill.setting(db.getTuple("skills", (int)userInfo["ultimate_skill"]), HeroGroup, EnemyGroup, true);
        setUser.ultimateSkillObj = ultimateSkill;

        Skill subSkill1 = userObject.AddComponent<Skill>();
        subSkill1.setting(db.getTuple("skills", (int)userInfo["skill1"]), HeroGroup, EnemyGroup, false);
        setUser.subSkillObj1 = subSkill1;

        Skill subSkill2 = userObject.AddComponent<Skill>();
        subSkill2.setting(db.getTuple("skills", (int)userInfo["skill2"]), HeroGroup, EnemyGroup, false);
        setUser.subSkillObj2 = subSkill2;

        Transform collTimebar = ultimateSkillBtn.Find("CoolTimeBar");
        skillBtnSetting(collTimebar, ultimateSkill);

        StatusBar ultimateSkillCollTimeBar = collTimebar.GetComponent<StatusBar>();
        ultimateSkill.coolTImeBarSetting(ultimateSkillCollTimeBar, new Color(255.0f, 255.0f, 255.0f));
        ultimateSkill.coolTimeBar.Add(ultimateSkillCollTimeBar);

        Debug.Log(userID + " " + RoomState.playerID);

        if (RoomState.orderUser == RoomState.playerID && userID != RoomState.playerID) {
            RectTransform specInfoSize = specInfo.GetComponent<RectTransform>();
            RectTransform hpMoveBar = uiHpBar.Find("MoveBar").GetComponent<RectTransform>();
            RectTransform hpCurrentBar = uiHpBar.Find("Bar").GetComponent<RectTransform>();
            hpMoveBar.sizeDelta = new Vector2(hpMoveBar.rect.width, specInfoSize.rect.height / 8);
            hpCurrentBar.sizeDelta = new Vector2(hpCurrentBar.rect.width, specInfoSize.rect.height / 8);

            hpMoveBar.localPosition = new Vector2(0, (specInfoSize.rect.height / 2) - (hpMoveBar.rect.height / 2));
            hpCurrentBar.localPosition = new Vector2(0, (specInfoSize.rect.height / 2) - (hpCurrentBar.rect.height / 2));

            setUserPanel.transform.Find("Commander").gameObject.SetActive(true);


            Button ultimateOrderBtn = ultimateSkillBtn.GetComponent<Button>();
            ultimateOrderBtn.onClick.AddListener(() => {
                setUser.setNextAction((int)Character.CharacterAction.Ultimate);
                setUser.ultimateSkillObj.orderFlag = true;
                Debug.Log(string.Format("{0} 에게 궁극기 사용 명령", (string)userInfo["name"]));
            });
        }// 플레이어가 지휘자 일경우 버튼 활성화
    }

    private void skillBtnSetting(Transform skillButton, Skill skillInfo) {
        Image skillIconBackground = skillButton.Find("Background").GetComponent<Image>();
        Image skillIcon = skillButton.Find("Bar").GetComponent<Image>();

        skillIconBackground.sprite = skillInfo.iconSprite;
        skillIcon.sprite = skillInfo.iconSprite;
    }

    private void userObjectSetting(GameObject setUser, tupleType userInfo) {
        Character user;
        RectTransform userRect;
        float positionX, positionY;
        int temp = 100;

        user = setUser.GetComponent<Character>();
        user.setting(userInfo, 1);

        userRect = setUser.GetComponent<RectTransform>();
        positionX = userStartPoint.x + (userRect.rect.width * (HeroGroup.transform.childCount + 1) + UIGAP);
        // Player Object 포함한 계산

        positionY = getTopviewGroundPositionY(setUser);

        userRect.localPosition = new Vector2(positionX, positionY);

        user.setSprite((string)userInfo["sprite"], new Vector2(150, 240));

        // dummy
        user.infomation.energyPower += 300;
        user.infomation.range += temp;
        user.infomation.holyPower = 10;

        user.direction = +1;
        temp += 100;
        positionX += userRect.rect.width;

        StatusBar headHpBar = setUser.transform.Find("HpBar").GetComponent<StatusBar>();
        headHpBar.init((float)userInfo["health_point"], new Color(255.0f, 0, 0));
        user.hpBar.Add(headHpBar);

        StatusBar headDelayBar = setUser.transform.Find("DelayBar").GetComponent<StatusBar>();
        headDelayBar.init(0, new Color(0, 0, 255.0f));
        user.delayBar.Add(headDelayBar);

        int thisPanelIndex = HeroGroup.transform.childCount - 1;
        user.destroyCallback = (() => {
            UserSpecGroup.transform.GetChild(thisPanelIndex).Find("DeadStatus").gameObject.SetActive(true);
            if ((HeroGroup.transform.childCount - 1) <= 0) {
                this.gameOver();
            }
        });
    }

    private void usersInit() {
        GameObject entryUser;
        GameObject userSpecPanel;
        tupleType userInfo;

        if (RoomState.users != null) {
            for (int index = 0; index < RoomState.users.Count; index++) {
                userInfo = db.getTuple("users", (int)RoomState.users[index]);
                entryUser = Instantiate(CharacterObject, HeroGroup.transform);
                userSpecPanel = Instantiate(UserSpecPanel, UserSpecGroup.transform);

                userObjectSetting(entryUser, userInfo);
                userPanelSetting(userSpecPanel, userInfo);
            }
        } // 파티에 유저가 있는지 체크
    } // 유저들 셋팅

    private void searchAlliance(Transform targetObj, Transform allianceGroup) {
        Character target = targetObj.GetComponent<Character>();
        float searchLowHP = -1;
        Character allianceInfo;

        foreach (Transform allianceObj in allianceGroup) {
            allianceInfo = allianceObj.GetComponent<Character>();

            if (searchLowHP == -1 || searchLowHP > allianceInfo.currentHealthPoint) {
                searchLowHP = allianceInfo.currentHealthPoint;
                target.aggroTarget = allianceObj.transform;
            }
        }

        target.action = (int)Character.CharacterAction.Battle;
    } // 체력이 가장 낮은 아군을 찾습니다.

    private void searchEnemy(Transform targetObj, Transform enemyGroup) {
        Character target = targetObj.GetComponent<Character>();
        Vector2 targetPosition = targetObj.position;

        float minDistance = -1;
        foreach (Transform enemyObj in enemyGroup) {
            //target.aggroCheck(enemyObj);

            Vector2 enemyPosition = enemyObj.position;

            float distance = Vector2.Distance(enemyPosition, targetPosition);
            /*
            if (target.aggroCheck(enemyObj) && (minDistance == -1 || minDistance > distance)) {
                minDistance = distance;
                target.target = enemyObj.gameObject;
            }

            if (minDistance != -1) {
                target.action = (int)Character.CharacterAction.Battle;
            }*/
        }
    } // 적을 찾습니다.

    private void playerTransform(int setDirection, int setStatus) {
        Character UserObj = Player.GetComponent<Character>();
        if (UserObj.action != (int)Character.CharacterAction.AfterDelay) {
            UserObj.direction = setDirection;
            UserObj.action = setStatus;
            UserObj.cancelCurrentBeforeDelay();
        }
    } // 플레이어의 이동방향과 상태를 바꿉니다.

    public void playerControlMoveStart (int setDirection) {
        CancelInvoke("playerAutoTransform");
        playerTransform(setDirection, (int)Character.CharacterAction.Control);
    } // 화살표 버튼을 누르기 시작합니다.

    public void playerControlMoveEnd() {
        playerTransform(1, (int)Character.CharacterAction.Wait);
        Invoke("playerAutoTransform", 1.0f);
    } // 이동 조작을 중지합니다.

    private void playerAutoTransform() {
        playerTransform(1, (int)Character.CharacterAction.Normal);
    } // 플레이어를 자동으로 전환합니다.

    private void basePattern(Transform targetObj, Transform allianceGroup, Transform enemyGroup) {
        Character target = targetObj.GetComponent<Character>();

        switch (target.action) {
            case (int)Character.CharacterAction.Normal:
                target.move();
                if (target.attackType == (int)Character.CharacterAttackType.Heal) {
                    searchAlliance(targetObj.transform, allianceGroup.transform);
                } else {
                    searchEnemy(targetObj.transform, enemyGroup.transform);
                }
                break;
            case (int)Character.CharacterAction.Battle:
                target.battle();
                break;
            case (int)Character.CharacterAction.BeforeDelay:
            case (int)Character.CharacterAction.AfterDelay:
            case (int)Character.CharacterAction.Attack:
                break;
            case (int)Character.CharacterAction.Control:
                target.move();
                break;
        }
    }

    private void backbroundTracking(Transform target) {
        if (target != null) {
            Vector3 targetPosition = target.position;
            float movementSpeed = target.gameObject.GetComponent<Character>().infomation.movementSpeed;
            int screenCenter = Screen.width / 2;

            if (targetPosition.x > screenCenter) {
                BackGround.transform.position = new Vector2(BackGround.transform.position.x - movementSpeed, BackGround.transform.position.y);
                target.position = new Vector2(screenCenter, targetPosition.y);

                foreach (Transform heroObj in HeroGroup.transform) {
                    heroObj.position = new Vector2(heroObj.position.x - movementSpeed, heroObj.position.y);

                    if (heroObj.position.x <= 0) {
                        heroObj.position = new Vector2(0, heroObj.position.y);
                    }
                }

                foreach (Transform enemyObj in EnemyGroup.transform) {
                    enemyObj.position = new Vector2(enemyObj.position.x - movementSpeed, enemyObj.position.y);
                }

                if (Mathf.Abs(BackGround.transform.position.x) % Screen.width == 0) {
                    map.backgroundBatch(BackGround, (-1 * BackGround.transform.localPosition.x) + Screen.width);
                    foreach (Transform backObject in BackGround.transform) {
                        if (backObject.localPosition.x < Mathf.Abs(BackGround.transform.localPosition.x) - (Screen.width)) {
                            Destroy(backObject.gameObject);
                        }
                    }
                }
            }
        }
    }

    private void FixedUpdate() {
        Transform maxPosXObj = null;

        foreach (Transform heroObj in HeroGroup.transform) {
            basePattern(heroObj.transform, HeroGroup.transform, EnemyGroup.transform);

            if (maxPosXObj == null || maxPosXObj.position.x < heroObj.position.x) {
                maxPosXObj = heroObj;
            }
        } // heros pattern

        foreach (Transform enemyObj in EnemyGroup.transform) {
            basePattern(enemyObj.transform, EnemyGroup.transform, HeroGroup.transform);
        } // enemys pattern

        backbroundTracking(maxPosXObj);
    }

    /* button onclick event */
    public void userTargetingBtn(GameObject targetObject) {
        Character UserObj = Player.GetComponent<Character>();

        //UserObj.target = targetObject;
    } // 유저를 타겟으로 설정합니다.

    public void activeUltimateSkill() {
        if (Player == null) {
            return;
        }

        BottomView.transform.Find("UltimateSkillButton").Find("CoolTimeBar").Find("Bar").GetComponent<Image>().color = new Color(0f, 174.0f, 255.0f);

        Character UserObj = Player.GetComponent<Character>();
        UserObj.setNextAction((int)Character.CharacterAction.Ultimate);
    }

    public void activeUserSubSkill1() {
        if (Player == null) {
            return;
        }

        BottomView.transform.Find("SubSkillButton1").Find("CoolTimeBar").Find("Bar").GetComponent<Image>().color = new Color(0f, 174.0f, 255.0f);

        Character UserObj = Player.GetComponent<Character>();
        UserObj.setNextAction((int)Character.CharacterAction.Skill1);
    }

    public void activeUserSubSkill2() {
        if (Player == null) {
            return;
        }

        BottomView.transform.Find("SubSkillButton2").Find("CoolTimeBar").Find("Bar").GetComponent<Image>().color = new Color(0f, 174.0f, 255.0f);

        Character UserObj = Player.GetComponent<Character>();
        UserObj.setNextAction((int)Character.CharacterAction.Skill2);
    }

    public void returnSceneBtn() {
        SceneManager.LoadScene("quest");
    }
}
