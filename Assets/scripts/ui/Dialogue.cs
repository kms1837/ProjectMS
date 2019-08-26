using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
class ScriptObject
{
    public List<DialogueObject> dialogue;
} // 전체 대화 스크립트 정보

[System.Serializable]
class DialogueObject
{
    public int type;
    public string name;
    public string message;
    public string leftImg;
    public string rightImg;
    public List<AnswerObject> answer;
    public string load; // 대화 종료후 다음 대화 스크립트를 설정합니다, 비어있으면 현행유지
} // 대화 정보

[System.Serializable]
class AnswerObject
{
    public int type;
    public string text;
    public int next;
    public string load;
} // 선택지 정보

public class Dialogue : MonoBehaviour {
    public GameObject answerButton;
    public AudioClip typingSound;
    private AudioSource sound;
    private Transform answerButtonGroup;

    private Character eventTarget; // 이벤트 대상
    private Npc eventNpc; // 해당 대화의 NPC
    private ScriptObject scriptData;
    private DialogueObject currentDialogue;
    private int currentDialogueIndex;
    private int typingPoint;
    private bool keyReady;

    private GameObject leftImage;
    private GameObject rightImage;
    private Transform messagePanel;
    private Text messageLabel;
    private Text nameLabel;

    const float typingSpeed = 0.1f;
    const int minTyping = 5; // 어느정도 타이핑되야 스킵이 가능한지 설정
    const string dirPath = "json/dialogue/";
    const string imgDirPath = "imgs/";

    public enum DialogueType { Normal, Question, End, Jump, Load };
    // Normal - 행동키를 누르면 다음으로 넘어감, question - 질문을 선택해야 다음으로 넘어감, End - 행동키를 누르면 대화가 종료됨, Jump - 해당인덱스 대화로 점프함, Load - 다른 파일의 대화를 불러옴
    public enum AnswerType { Next };
    // Next - 해당 인덱스의 대화로 이동함

    void Start () {
        messagePanel = this.transform.Find("MessagePanel");
        messageLabel = messagePanel.Find("MessageLabel").GetComponent<Text>();
        nameLabel = messagePanel.Find("NameLabel").GetComponent<Text>();
        answerButtonGroup = this.transform.Find("AnswerButtonGroup");
        sound = this.GetComponent<AudioSource>();

        leftImage = this.transform.Find("LeftImage").gameObject;
        rightImage = this.transform.Find("RightImage").gameObject;
    }
	
	void Update () {
        if ((Input.GetButtonUp("Action") || Input.GetMouseButtonDown(0)) && keyReady) {
            if (typingPoint < currentDialogue.message.Length) {
                typingEnd();
            }
            else {
                dialogueAction();
            }
        }
    }

    private void dialogueEvent() {
        switch (currentDialogue.type) {
            case (int)Dialogue.DialogueType.Question:
                setQuestion();
                break;
        }
    } // 타입에 따른 이벤트를 배치합니다.

    private void dialogueAction() {
        switch (currentDialogue.type) {
            case (int)Dialogue.DialogueType.Normal:
                currentDialogueIndex = currentDialogueIndex + 1;
                if (currentDialogueIndex < scriptData.dialogue.Count) {
                    setDialogue(currentDialogueIndex);
                } else {
                    SceneManager.LoadScene("battle_ground");
                    //스크립트의 종료
                }
                break;
            case (int)Dialogue.DialogueType.Jump:
                break;
            case (int)Dialogue.DialogueType.Load:
                SceneManager.LoadScene(scriptData.dialogue[currentDialogueIndex].load);
                break;
            case (int)Dialogue.DialogueType.End:
                endDialogue();
                break;
        }
    }

    public void startDialogue(string fileName) {
        if (messageLabel == null) {
            Start();
            keyReady = false;
        } // 비활성화 상태에서 대화시 실패 방지

        loadJSon(fileName);
        setDialogue(0);
    }

    public void startDialogue(string fileName, Character inEventTarget, Npc inEventNpc) {
        if (messageLabel == null) {
            Start();
            keyReady = false;
        }

        eventTarget = inEventTarget;
        eventNpc = inEventNpc;
        loadJSon(fileName);
        setDialogue(0);
    }

