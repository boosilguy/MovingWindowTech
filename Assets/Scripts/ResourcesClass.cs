using System.Collections.Generic;
using UnityEngine;

public enum BlockState {Exp1P, Exp1PMW, Exp1, Exp2P, Exp2};
public enum EmotionCategory {Null, Sad, Happiness, Angry, Disgust, Fear, Surprise};
public enum ValenceCategory {Null, Positive, Nagative};
public enum Gender {Null, Female, Male}

/***************************************************/
/*                  블록 클래스                     */
/***************************************************/
public class Block
{
    public StimuliSet stimuliSet;

    BlockState state;
    
    /// <summary>
    /// Block 클래스 생성자.
    /// </summary>
    /// <param name="state">현재 블록 컨디션</param>
    public Block(BlockState state) {
        this.state = state;
    }

    /// <summary>
    /// Block 객체의 컨디션을 반환하는 Getter.
    /// </summary>
    public BlockState GetState() {
        return this.state;
    }
}

/***************************************************/
/*                 자극 세트 클래스                 */
/***************************************************/
public class StimuliSet
{
    public Queue<Emotion> set;
    public Queue<Valence> valSubSet;
    public int length;
    public int valSubLength;

    public StimuliSet (Queue<Emotion> set)
    {
        this.set = set;
        this.length = set.Count;
    }

    public StimuliSet (Queue<Emotion> set, Queue<Valence> valSubSet)
    {
        this.set = set;
        this.length = set.Count;

        this.valSubSet = valSubSet;
        this.valSubLength = valSubSet.Count;
    }
}

/***************************************************/
/*                 감정 클래스                      */
/***************************************************/
public class Emotion {
    // 성별용 상수키워드 (파일 명명이 변경될 시, 해당 영역 수정).
    const string FEMALE = "Female";
    const string MALE = "Male";

    // 감정용 상수키워드 (파일 명명이 변경될 시, 해당 영역 수정).
    const string SAD = "Sad";
    const string HAPPINESS = "Happy";
    const string ANGRY = "Angry";
    const string DISGUST = "Disgusted";
    const string FEAR = "Fearful";
    const string SURPRISE = "Surprised";

    public Gender gender;
    public Vector2 eyePixCoordinate1 = new Vector2(0, 0);
    public Vector2 eyePixCoordinate2 = new Vector2(0, 0);
    public Vector2 mouthPixCoordinate1 = new Vector2(0, 0);
    public Vector2 mouthPixCoordinate2 = new Vector2(0, 0);

    int identity;
    public string actorIdentity;
    public EmotionCategory emotionCategory;
    public string imgKeyword;
    public string rawKeyword = "-";

    public Emotion (EmotionCategory emotionCategory)
    {
        this.emotionCategory = emotionCategory;
        this.imgKeyword = "-";
    }

