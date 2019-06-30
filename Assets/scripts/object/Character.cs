using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using tupleType = System.Collections.Generic.Dictionary<string, object>;

[CustomEditor(typeof(Character))]
public class Character : MonoBehaviour
{
    public Ability infomation; // 케릭터 정보
    public int action; // 케릭터 행동(이동, 공격 등)
    public int status; // 케릭터 상태(화상, 빙결 등)
    public int type; // 케릭터 타입
    public int attackType;
    public int layer; // 케릭터가 있는 위치(0 - outdoor, 1 - indoor)
    public int groupNumber; // 케릭터 식별그룹(같은 아군끼리 공격방지)

    public bool isRun; // 달리는 중인지 체크
    public bool isJump; // 점프 중인지 체크
    public bool isGround; // 공중에 떠있는 상태인지 체크
    public bool isHangOn; // 매달려 있는 상태인지 체크
    private bool isSlide; // 슬라이딩 중인지 체크

    public float currentHealthPoint; // HP
    public float currentManaPoint; // MP
    private float currentJumpPoint;
    private int jumpCount; // 현재 몇번 점프중인지 체크

    public Transform aggroTarget; // 공격대상
    public float direction; // 방향
    private int originDirection; // 평소 이동 방향

    public enum CharacterAction { Normal, Battle, Attack,  Event, Control, Wait, BeforeDelay, AfterDelay, Constraint, Ultimate, Skill }
    // 평상시, 전투모드, 공격중, 이벤트중, 조작중, 대기중, 선딜레이중, 후딜레이중, 행동제약, 궁극기사용중, 스킬 사용중

    public enum CharacterType { Hero, NPC, Monster, Boss }
    public enum CharacterAttackType { Attack, Heal }

    private string beforeDelayActionStr; // 선딜레이 중인 함수 이름(invoke 중인 상태)

    public Ability[] equipments; // 장비

    public ArrayList buffList; // 버프, 디버프 리스트

    // Skill Objects
    public Skill ultimateSkillObj;
    public Skill subSkillObj1;
    public Skill subSkillObj2;

    // ui
    public ArrayList hpBar; // hpbar ui 목록
    public ArrayList delayBar; // hpbar ui 목록

    public UnityEngine.Events.UnityAction destroyCallback; // 케릭터 사망시 콜백 설정

    Color AfterDelayColor = new Color(255.0f, 150.0f, 0.0f);
    Color BeforeDelayColor = new Color(0.0f, 0.0f, 255.0f);

    private const float baseWidth = 150;

    // AI Support
    private int nextAction; // 다음상태 예약
    private int prevAction; // 전 상태

    // object
    private Rigidbody2D rigidbody;
    private BoxCollider2D characterCollider;
    private Animator animator;

    void Awake() {
        Vector2 objPosition = this.transform.position;
        Rect objRect = this.gameObject.GetComponent<RectTransform>().rect;

        rigidbody = this.GetComponent<Rigidbody2D>();
        characterCollider = this.GetComponent<BoxCollider2D>();
        animator = this.GetComponent<Animator>();

        infomation = new Ability();

        this.action = (int)CharacterAction.Normal;

        aggroTarget = null;

        // base setting
        infomation.movementSpeed = 5f;
        infomation.runSpeed = 5f;
        infomation.jumpPower = 15.5f;
        infomation.maxJump = 2;
        direction = 1;
        infomation.power = 10;
        infomation.beforeDelay = 2.0f;
        infomation.afterDelay = 2.0f;
        infomation.energyPower = 5;
        infomation.healthPoint = 100;
        infomation.manaPoint = 50;
        currentHealthPoint = infomation.healthPoint;
        currentManaPoint = infomation.manaPoint;

        infomation.aggroRange = objRect.width;
        infomation.range = objRect.width * 2;

        beforeDelayActionStr = string.Empty;

        hpBar = new ArrayList();
        delayBar = new ArrayList();

        buffList = new ArrayList();
        equipments = new Ability[5];

        for (int i = 0; i < 5; i++) {
            equipments[i] = new Ability();
        }

        jumpClear();
    }

    private void updateUI() {
        Vector2 objPosition = this.transform.position;

        try {
            foreach (StatusBar bar in hpBar) {
                bar.setCurrent(currentHealthPoint);
            }
        } catch (NullReferenceException err) {
            Debug.Log(err);
        }
    }

    void Update() {
        updateUI();
    }