    public void endDialogue() {
        if (eventTarget != null) {
            eventTarget.action = (int)Character.CharacterAction.Normal;
        }

        if (currentDialogue.load != null && currentDialogue.load.Length > 0) {
            eventNpc.dialogueFileName = currentDialogue.load;
        }

        answerButtonGroupClear();

        this.gameObject.SetActive(false);
    }

    private void loadJSon(string fileName) {
        try {
            TextAsset jsonText = Resources.Load(dirPath + fileName) as TextAsset;
            scriptData = JsonUtility.FromJson<ScriptObject>(jsonText.text);
        }
        catch (System.Exception e) {
            Debug.Log(e);
            Debug.Log("대화를 불러들이지 못했습니다.");
        }
    }

    private void setDialogue(int setIndex) {
        keyReady = false;

        try {
            currentDialogue = scriptData.dialogue[setIndex];
            currentDialogueIndex = setIndex;

            if (currentDialogue.rightImg != null) {
                rightImage.SetActive(true);
                rightImage.GetComponent<Image>().sprite = Resources.Load<Sprite>(imgDirPath + currentDialogue.rightImg);
            }
            else {
                rightImage.SetActive(false);
            }

            if (currentDialogue.leftImg != null) {
                leftImage.SetActive(true);
                leftImage.GetComponent<Image>().sprite = Resources.Load<Sprite>(imgDirPath + currentDialogue.leftImg);
            }
            else {
                leftImage.SetActive(false);
            }

            runTyping();
        }
        catch (System.Exception e) {
            Debug.Log(e);
            Debug.Log("대화 실패");
            endDialogue();
        }
    }
    
    private void runTyping() {
        typingPoint = 0;
        nameLabel.text = currentDialogue.name;
        messageLabel.text = "";

        InvokeRepeating("typing", typingSpeed, typingSpeed);
    }
    
    private void typing() {
        if (typingPoint >= currentDialogue.message.Length) {
            typingEnd();
            return;
        }

        if (minTyping >= typingPoint) {
            keyReady = true;
        }

        messageLabel.text += currentDialogue.message[typingPoint];
        typingPoint++;

        sound.PlayOneShot(typingSound);
    }

    private void typingEnd() {
        CancelInvoke("typing");
        typingPoint = currentDialogue.message.Length;
        messageLabel.text = currentDialogue.message;
        keyReady = true;

        dialogueEvent();
    }

    private void setQuestion() {
        Rect panelRect = messagePanel.GetComponent<RectTransform>().rect;
        Rect buttonRect = answerButton.GetComponent<RectTransform>().rect;
        float buttonMargin = 10.0f; // 버튼간 간격
        float panelMargin = buttonRect.height; // 버튼그룹과 대화패널간 간격

        float buttonGroupHeight = ((buttonRect.height + buttonMargin) * (currentDialogue.answer.Count-1));
        float baseX = panelRect.xMax - (buttonRect.width / 2);
        float baseY = messagePanel.localPosition.y + (panelRect.height/2) + panelMargin + buttonGroupHeight;

        float currentY = baseY;

        foreach (AnswerObject answer in currentDialogue.answer) {
            Transform newAnswerButton = Instantiate(answerButton, answerButtonGroup).transform;
            newAnswerButton.localPosition = new Vector2(baseX, currentY);
            currentY -= buttonRect.height + buttonMargin;

            newAnswerButton.Find("Label").GetComponent<Text>().text = answer.text;

            newAnswerButton.GetComponent<Button>().onClick.AddListener(() => { selectAnswer(answer); });
        }
    }

    private void selectAnswer(AnswerObject answerInfo) {
        switch (answerInfo.type) {
            case (int)Dialogue.AnswerType.Next:
                setDialogue(answerInfo.next);
                break;
        }

        answerButtonGroupClear();
    }

    private void answerButtonGroupClear() {
        foreach (Transform answerButton in answerButtonGroup.transform) {
            Destroy(answerButton.gameObject);
        };
    }
}