    /// <summary>
    /// 정서 클래스 생성자.
    /// </summary>
    /// <param name="gender">성별</param>
    /// <param name="identity">숫자 ID</param>
    /// <param name="actorIdentity">배우 ID</param>
    /// <param name="emotionCategory">정서 종류</param>
    public Emotion (Gender gender, int identity, string actorIdentity, EmotionCategory emotionCategory)
    {
        this.gender = gender;
        this.identity = identity;
        this.emotionCategory = emotionCategory;
        this.actorIdentity = actorIdentity;
        
        // 감정 자극 이미지 키워드 준비
        // 만약 해당 감정 자극 이미지가 여성이라면,
        if (this.gender == Gender.Female)
        {
            // Sad 감정일 경우,
            if (this.emotionCategory == EmotionCategory.Sad)
                this.imgKeyword = SAD + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Happiness 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Happiness)
                this.imgKeyword = HAPPINESS + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Angry 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Angry)
                this.imgKeyword = ANGRY + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Disgust 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Disgust)
                this.imgKeyword = DISGUST + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Fear 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Fear)
                this.imgKeyword = FEAR + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // 그 밖의 감정 (Surprise)일 경우,
            else
                this.imgKeyword = SURPRISE + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
        }
        // 만약 해당 감정 자극 이미지가 남성이라면,
        else
        {
            // Sad 감정일 경우,
            if (this.emotionCategory == EmotionCategory.Sad)
                this.imgKeyword = SAD + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Happiness 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Happiness)
                this.imgKeyword = HAPPINESS + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Angry 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Angry)
                this.imgKeyword = ANGRY + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Disgust 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Disgust)
                this.imgKeyword = DISGUST + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Fear 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Fear)
                this.imgKeyword = FEAR + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // 그 밖의 감정 (Surprise)일 경우,
            else
                this.imgKeyword = SURPRISE + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
        }

        this.rawKeyword = this.imgKeyword;
    }

    public Emotion (Gender gender, int identity, string actorIdentity, EmotionCategory emotionCategory, Vector2 e1, Vector2 e2, Vector2 m1, Vector2 m2, string rawKeyword)
    {
        this.gender = gender;
        this.identity = identity;
        this.emotionCategory = emotionCategory;
        this.actorIdentity = actorIdentity;
        this.eyePixCoordinate1 = e1;
        this.eyePixCoordinate2 = e2;
        this.mouthPixCoordinate1 = m1;
        this.mouthPixCoordinate2 = m2;
        this.rawKeyword = rawKeyword;
        
        // 감정 자극 이미지 키워드 준비
        // 만약 해당 감정 자극 이미지가 여성이라면,
        if (this.gender == Gender.Female)
        {
            // Sad 감정일 경우,
            if (this.emotionCategory == EmotionCategory.Sad)
                this.imgKeyword = SAD + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Happiness 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Happiness)
                this.imgKeyword = HAPPINESS + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Angry 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Angry)
                this.imgKeyword = ANGRY + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Disgust 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Disgust)
                this.imgKeyword = DISGUST + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Fear 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Fear)
                this.imgKeyword = FEAR + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // 그 밖의 감정 (Surprise)일 경우,
            else
                this.imgKeyword = SURPRISE + "_" + FEMALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
        }
        // 만약 해당 감정 자극 이미지가 남성이라면,
        else
        {
            // Sad 감정일 경우,
            if (this.emotionCategory == EmotionCategory.Sad)
                this.imgKeyword = SAD + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Happiness 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Happiness)
                this.imgKeyword = HAPPINESS + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Angry 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Angry)
                this.imgKeyword = ANGRY + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Disgust 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Disgust)
                this.imgKeyword = DISGUST + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // Fear 감정일 경우,
            else if (this.emotionCategory == EmotionCategory.Fear)
                this.imgKeyword = FEAR + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
            // 그 밖의 감정 (Surprise)일 경우,
            else
                this.imgKeyword = SURPRISE + "_" + MALE + "_" + identity.ToString("D2") + "_" + actorIdentity;
        }

    }
}

/***************************************************/
/*                 Valence 클래스                   */
/***************************************************/
public class Valence {
    // 성별용 상수키워드 (파일 명명이 변경될 시, 해당 영역 수정).
    const string FEMALE = "Female";
    const string MALE = "Male";

    // 감정용 상수키워드 (파일 명명이 변경될 시, 해당 영역 수정).
    const string NAGATIVE = "Negative";
    const string POSITIVE = "Positive";

    public Gender gender;
    public int identity;
    public ValenceCategory valenceCategory;
    public string imgKeyword;

    public Valence (ValenceCategory valenceCategory)
    {
        this.valenceCategory = valenceCategory;
        this.imgKeyword = "-";
    }

    /// <summary>
    /// Valence 클래스 생성자.
    /// </summary>
    /// <param name="gender">성별</param>
    /// <param name="identity">숫자 ID</param>
    /// <param name="emotionCategory">Valence 종류</param>
    public Valence (Gender gender, int identity, ValenceCategory valenceCategory)
    {
        this.gender = gender;
        this.identity = identity;
        this.valenceCategory = valenceCategory;
        
        // 감정 자극 이미지 키워드 준비
        // 만약 해당 감정 자극 이미지가 여성이라면,
        if (this.gender == Gender.Female)
        {
            // 부정적일 경우,
            if (this.valenceCategory == ValenceCategory.Nagative)
                this.imgKeyword = NAGATIVE + "_" + FEMALE + "_" + identity.ToString("D2");
            else
                this.imgKeyword = POSITIVE + "_" + FEMALE + "_" + identity.ToString("D2");
        }
        // 만약 해당 감정 자극 이미지가 남성이라면,
        else
        {
            // 부정적일 경우,
            if (this.valenceCategory == ValenceCategory.Nagative)
                this.imgKeyword = NAGATIVE + "_" + MALE + "_" + identity.ToString("D2");
            else
                this.imgKeyword = POSITIVE + "_" + MALE + "_" + identity.ToString("D2");
        }
    }
}