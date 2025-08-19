using UnityEngine;
using UnityEngine.UI;
public class MicroGameController2D : MonoBehaviour
{
    public float roundTime = 25f; float t; public Text timerText;
    void Start() { t = roundTime; RuleApplier2D.ApplyAll(); }
    void Update()
    {
        t -= Time.deltaTime; if (timerText) timerText.text = Mathf.CeilToInt(t).ToString();
        if (t <= 0f) GameStateManager.I.OnRoundEnd(0);
    }
}