    public void setSprite(string filePath) {
        Transform spriteObject = this.transform.Find("Sprite");
        SpriteRenderer setSprite = spriteObject.GetComponent<SpriteRenderer>();
        Sprite loadSprite = Resources.Load<Sprite>(filePath);
        RectTransform setSize = this.transform.Find("Sprite").GetComponent<RectTransform>();

        float setHeight = loadSprite.rect.height * (baseWidth / loadSprite.rect.width);

        setSprite.sprite = loadSprite;
        setSize.sizeDelta = new Vector2(baseWidth, setHeight);
    }

    public void setSprite(string filePath, Vector2 size) {
        setSprite(filePath);

        RectTransform setSize = this.transform.Find("Sprite").GetComponent<RectTransform>();
        setSize.sizeDelta = size;
    }

    public void setting(tupleType settingData, int inDirection) {
        this.attackType = (int)settingData["attack_type"];

        this.infomation.id = (int)settingData["id"];
        this.infomation.nameStr = (string)settingData["name"];

        this.infomation.healthPoint = (float)settingData["health_point"];
        this.currentHealthPoint = (float)settingData["health_point"];

        this.infomation.beforeDelay = (float)settingData["before_delay"];
        this.infomation.afterDelay = (float)settingData["after_delay"];

        this.infomation.power = (float)settingData["power"];

        this.infomation.movementSpeed = (float)settingData["movement_speed"];

        direction = inDirection;
        originDirection = inDirection;
    }
    
    public void attack() {
        if (actionCheck() || isSlide) {
            return;
        }

        prevAction = this.action;

        clearAction();

        this.action = (int)Character.CharacterAction.Attack;
        animator.SetBool("Attack", true);
        Invoke("clearToBackAction", 0.4f);

        /*
        if (targetCheck()) {
            return;
        }

        Vector2 currentPosition = this.gameObject.transform.position;
        float distance = Vector2.Distance(aggroTarget.transform.position, currentPosition);
        */
        /*
        if (infomation.range >= distance) {
            Character attackTarget = target.GetComponent<Character>();
            float damage = infomation.power;

            if (attackType == (int)CharacterAttackType.Heal) {
                attackTarget.heal(this.infomation.power);
                if (attackTarget.currentHealthPoint >= attackTarget.infomation.healthPoint) {
                    target = null;
                } // 체력이 가득차서 타겟을 변경함

            } else {
                foreach (Skill buff in buffList) {
                    damage += buff.infomation.energyPower;
                }

                foreach (Ability equipment in equipments) {
                    damage += equipment.energyPower;
                }

                attackTarget.hit(damage);
            }
        } // 공격 성공
        */
        //runAfterDelay();
    }

    public void setNextAction(int inNextAction) {
        this.nextAction = inNextAction;
    }

    private void runNextAction() {
        /*
        switch (this.nextAction) {
            case (int)CharacterAction.Ultimate:
                this.activeUltimateSkill();
                break;
            case (int)CharacterAction.Skill1:
                this.activeSubSkill1();
                break;
            case (int)CharacterAction.Skill2:
                this.activeSubSkill2();
                break;
        }*/

        this.action = nextAction;
    } // 다음 행동을 행함

    private void backAction() {
        this.action = prevAction;
    }

    private void runBeforeDelay() {
        float finalDelay = this.infomation.beforeDelay;
        foreach (Skill buff in buffList) {
            finalDelay += buff.infomation.beforeDelay;
        } // 버프, 디버프로 인한 딜레이 추가
        delay(finalDelay, (int)Character.CharacterAction.BeforeDelay, BeforeDelayColor, beforeDelayActionStr);
    }

    private void runAfterDelay() {
        float finalDelay = this.infomation.afterDelay;
        foreach (Skill buff in buffList) {
            finalDelay += buff.infomation.afterDelay;
        } // 버프, 디버프로 인한 딜레이 추가
        delay(infomation.afterDelay, (int)Character.CharacterAction.AfterDelay, AfterDelayColor, "runNextAction");
    }

    private void delay(float time, int setStatus, Color delayColor, string callBack) {
        foreach (StatusBar bar in delayBar) {
            bar.setMaximum(time);
            bar.setColor(delayColor);
            bar.runProgress();
        }

        this.action = setStatus;

        Invoke(callBack, time);
    }

    public void cancelCurrentBeforeDelay() {
        if (beforeDelayActionStr != string.Empty) {
            foreach (StatusBar bar in delayBar) {
                bar.stopProgress();
            }

            CancelInvoke(beforeDelayActionStr);
            beforeDelayActionStr = string.Empty;
        }
    }

    public void heal(float healPower) {
        this.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = new Color(0, 255, 0);
        Invoke("colorClear", 0.1f);

        this.currentHealthPoint = this.currentHealthPoint >= this.infomation.healthPoint ? this.infomation.healthPoint : this.currentHealthPoint + healPower;
    } // 회복받음

