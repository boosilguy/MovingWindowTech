using UnityEngine;

public class EmotionalCategorySelector : MonoBehaviour
{
    [Header("Experiment Manager Script")]
    public ExperimentManager experimentManager;

    /// <summary>
    /// 정서 분류에서 Angry 버튼 눌렀을 때, 호출되는 함수.
    /// </summary>
    public void GetAngryButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Emotion(EmotionCategory.Angry));
    }

    /// <summary>
    /// 정서 분류에서 Disgusted 버튼 눌렀을 때, 호출되는 함수.
    /// </summary>
    public void GetDisgustButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Emotion(EmotionCategory.Disgust));
    }

    /// <summary>
    /// 정서 분류에서 Fearful 버튼 눌렀을 때, 호출되는 함수.
    /// </summary>
    public void GetFearButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Emotion(EmotionCategory.Fear));
    }

    /// <summary>
    /// 정서 분류에서 Surprised 버튼 눌렀을 때, 호출되는 함수.
    /// </summary>
    public void GetSurpriseButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Emotion(EmotionCategory.Surprise));
    }

    /// <summary>
    /// 정서 분류에서 Happy 버튼 눌렀을 때, 호출되는 함수.
    /// </summary>
    public void GetHappinessButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Emotion(EmotionCategory.Happiness));
    }

    /// <summary>
    /// 정서 분류에서 Sad 버튼 눌렀을 때, 호출되는 함수.
    /// </summary>
    public void GetSadButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Emotion(EmotionCategory.Sad));
    }
}
