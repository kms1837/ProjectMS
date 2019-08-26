using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score {
    public Dictionary<int, int> killScore; // 몬스터 사냥 점수
    public Dictionary<int,int> skillScore;

    public Score () {
        killScore = new Dictionary<int, int>();
        skillScore = new Dictionary<int,int>();
    }
}
