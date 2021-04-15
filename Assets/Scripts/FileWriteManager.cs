using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class FileWriteManager : MonoBehaviour
{
    string rawDataIODir;
    StreamWriter outStream;

    /// <Summary>
    /// Raw Data 저장 경로를 할당하는 함수
    /// </Summary>
    /// <param name="dir">저장될 디렉토리의 string값</param>
    public void SetRawDataIODir (string dir) {
        rawDataIODir = dir + ".csv";
        outStream = System.IO.File.CreateText(rawDataIODir);

        string[] column = {"time","mouse_x_pos","mouse_y_pos","target_emotion","t_e_gender","t_e_imgName","response_emotion","prime_valence","p_v_gender","p_v_imgName","response_valence","region_of_interest","EVENT"};
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Join(",", column));

        outStream.Write(sb);
    }

    public void WriteRawData (RawDataInput rdi) {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(string.Join(",", rdi.GetDataList().ToArray()));
        outStream.Write(sb);
    }

    public void ByeByeStream () {
        outStream.Close();
    }
}

public enum Event {Waiting, Mask, Prime, Target, Selecting, Response};
public enum ROI {Null, Eye, Mouth};

public class RawDataInput
{
    float time;
    float mouseXPos;
    float mouseYPos;

    Emotion targetEmotion;
    Emotion responseEmotion;
    Valence targetValence;
    Valence responseValence;
    
    ROI roi;
    Event expEvent;
    
    public RawDataInput(float time, float mouseXPos, float mouseYPos, Emotion targetEmotion, Emotion responseEmotion, Valence targetValence, Valence responseValence, ROI roi, Event expEvent)
    {
        this.time = time;
        this.mouseXPos = mouseXPos;
        this.mouseYPos = mouseYPos;
        this.targetEmotion = targetEmotion;
        this.responseEmotion = responseEmotion;
        this.targetValence = targetValence;
        this.responseValence = responseValence;
        this.roi = roi;
        this.expEvent = expEvent;
    }

    public List<string> GetDataList()
    {
        List<string> dataList = new List<string>();
        
        dataList.Add(this.time.ToString());
        dataList.Add(this.mouseXPos.ToString());
        dataList.Add(this.mouseYPos.ToString());
        
        // TargetEmotion
        if(this.targetEmotion.emotionCategory == EmotionCategory.Angry)
            dataList.Add("Angry");
        else if(this.targetEmotion.emotionCategory == EmotionCategory.Sad)
            dataList.Add("Sad");
        else if(this.targetEmotion.emotionCategory == EmotionCategory.Surprise)
            dataList.Add("Surprised");
        else if(this.targetEmotion.emotionCategory == EmotionCategory.Disgust)
            dataList.Add("Disgusted");
        else if(this.targetEmotion.emotionCategory == EmotionCategory.Happiness)
            dataList.Add("Happy");
        else if(this.targetEmotion.emotionCategory == EmotionCategory.Fear)
            dataList.Add("Fearful");
        else
            dataList.Add("-");

        if(this.targetEmotion.gender == Gender.Female)
            dataList.Add("F");
        else if(this.targetEmotion.gender == Gender.Male)
            dataList.Add("M");
        else
            dataList.Add("-");
        
        dataList.Add(this.targetEmotion.imgKeyword);

        // ResponseEmotion
        if(this.responseEmotion.emotionCategory == EmotionCategory.Angry)
            dataList.Add("Angry");
        else if(this.responseEmotion.emotionCategory == EmotionCategory.Sad)
            dataList.Add("Sad");
        else if(this.responseEmotion.emotionCategory == EmotionCategory.Surprise)
            dataList.Add("Surprised");
        else if(this.responseEmotion.emotionCategory == EmotionCategory.Disgust)
            dataList.Add("Disgusted");
        else if(this.responseEmotion.emotionCategory == EmotionCategory.Happiness)
            dataList.Add("Happy");
        else if(this.responseEmotion.emotionCategory == EmotionCategory.Fear)
            dataList.Add("Fearful");
        else
            dataList.Add("-");

        // TargetValence
        if(this.targetValence.valenceCategory == ValenceCategory.Nagative)
            dataList.Add("Nagative");
        else if(this.targetValence.valenceCategory == ValenceCategory.Positive)
            dataList.Add("Positive");
        else
            dataList.Add("-");
        
        if(this.targetValence.gender == Gender.Female)
            dataList.Add("F");
        else if(this.targetValence.gender == Gender.Male)
            dataList.Add("M");
        else
            dataList.Add("-");

        dataList.Add(this.targetValence.imgKeyword);
        
        // ResponseValence
        if(this.responseValence.valenceCategory == ValenceCategory.Nagative)
            dataList.Add("Nagative");
        else if(this.responseValence.valenceCategory == ValenceCategory.Positive)
            dataList.Add("Positive");
        else
            dataList.Add("-");

        // ROI
        if(this.roi == ROI.Eye)
            dataList.Add("Eye");
        else if(this.roi == ROI.Mouth)
            dataList.Add("Mouth");
        else
            dataList.Add("-");
        
        // Event
        if(this.expEvent == Event.Waiting)
            dataList.Add("-");
        else if(this.expEvent == Event.Target)
            dataList.Add("TargetRendered");
        else if(this.expEvent == Event.Selecting)
            dataList.Add("Selecting");
        else if(this.expEvent == Event.Mask)
            dataList.Add("MaskRendered");
        else if(this.expEvent == Event.Prime)
            dataList.Add("PrimeRendered");
        else
            dataList.Add("Response");
        
        return dataList;
    }
}