    public void hit(float damage, float attackDirection) {
        this.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
        Invoke("colorClear", 0.1f);

        cancelCurrentBeforeDelay();

        currentHealthPoint -= damage;

        this.action = (int)Character.CharacterAction.Constraint;
        Invoke("clearAction", 0.5f);
        // addForce가 velocity와 곂치면 x축으로 힘을 받지않는 문제가 있어 행동 제약을 둠

        if (currentHealthPoint <= 0) {
            dead();
        }
        
        rigidbody.velocity = new Vector2(0, 0);
        rigidbody.AddForce(new Vector2(attackDirection, 1) * this.infomation.movementSpeed, ForceMode2D.Impulse);
    } // 공격받음

    private void dead() {
        if (destroyCallback != null) {
            destroyCallback();
        }

        Destroy(this.gameObject);
    }

    private void colorClear() {
        this.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
    }

    private bool targetCheck() {
        bool nullCheck = (aggroTarget == null);
        if (nullCheck && (action != (int)CharacterAction.Control)) {
            this.action = (int)CharacterAction.Normal;
            direction = originDirection;
        }

        // 타켓이 없을때 유저 직접 컨트롤 상태이면 유지하고 컨트롤 상태가 아니면 일반 상태로 돌린다.

        return nullCheck;
    } // 타겟이 있는지 없는지 확인 없으면 일반상태로 바꾼다.

    public void move() {
        if (actionCheck() || isSlide) {
            return;
        }

        prevAction = this.action;
        
        Vector2 currentPosition = this.transform.position;
        float spriteDirection = this.direction > 0 ? 1 : -1;
        this.transform.localScale = new Vector3(spriteDirection, 1, 1);
        float totalSpeed = infomation.movementSpeed;

        if (isRun) {
           totalSpeed += infomation.runSpeed;
        }

        animator.SetBool("Walk", !isJump);

        rigidbody.velocity = new Vector2(this.direction * totalSpeed, rigidbody.velocity.y);

        backAction();
        // * Time.deltaTime - 프레임차이 문제인데 너무 속도에 영향을줌
    } // 이동

    public void hangOnWall(Collision2D collisionWall) {
        if (transform.position.y > collisionWall.contacts[0].point.y) {
            isHangOn = true;
            rigidbody.gravityScale = 0;
            transform.position = new Vector2(transform.position.x, collisionWall.contacts[0].point.y);

            jumpClear();
        };
    } // 벽에 매달릴수 있는지 체크하고 벽에 매달림

    public void falling() {
        if (!groundCheck()) {
            animator.SetBool("Jump", true);
            animator.SetBool("Walk", false);
        }
        /* 
         isGround 상태 체크를 충돌시에 한 이유는 최적화와 얇게 점프시 ray길이 문제로 바닥에 닿은 것으로 판단해 다중 점프가 가능해지는 문제가 발견됨
         (ray길이는 어느정도 길게 해줘야 정확도가 높아짐)

         */
    } // 추락상태인지 확인하고 애니메이션 재생

    private bool groundCheck() {
        float bottom = transform.position.y - characterCollider.bounds.extents.y;
        RaycastHit2D rayObject = Physics2D.Raycast(new Vector2(transform.position.x, bottom), Vector2.down, 1.0f, LayerMask.GetMask("Platform"));
        Debug.DrawLine(new Vector2(transform.position.x, bottom), new Vector2(transform.position.x, bottom - 1.0f));

        return rayObject.transform != null ? true : false;
    }

    public void jumpEnd() {
        currentJumpPoint = 0;
        CancelInvoke("jumpHolding");
    }

    private void jumpHolding() {
        currentJumpPoint += 1.0f;
        rigidbody.AddForce(Vector2.up * 1.0f, ForceMode2D.Impulse);
        

        if (currentJumpPoint >= infomation.jumpPower) {
            jumpEnd();
        }
    }

    private void jumpClear() {
        isJump = false;
        jumpCount = 0;
        animator.SetBool("Jump", false);
    }

    public void jump() {
        if (actionCheck() || isSlide) {
            return;
        }
        
        if (!isJump) {
            if (isHangOn) {
                rigidbody.gravityScale = 1;
                isHangOn = false;
                Debug.Log("매달리기 해제");
            }
        }

        if (!isJump || jumpCount < infomation.maxJump) {
            jumpCount++;

            isJump = true;
            CancelInvoke("jumpHolding");

            clearAction();
            InvokeRepeating("jumpHolding", 0.0f, 0.01f);
        }
    }

