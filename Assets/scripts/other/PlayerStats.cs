using System.Collections;
using System.Collections.Generic;

public static class PlayerStats
{
    private static string eventFilePath; // 실행할 이벤트 스크립트

    public static string EventFilePath
    {
        get {
            return eventFilePath;
        }
        set {
            eventFilePath = value;
        }
    }

    private static Score globalScore = new Score();

    public static Score GlobalScore
    {
        get {
            return globalScore;
        }
    }
}
// 플레이어의 상태 및 업적을 저장하는 정적 클래스
