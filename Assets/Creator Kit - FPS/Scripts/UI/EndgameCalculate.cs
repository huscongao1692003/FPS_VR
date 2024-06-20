
using UnityEngine;
using UnityEngine.UI;

public class EndgameCalculate : MonoBehaviour
{
    

    public Text TargetDestroyed;
    public Text Penalty;
    public Text FinalScore;

 

    public void Update()
    {
        gameObject.SetActive(true);

        int targetDestroyed = GameSystem.Instance.DestroyedTarget;
        int totalTarget = GameSystem.Instance.TargetCount;
        int missedTarget = totalTarget - targetDestroyed;
        float penaltyAmount = GameSystem.Instance.TargetMissedPenalty * missedTarget;

        TargetDestroyed.text = targetDestroyed + "/" + totalTarget;
     
        Penalty.text = missedTarget + "*" + GameSystem.Instance.TargetMissedPenalty.ToString("N2") + "s = " + penaltyAmount.ToString("N2") + "s";
     

        FinalScore.text = GameSystem.Instance.Score.ToString("N");
    }
}