    public void slide() {
        if (actionCheck()) {
            return;
        }

        clearAction();

        this.action = (int)Character.CharacterAction.Constraint;
        Invoke("clearAction", 0.5f);

        rigidbody.velocity = new Vector2(0, 0);
        rigidbody.AddForce(new Vector2(this.transform.localScale.x, 0) * this.infomation.movementSpeed * 2, ForceMode2D.Impulse);

        animator.SetBool("Slide", true);
        isSlide = true;
    }

    public void idle() {
        if (!isJump && !isSlide && !isHangOn) {
            clearAction();
        }
    } // 대기 상태인지 체크하고 애니메이션을 초기화함

    private void clearAction() {
        float bottom = transform.position.y - characterCollider.bounds.extents.y;
        RaycastHit2D rayObject = Physics2D.Raycast(new Vector2(transform.position.x, bottom), Vector2.up, 1.0f, LayerMask.GetMask("Platform"));
        Debug.DrawLine(new Vector2(transform.position.x, bottom), new Vector2(transform.position.x, bottom + 1.0f));
        // 슬라이드중 벽 사이에 끼어있는지 체크

        bool clearFlag = true;

        if (rayObject.transform != null) {
            clearFlag = false;
        }// 슬라이드중 벽에 끼어있으므로 슬라이드 유지

        if (clearFlag) {
            animator.SetBool("Slide", false);
            isSlide = false;
        }

        animator.SetBool("Jump", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Walk", false);

        this.action = (int)Character.CharacterAction.Normal;
    } // 케릭터 행동상태를 기본으로 초기화시킴

    private void clearToBackAction() {
        clearAction();
        backAction();
    } // 케릭터 행동상태를 초기화 시키고 이전 행동으로 돌아감

    public void battle() {
        if (targetCheck()) {
            return;
        }

        Vector2 currentPosition = this.gameObject.transform.position;

        float distance = Vector2.Distance(aggroTarget.transform.position, currentPosition);

        if (this.infomation.range >= distance) {
            this.prevAction = action;
            this.action = (int)CharacterAction.Attack;
            beforeDelayActionStr = "attack";

            foreach (StatusBar bar in delayBar) {
                bar.setMaximum(infomation.beforeDelay);
            }

            CancelInvoke(beforeDelayActionStr);
            runBeforeDelay();
        } else {
            float temp = this.gameObject.transform.position.x - aggroTarget.transform.position.x;
            direction = temp >= 0 ? -1 : +1;

            this.move();
        }

        if (this.infomation.aggroRange < distance) {
            aggroTarget = null;
            action = (int)CharacterAction.Normal;
        } // 어그로 해제
    } // 행동

    private bool actionCheck() {
        bool result = (this.action == (int)Character.CharacterAction.AfterDelay ||
                        this.action == (int)Character.CharacterAction.BeforeDelay ||
                        this.action == (int)Character.CharacterAction.Constraint ||
                        this.action == (int)Character.CharacterAction.Attack);

        return result;
    } // 행동이 가능한지 체크하고 행동이 불가능하면 이전 행동으로 되돌림

    public bool activeUltimateSkill() {
        if (actionCheck()) {
            return false;
        }

        return ultimateSkillObj.activation();
    } // 메인 스킬 발동

    public bool activeSubSkill1() {
        if (actionCheck()) {
            return false;
        }

        return subSkillObj1.activation();
    } // 서브 스킬1 발동

    public bool activeSubSkill2() {
        if (actionCheck()) {
            return false;
        }

        return subSkillObj2.activation();
    } // 서브 스킬2 발동

    public void setAggroTarget(Transform target) {
        if (this.action == (int)Character.CharacterAction.Normal) {
            aggroTarget = target;
        }
    } // 케릭터가 바라보는 대상을 변경합니다.

    public void forceAggroTarge(Transform target) {
        aggroTarget = target;
    } // 강제로 바라보는 대상을 변경합니다.

    private void OnCollisionEnter2D (Collision2D collision) {
        Character collisionObj = collision.transform.GetComponent<Character>();

        if (collision.transform.tag == "Character" && groupNumber != collisionObj.groupNumber) {
            float damage = collisionObj.infomation.power;
            this.hit(damage, this.transform.localScale.x * -1);
        } // 같은그룹 공격 불가 판정

        if (collision.transform.tag == "Platform") {
            isGround = groundCheck();
            if (isGround) {
                jumpClear();
            }
        }
    }// 서로 충돌시

    private void OnTriggerEnter2D (Collider2D collision) {
        Character collisionObj = collision.transform.parent.GetComponent<Character>();

        if (collision.tag == "Hitbox" && groupNumber != collisionObj.groupNumber) {
            float damage = collision.transform.parent.GetComponent<Character>().infomation.power;
            this.hit(damage, collision.transform.parent.localScale.x);
        } // 같은그룹 공격 불가 판정
    }// 히트박스에 충돌
}
