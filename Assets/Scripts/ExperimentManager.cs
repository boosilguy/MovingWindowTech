using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;


public class ExperimentManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    /** Public Variable *****************************************************************/
    [Header("File Writer Component")]
    public FileWriteManager fileWriteManager;
    [Header("UI GameObject")]
    public GameObject emotionOption;        // 정서 선택지
    public GameObject valenceOption;        // Valence 선택지
    public GameObject fixationPointObject;  // FixationPoint
    public GameObject userInformation;      // 사용자 정보 입력 인터페이스

    [Header("Stimuli's Image Component")]
    public MovingWindowMask movingWindowMask; // MW Mask
    public Image emotionStimuliImage;       // 정서 자극 Image 컴포넌트
    public Image blurStimuli;               // 블러 자극 Image 컴포넌트
    public Sprite maskImage;                // Exp2 마스크 이미지 Sprite 큼포넌트
    
    [Header("Stimuli Exposure Time")]
    public float exp1StimuliTime = 2.5f;    // Exp1 자극 노출 시간
    public float exp2StimuliTime = 2.0f;    // Exp2 자극 노출 시간
    public float exp2PrimeTime = 0.05f;     // Exp2 Prime 자극 노출 시간
    public float exp2MaskTime = 0.05f;      // Exp2 마스크 이미지 노출 시간

    [Header("Stimuli Exposure Time")]
    public int xPixelOffset = 335;          // 해상도 맞추기 위한 너비 Offset
    public int yPixelOffset = 705;          // 해상도 맞추기 위한 높이 Offset
    /************************************************************************************/

    /** Default Variable ****************************************************************/
    Emotion currentEmotionResponse = new Emotion(EmotionCategory.Null);  // 응답한 Emotion
    Emotion currentTargetEmotion = new Emotion(EmotionCategory.Null);    // 타겟 Emotion
    Valence currentValenceResponse = new Valence(ValenceCategory.Null);  // 응답한 Valence
    Valence currentTargetValence = new Valence(ValenceCategory.Null);  // 응답한 Valence
    Block currentBlock;                     // 현재 구성된 블록의 속성 (어떤 Condition인지)
    Event currentEvent;                     // 현재 어떤 이벤트인지

    float blockTime = 0;                    // 현재 Block이 진행된 시간
    float trialTime = 0;                    // 현재 trial가 진행된 시간
    float forwardMaskTime = 0;              // 현재 Forward 마스크가 노출된 시간
    float primeTime = 0;                    // 현재 Prime 자극이 노출된 시간
    float backwardMaskTime = 0;             // 현재 Backward 마스크가 노출된 시간
    float responseTime = 0;                 // 선택지가 등장 ~ 선택지 선택까지의 지연된 시간
    bool isUsedBlur = false;                // 블러 자극 사용 여부
    bool getReady = false;                  // 실험 준비 여부
    string stimuliImgDir;                   // Emotion 이미지 경로
    string valenceImgDir;                   // Valence 이미지 경로
    /************************************************************************************/

    void Awake() {
        Application.targetFrameRate = 60;    
    }

    void Start() {
        // 기본값 초기화
        InitCurrentBlock();
        InitEmotionImageComponent();
        InitEmotionImageColor();
        InitEmotionImgDir();
        InitValenceImgDir();
        InitGaussianBlur();
    }

    void Update()
    {
        if(currentBlock == null)
        {
            blockTime = 0;
            return;
        }
        else
        {
            // 피험자 정보 입력창이 렌더링된 상태라면, Off한다.
            // 더불어, Blur 자극도 조건에 맞게 조정한다.
            if(GetExpSetUIState() == true)
            {
                UnrenderExpSetUI();
                ConfigureBlur();
            }

            if(currentBlock.stimuliSet.length != 0)
            {
                // 피험자가 준비되었음을 확인하기 위해 Fixation Point를 렌더링한다.
                if(getReady == false)
                {
                    // Fixation Point가 렌더링되지 않았다면, 렌더링한다.
                    if(GetFixationState() == false)
                    {
                        RenderFixation();
                        currentEvent = Event.Waiting;
                    }
                    else
                    {
                        // Fixation Point를 응시하던 피험자가 준비되었을 때,
                        // 스페이스바를 입력하면 실험이 시작된다.
                        // 더불어, 마우스 위치는 가운데로 초기화된다.
                        // 또한, Gaussian Blur가 사용되는 상황이라면, Blur 자극을 On한다.
                        if(Input.GetKeyDown(KeyCode.Space))
                        {
                            getReady = true;
                            UnrenderFixation();

                            // Exp1용 커서 얼리기
                            FreezeCursor();           

                            if (isUsedBlur == true && ((currentBlock.GetState() == BlockState.Exp1PMW) || (currentBlock.GetState() == BlockState.Exp1)))
                                blurStimuli.enabled = true;
                            else
                                blurStimuli.enabled = false;
                        }
                    }
                }

                // 준비되었으니, Task Trial로 넘어간다.                
                if(getReady == true)
                {
                    // 자극을 하나씩 렌더링한다.
                    if(currentBlock.stimuliSet.length == currentBlock.stimuliSet.set.Count)
                    {
                        RenderStimuli(currentBlock);
                    }
                    // 자극이 렌더링 된 직후 호출되는 함수
                    else
                    {
                        /********************************************************************/
                        //          실험 2와 관련된 블록                                     //
                        /********************************************************************/
                        if((currentBlock.GetState() == BlockState.Exp2) || (currentBlock.GetState() == BlockState.Exp2P))
                        {
                            // 자극 등장 ~ 자극 최종 선택까지 시간 저장.
                            trialTime = trialTime + Time.deltaTime;
                            if(trialTime > exp2StimuliTime)
                            {
                                // 선택지 등장 ~ 자극 최종 선택까지 시간 저장.
                                responseTime = responseTime + Time.deltaTime;
                                if(GetValenceOptionState() == false)
                                {
                                    RenderValenceOption();

                                    if (isUsedBlur == true)
                                        blurStimuli.enabled = false;
                                    
                                    currentEvent = Event.Selecting;
                                }
                                else
                                {
                                    if(currentValenceResponse.valenceCategory != ValenceCategory.Null)
                                    {
                                        blockTime = blockTime + Time.deltaTime;
                                        currentEvent = Event.Response;

                                        // 현재 데이터를 기록한다.
                                        WriteRawData(false);

                                        // Valence 선택지를 Off하고, 화면 렌더링을 조정한다.
                                        // 이미지 초기화 -> 검은 색 -> 선택지 Off
                                        UnrenderValenceOption();
                                        InitEmotionImageComponent();
                                        SetEmotionImageColor(Color.black);

                                        // 또한, 현재 자극 셋 길이를 갱신한다.
                                        // 그리고 새로운 Trial을 준비하기 위해서, 준비 상태를 Off한다.
                                        SetCurrentResponse(new Valence(ValenceCategory.Null));
                                        currentBlock.stimuliSet.length = currentBlock.stimuliSet.set.Count;
                                        getReady = false;
                                        trialTime = 0;
                                        responseTime = 0;

                                        currentTargetEmotion = new Emotion(EmotionCategory.Null);
                                        currentTargetValence = new Valence(ValenceCategory.Null);

                                        return;
                                    }
                                }
                            }
                        }

                        /********************************************************************/
                        //          실험 1과 관련된 블록                                     //
                        /********************************************************************/
                        else
                        {
                            // Exp1용 커서 녹이기
                            MeltCursor();
                            
                            // 자극 등장 ~ 자극 최종 선택까지 시간 저장.
                            trialTime = trialTime + Time.deltaTime;

                            if(trialTime > exp1StimuliTime)
                            {
                                // 선택지 등장 ~ 자극 최종 선택까지 시간 저장.
                                responseTime = responseTime + Time.deltaTime;

                                if(GetEmotionOptionState() == false)
                                {
                                    RenderEmotionOption();

                                    if (isUsedBlur == true)
                                        blurStimuli.enabled = false;
                                    
                                    currentEvent = Event.Selecting;
                                }
                                else
                                {
                                    // 카테고리 선택지에서 피험자가 어떤 감정인지 선택을 했다면,
                                    if(currentEmotionResponse.emotionCategory != EmotionCategory.Null)
                                    {
                                        blockTime = blockTime + Time.deltaTime;
                                        currentEvent = Event.Response;

                                        // 현재 데이터를 기록한다.
                                        WriteRawData(false);

                                        // 카테고리 선택지를 Off하고, 화면 렌더링을 조정한다.
                                        // 이미지 초기화 -> 검은 색 -> 선택지 Off
                                        UnrenderEmotionOption();
                                        InitEmotionImageComponent();
                                        SetEmotionImageColor(Color.black);

                                        // 또한, 현재 자극 셋 길이를 갱신한다.
                                        // 그리고 새로운 Trial을 준비하기 위해서, 준비 상태를 Off한다.
                                        SetCurrentResponse(new Emotion(EmotionCategory.Null));
                                        currentBlock.stimuliSet.length = currentBlock.stimuliSet.set.Count;
                                        getReady = false;
                                        trialTime = 0;
                                        responseTime = 0;

                                        currentTargetEmotion = new Emotion(EmotionCategory.Null);

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                blockTime = blockTime + Time.deltaTime;
                
                if ((currentBlock.GetState() == BlockState.Exp2) || (currentBlock.GetState() == BlockState.Exp2P))
                {
                    if (trialTime < exp2StimuliTime && trialTime > 0)
                    {
                        WriteRawData(true);
                    }
                    else
                    {
                        WriteRawData(false);
                    }
                }
                else
                {
                    if (trialTime < exp1StimuliTime && trialTime > 0)
                    {
                        WriteRawData(true);
                    }
                    else
                    {
                        WriteRawData(false);
                    }
                }
                    
            }
            else
            {
                // 모든 상태를 초기화하여, 처음 화면으로 회귀한다.
                InitCurrentBlock();
                InitEmotionImageComponent();
                InitEmotionImageColor();
                InitEmotionImgDir();
                InitValenceImgDir();
                InitGaussianBlur();

                RenderExpSetUI();
                OnEmotionImageComponent();
                
                trialTime = 0;
                responseTime = 0;
                fileWriteManager.ByeByeStream();
            }
        }
    }

    /** Public Method *****************************************************************/
    
    /// <Summary>
    /// 현재 응답에 피험자의 정서 응답을 저장하는 함수
    /// </Summary>
    /// <param name="emotionCategory">피험자가 응답한 정서</param>
    public void SetCurrentResponse (Emotion emotion)
    {
        currentEmotionResponse = emotion;
    }

    /// <Summary>
    /// 현재 응답에 피험자의 Valence 응답을 저장하는 함수
    /// </Summary>
    /// <param name="valenceCategory">피험자가 응답한 Valence</param>
    public void SetCurrentResponse (Valence valence)
    {
        currentValenceResponse = valence;
    }

    /// <Summary>
    /// 현재 진행할 Block을 정의하는 함수
    /// </Summary>
    /// <param name="state">Block의 Condition을 정의하는 열거형 변수</param>
    public void SetUpBlock (BlockState state) {
        currentBlock = new Block(state);

        if (state == BlockState.Exp1P)
        {
            // 연습용 자극 이미지가 있을 곳
            stimuliImgDir += "/Practice";

            Queue<Emotion> eQueue = new Queue<Emotion>();
            
            // 임시적으로 구현한 감정 자극 세트 큐 구축 기능.
            // 아래와 같은 양식으로 Queue에 삽입할 것.
            // Pratice이므로, 정적으로 구성해 놓았음.
            // 후에 Practice 이미지가 정의될 시, 수정할 것. 
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Angry));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Disgust));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Fear));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Happiness));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Sad));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Surprise));
            
            StimuliSet exp1PracticeSet = new StimuliSet(eQueue);

            currentBlock.stimuliSet = exp1PracticeSet;
        }
        else if (state == BlockState.Exp1PMW)
        {
            // 연습용 자극 이미지가 있을 곳
            stimuliImgDir += "/Practice";

            Queue<Emotion> eQueue = new Queue<Emotion>();
            
            // 임시적으로 구현한 감정 자극 세트 큐 구축 기능.
            // 아래와 같은 양식으로 Queue에 삽입할 것.
            // Pratice이므로, 정적으로 구성해 놓았음.
            // 후에 Practice 이미지가 정의될 시, 수정할 것.
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Angry));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Disgust));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Fear));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Happiness));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Sad));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Surprise));
            
            StimuliSet exp1MWPracticeSet = new StimuliSet(eQueue);

            currentBlock.stimuliSet = exp1MWPracticeSet;
        }
        else if (state == BlockState.Exp1)
        {
            // 랜덤으로 자극 이미지를 뽑아, 큐에 삽입.
            Queue<Emotion> eQueue = GetEmoStimuliSet ();
            
            StimuliSet exp1Set = new StimuliSet(eQueue);

            currentBlock.stimuliSet = exp1Set;
        }
        else if (state == BlockState.Exp2P)
        {
            // 연습용 자극 이미지가 있을 곳
            stimuliImgDir += "/Practice";
            valenceImgDir += "/Practice";

            Queue<Emotion> eQueue = new Queue<Emotion>();
            Queue<Valence> vQueue = new Queue<Valence>();
            
            // 임시적으로 구현한 감정 및 Valence 자극 세트 큐 구축 기능.
            // 아래와 같은 양식으로 Queue에 삽입할 것.
            // Pratice이므로, 정적으로 구성해 놓았음.
            // 후에 Practice 이미지가 정의될 시, 수정할 것.
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Angry));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Disgust));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Fear));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Happiness));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Sad));
            eQueue.Enqueue(new Emotion(Gender.Male, 1, "#", EmotionCategory.Surprise));

            vQueue.Enqueue(new Valence(Gender.Male, 99, ValenceCategory.Positive));
            vQueue.Enqueue(new Valence(Gender.Female, 99, ValenceCategory.Nagative));
            vQueue.Enqueue(new Valence(Gender.Male, 99, ValenceCategory.Positive));
            vQueue.Enqueue(new Valence(Gender.Female, 99, ValenceCategory.Nagative));
            vQueue.Enqueue(new Valence(Gender.Male, 99, ValenceCategory.Positive));
            vQueue.Enqueue(new Valence(Gender.Female, 99, ValenceCategory.Nagative));
            
            StimuliSet exp2PracticeSet = new StimuliSet(eQueue, vQueue);

            currentBlock.stimuliSet = exp2PracticeSet;
        }
        else if (state == BlockState.Exp2)
        {
            // 랜덤으로 자극, Valence 이미지를 뽑아, 큐에 삽입.
            Queue<Emotion> eQueue = GetEmoStimuliSet ();
            Queue<Valence> vQueue = GetValStimuliSet ();
            
            StimuliSet exp2Set = new StimuliSet(eQueue, vQueue);

            currentBlock.stimuliSet = exp2Set;
        }
    }
    /**********************************************************************************/

    /** Default Method ****************************************************************/
    
    /// <Summary>
    /// Block Condition에 따라, 자극을 렌더링하는 함수
    /// </Summary>
    /// <param name="currentBlock">현재 진행되는 블록 정보를 담는 Block 인스턴스</param>
    void RenderStimuli(Block currentBlock) {

        /********************************************************************/
        //          실험 2와 관련된 블록                                     //
        /********************************************************************/
        if((currentBlock.GetState() == BlockState.Exp2) || (currentBlock.GetState() == BlockState.Exp2P))
        {
            // Forward Masking
            if(forwardMaskTime < exp2MaskTime)
            {
                InitEmotionImageColor();

                if(emotionStimuliImage.sprite == null)
                {
                    emotionStimuliImage.sprite = maskImage;
                    currentEvent = Event.Mask;
                }

                forwardMaskTime = forwardMaskTime + Time.deltaTime;
            }
            else
            {
                // 프라임 자극 이미지 렌더링
                if(primeTime < exp2PrimeTime)
                {
                    if(emotionStimuliImage.sprite.Equals(maskImage))
                    {
                        currentTargetValence = currentBlock.stimuliSet.valSubSet.Dequeue();

                        string path = valenceImgDir + "/" + currentTargetValence.imgKeyword;
                        emotionStimuliImage.sprite = Resources.Load<Sprite>(path);
                        currentBlock.stimuliSet.valSubLength = currentBlock.stimuliSet.valSubSet.Count;
                        currentEvent = Event.Prime;
                    }

                    primeTime = primeTime + Time.deltaTime;
                }
                else
                {
                    // Backward Masking
                    if(backwardMaskTime < exp2MaskTime)
                    {
                        currentEvent = Event.Mask;
                        if(!emotionStimuliImage.sprite.Equals(maskImage))
                        {
                            emotionStimuliImage.sprite = maskImage;
                        }

                        backwardMaskTime = backwardMaskTime + Time.deltaTime;

                        // Exp2용 커서 얼리기
                        FreezeCursor();
                    }
                    // 모든 Prime 자극이 끝나면, 타겟 자극 렌더링
                    else
                    {
                        forwardMaskTime = primeTime = backwardMaskTime = 0;

                        currentTargetEmotion = currentBlock.stimuliSet.set.Dequeue();

                        string path = stimuliImgDir + "/" + currentTargetEmotion.rawKeyword;
                        emotionStimuliImage.sprite = Resources.Load<Sprite>(path);

                        // Exp2용 커서 녹이기
                        MeltCursor();

                        if (isUsedBlur == true)
                            blurStimuli.enabled = true;
                        
                        currentEvent = Event.Target;
                    }
                }
            }
        }

        /********************************************************************/
        //          실험 1과 관련된 블록                                     //
        /********************************************************************/
        else
        {
            currentTargetEmotion = currentBlock.stimuliSet.set.Dequeue();

            string path = stimuliImgDir + "/" + currentTargetEmotion.rawKeyword;
            emotionStimuliImage.sprite = Resources.Load<Sprite>(path);

            InitEmotionImageColor();

            currentEvent = Event.Target;
        }
    }

    /// <Summary>
    /// Fixation Point의 On/Off 상태를 반환하는 Getter
    /// </Summary>
    bool GetFixationState() {
        return fixationPointObject.activeInHierarchy;
    }

    /// <Summary>
    /// 정서 선택지의 On/Off 상태를 반환하는 Getter
    /// </Summary>
    bool GetEmotionOptionState() {
        return emotionOption.activeInHierarchy;
    }

    /// <Summary>
    /// Valence 선택지의 On/Off 상태를 반환하는 Getter
    /// </Summary>
    bool GetValenceOptionState() {
        return valenceOption.activeInHierarchy;
    }

    /// <Summary>
    /// 사용자 정보 입력 인터페이스의 On/Off 상태를 반환하는 Getter
    /// </Summary>
    bool GetExpSetUIState() {
        return userInformation.activeInHierarchy;
    }
    
    /// <Summary>
    /// string값에 있는 정서 이미지 파일을 파싱하여, 파일 이름을 반환하는 Getter
    /// </Summary>
    /// <param name="emo">Raw String</param>
    string GetEmoStimuliName(string emo)
    {
        string resourcesPath = Application.dataPath + "/Resources/" + stimuliImgDir + "/";
        string[] defaultDir = Directory.GetFiles(resourcesPath);
        
        // Regex 'emo' & Linq pattern
        string pattern = @emo; string metaPattern = @"meta";

        // Linq
        IEnumerable<string> emoFiles = from file in defaultDir where file.Contains(pattern) select file;
        emoFiles = from file in emoFiles where !file.Contains(metaPattern) select file;
        int idx = new System.Random().Next(emoFiles.Count<string>());
        
        return emoFiles.ElementAt<string>(idx);
    }

    /// <Summary>
    /// string값에 있는 Valence 이미지 파일을 파싱하여, 파일 이름을 반환하는 Getter
    /// </Summary>
    /// <param name="emo">Raw String</param>
    string GetValStimuliName(string val)
    {
        string resourcesPath = Application.dataPath + "/Resources/" + valenceImgDir + "/";
        string[] defaultDir = Directory.GetFiles(resourcesPath);
        
        // Regex 'val' & Linq pattern
        string pattern = @val; string metaPattern = @"meta";

        // Linq
        IEnumerable<string> valFiles = from file in defaultDir where file.Contains(pattern) select file;
        valFiles = from file in valFiles where !file.Contains(metaPattern) select file;
        int idx = new System.Random().Next(valFiles.Count<string>());
        
        return valFiles.ElementAt<string>(idx);
    }

    /// <Summary>
    /// 정서 자극을 랜덤으로 추출하여, Queue로 구성한 후 반환하는 Getter
    /// </Summary>
    Queue<Emotion> GetEmoStimuliSet ()
    {
        List<string> emoStimuliDir = new List<string>();
        
        // List에 각 정서별로 무작위 이미지를 추출하여 저장.
        emoStimuliDir.Add(GetEmoStimuliName("Angry"));
        emoStimuliDir.Add(GetEmoStimuliName("Happy"));
        emoStimuliDir.Add(GetEmoStimuliName("Surprised"));
        emoStimuliDir.Add(GetEmoStimuliName("Disgusted"));
        emoStimuliDir.Add(GetEmoStimuliName("Sad"));
        emoStimuliDir.Add(GetEmoStimuliName("Fearful"));
        
        // List 셔플.
        List<string> shuffledESD = emoStimuliDir.OrderBy(g => System.Guid.NewGuid()).ToList();

        Queue<Emotion> emotionQueue = new Queue<Emotion>();

        foreach (string value in shuffledESD)
        {
            emotionQueue.Enqueue(EmoStimuliClassifier(value));
        }

        return emotionQueue;
    }

    /// <Summary>
    /// Valence 자극을 랜덤으로 추출하여, Queue로 구성한 후 반환하는 Getter
    /// </Summary>
    Queue<Valence> GetValStimuliSet ()
    {
        List<string> valStimuliDir = new List<string>();

        // List에 각 Valence별로 무작위 이미지를 추출하여 저장.
        valStimuliDir.Add(GetValStimuliName("Negative"));
        valStimuliDir.Add(GetValStimuliName("Positive"));
        valStimuliDir.Add(GetValStimuliName("Negative"));
        valStimuliDir.Add(GetValStimuliName("Positive"));
        valStimuliDir.Add(GetValStimuliName("Negative"));
        valStimuliDir.Add(GetValStimuliName("Positive"));
        
        // List 셔플.
        List<string> shuffledVSD = valStimuliDir.OrderBy(g => System.Guid.NewGuid()).ToList();

        Queue<Valence> valenceQueue = new Queue<Valence>();

        foreach (string value in shuffledVSD)
        {
            valenceQueue.Enqueue(ValStimuliClassifier(value));
        }

        return valenceQueue;
    }

    /// <Summary>
    /// 정서 자극의 Image 컴포넌트의 색상을 조정하는 Setter
    /// </Summary>
    /// <param name="color">할당될 색상</param>
    void SetEmotionImageColor(Color color)
    {
        emotionStimuliImage.color = color;
    }

    /// <Summary>
    /// 사용자 정보 입력 인터페이스를 렌더링하는 함수
    /// </Summary>
    void RenderExpSetUI() {
        userInformation.SetActive(true);
    }

    /// <Summary>
    /// 사용자 정보 입력 인터페이스를 Off하는 함수
    /// </Summary>
    void UnrenderExpSetUI() {
        userInformation.SetActive(false);
    }

    /// <Summary>
    /// Fixation Point를 렌더링하는 함수
    /// </Summary>
    void RenderFixation() {
        fixationPointObject.SetActive(true);
        OffEmotionImageComponent();
    }

    /// <Summary>
    /// Fixation Point를 Off하는 함수
    /// </Summary>
    void UnrenderFixation() {
        fixationPointObject.SetActive(false);
        OnEmotionImageComponent();
    }

    /// <Summary>
    /// 정서 선택지를 렌더링하는 함수
    /// </Summary>
    void RenderEmotionOption() {
        emotionOption.SetActive(true);
        OffEmotionImageComponent();
    }

    /// <Summary>
    /// 정서 선택지를 Off하는 함수
    /// </Summary>
    void UnrenderEmotionOption() {
        emotionOption.SetActive(false);
        OnEmotionImageComponent();
    }

    /// <Summary>
    /// Valence 선택지를 렌더링하는 함수
    /// </Summary>
    void RenderValenceOption() {
        valenceOption.SetActive(true);
        OffEmotionImageComponent();
    }

    /// <Summary>
    /// Valence 선택지를 Off하는 함수
    /// </Summary>
    void UnrenderValenceOption() {
        valenceOption.SetActive(false);
        OnEmotionImageComponent();
    }

    /// <Summary>
    /// 정서 이미지의 Image 컴포넌트를 Off하는 함수
    /// </Summary>
    void OffEmotionImageComponent() {
        emotionStimuliImage.enabled = false;
    }

    /// <Summary>
    /// 정서 이미지의 Image 컴포넌트를 On하는 함수
    /// </Summary>
    void OnEmotionImageComponent() {
        emotionStimuliImage.enabled = true;
    }

    /// <Summary>
    /// Blur를 초기화하는 함수
    /// </Summary>
    void InitGaussianBlur() {
        isUsedBlur = false;
    }

    /// <Summary>
    /// 정서 이미지 경로를 초기화하는 함수
    /// </Summary>
    void InitEmotionImgDir()
    {
        stimuliImgDir = "EmotionalFacialExpression";
    }

    /// <Summary>
    /// Valence 이미지 경로를 초기화하는 함수
    /// </Summary>
    void InitValenceImgDir()
    {
        valenceImgDir = "PrimeFacialExpression";
    }

    /// <Summary>
    /// 정서 이미지의 Image 컴포넌트를 초기화하는 함수
    /// </Summary>
    void InitEmotionImageComponent()
    {
        emotionStimuliImage.sprite = null;
    }

    /// <Summary>
    /// 정서 이미지의 Image 컴포넌트의 색상을 초기화하는 함수
    /// </Summary>
    void InitEmotionImageColor()
    {
        emotionStimuliImage.color = Color.white;
    }

    /// <Summary>
    /// 현재 Block Condition을 초기화하는 함수
    /// </Summary>
    void InitCurrentBlock()
    {
        currentBlock = null;
    }

    /// <Summary>
    /// rawString을 정서 파일 프로퍼티로 분류하는 함수
    /// </Summary>
    /// <param name="rawString">Dir를 포함한 정서 이미지 파일의 경로</param>
    Emotion EmoStimuliClassifier(string rawString)
    {
        /*************************************************************************************/
        //  파일명 양식 : 감정_성별_번호(두자리)_배우 ID_ex1_ey1_ex2_ey2_mx1_my1_mx2_my2.형식자 //
        //  * [0] : 감정                                                                     //
        //  * [1] : 성별                                                                     //
        //  * [2] : 번호                                                                     //
        //  * [3] : 배우 ID                                                                  //
        //  * [4] : ex1                                                                     //
        //  * [5] : ey1                                                                     //
        //  * [6] : ex2                                                                     //
        //  * [7] : ey2                                                                     //
        //  * [8] : mx1                                                                     //
        //  * [9] : my1                                                                     //
        //  * [10] : mx2                                                                    //
        //  * [11] : my2                                                                    //
        //  * [12] : 형식자                                                                  //
        /*************************************************************************************/

        string [] splitDir = rawString.Split('/');
        string emotionFileName = splitDir.Last<string>();       // 파일명 저장 변수
        string [] splitComponent = emotionFileName.Split('_');  // Underbar로 Parsing하여, 얻는 정서 이미지 파일의 프로퍼티 배열 변수
        
        Gender gender;
        EmotionCategory emotionCategory;
        int identity;
        string actorIdentity;
        Vector2 e1;
        Vector2 e2;
        Vector2 m1;
        Vector2 m2;

        // [0] -> 감정 분류
        if (splitComponent[0] == "Happy")
            emotionCategory = EmotionCategory.Happiness;
        else if (splitComponent[0] == "Sad")
            emotionCategory = EmotionCategory.Sad;
        else if (splitComponent[0] == "Disgusted")
            emotionCategory = EmotionCategory.Disgust;
        else if (splitComponent[0] == "Angry")
            emotionCategory = EmotionCategory.Angry;
        else if (splitComponent[0] == "Surprised")
            emotionCategory = EmotionCategory.Surprise;
        else if (splitComponent[0] == "Fearful")
            emotionCategory = EmotionCategory.Fear;
        else
        {
            Debug.LogException(new System.Exception("이미지 자극 디렉토리에 불필요한 파일이 존재합니다. 제거 후, 재실행 하십시오."));
            emotionCategory = EmotionCategory.Null;
        }

        // [1] -> 성별 분류
        if (splitComponent[1] == "Female")
            gender = Gender.Female;
        else
            gender = Gender.Male;

        // [2] -> 감정 이미지의 ID 분류
        identity = Convert.ToInt32(splitComponent[2]);

        // [3] -> 감정 이미지 배우의 ID 분류
        actorIdentity = splitComponent[3];

        // [4],[5] -> 눈 ROI Coord 1
        e1 = new Vector2(Convert.ToSingle(splitComponent[4]), Convert.ToSingle(splitComponent[5]));
        
        // [6],[7] -> 눈 ROI Coord 2
        e2 = new Vector2(Convert.ToSingle(splitComponent[6]), Convert.ToSingle(splitComponent[7]));
        
        // [8],[9] -> 입 ROI Coord 1
        m1 = new Vector2(Convert.ToSingle(splitComponent[8]), Convert.ToSingle(splitComponent[9]));
        
        // [10],[11] -> 입 ROI Coord 2
        m2 = new Vector2(Convert.ToSingle(splitComponent[10]), Convert.ToSingle(splitComponent.Last<string>().Split('.')[0]));

        // Emotion 인스턴스로 재갱신
        return new Emotion(gender, identity, actorIdentity, emotionCategory, e1, e2, m1, m2, emotionFileName.Split('.')[0]);
    }

    /// <Summary>
    /// rawString으로부터 Valence 자극을 분류하는 함수
    /// </Summary>
    /// <param name="rawString">Raw String</param>
    Valence ValStimuliClassifier(string rawString)
    {
        /*****************************************************/
        //  파일명 양식 : 감정_성별_번호(두자리)_배우 ID.형식자 //
        //  * [0] : 감정                                     //
        //  * [1] : 성별                                     //
        //  * [2] : 번호.형식자                              //
        /*****************************************************/

        string [] splitDir = rawString.Split('/');
        string valenceFileName = splitDir.Last<string>();
        string [] splitComponent = valenceFileName.Split('_'); // Underbar로 Parsing하여, 얻는 Valence 이미지 파일의 프로퍼티 배열 변수
        
        Gender gender;
        ValenceCategory valenceCategory;
        int identity;

        // [0] -> 감정 분류
        if (splitComponent[0] == "Negative")
            valenceCategory = ValenceCategory.Nagative;
        else if (splitComponent[0] == "Positive")
            valenceCategory = ValenceCategory.Positive;
        else
        {
            Debug.LogException(new System.Exception("이미지 자극 디렉토리에 불필요한 파일이 존재합니다. 제거 후, 재실행 하십시오."));
            valenceCategory = ValenceCategory.Null;
        }

        // [1] -> 성별 분류
        if (splitComponent[1] == "Female")
            gender = Gender.Female;
        else
            gender = Gender.Male;

        // [2] -> 마지막 형식자 제거 동시에, Identity 번호
        identity = Convert.ToInt32(splitComponent.Last<string>().Split('.')[0]);

        // Valence 인스턴스로 재갱신
        return new Valence(gender, identity, valenceCategory);
    }

    ///<summary>
    ///블러 필터 프로퍼티 구성
    ///</summary>
    void ConfigureBlur() {
        if (currentBlock.GetState() == BlockState.Exp1P)
        {
            isUsedBlur = false;
        }
        else if (currentBlock.GetState() == BlockState.Exp1PMW)
        {
            isUsedBlur = true;
        }
        else if (currentBlock.GetState() == BlockState.Exp1)
        {
            isUsedBlur = true;
        }
        else if (currentBlock.GetState() == BlockState.Exp2P)
        {
            isUsedBlur = true;
        }
        else
        {
            isUsedBlur = true;
        }
    }

    ///<summary>
    ///Cursor를 중앙으로 고정시키는 메소드
    ///</summary> 
    void FreezeCursor ()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    ///<summary>
    ///Cursor 고정을 해제시키는 메소드
    ///</summary>
    void MeltCursor ()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    ///<summary>
    ///RawData를 기록하는 메소드
    ///</summary>
    void WriteRawData (bool isExistROI)
    {
        if (isExistROI==true)
        {
            if ((movingWindowMask.GetScreenMousePos().y < (currentTargetEmotion.eyePixCoordinate1.y + yPixelOffset)) && (movingWindowMask.GetScreenMousePos().y > (currentTargetEmotion.eyePixCoordinate2.y + yPixelOffset)))
            {
                if ((movingWindowMask.GetScreenMousePos().x > (currentTargetEmotion.eyePixCoordinate1.x + xPixelOffset)) && (movingWindowMask.GetScreenMousePos().x < (currentTargetEmotion.eyePixCoordinate2.x + xPixelOffset)))
                {
                    fileWriteManager.WriteRawData(new RawDataInput(blockTime, movingWindowMask.GetScreenMousePos().x, movingWindowMask.GetScreenMousePos().y, currentTargetEmotion, currentEmotionResponse, currentTargetValence, currentValenceResponse, ROI.Eye, currentEvent));
                }
                else
                {
                    fileWriteManager.WriteRawData(new RawDataInput(blockTime, movingWindowMask.GetScreenMousePos().x, movingWindowMask.GetScreenMousePos().y, currentTargetEmotion, currentEmotionResponse, currentTargetValence, currentValenceResponse, ROI.Null, currentEvent));
                }
            }
            else if ((movingWindowMask.GetScreenMousePos().y < (currentTargetEmotion.mouthPixCoordinate1.y + yPixelOffset)) && (movingWindowMask.GetScreenMousePos().y > (currentTargetEmotion.mouthPixCoordinate2.y + yPixelOffset)))
            {
                if ((movingWindowMask.GetScreenMousePos().x > (currentTargetEmotion.mouthPixCoordinate1.x + xPixelOffset)) && (movingWindowMask.GetScreenMousePos().x < (currentTargetEmotion.mouthPixCoordinate2.x + xPixelOffset)))
                {
                    fileWriteManager.WriteRawData(new RawDataInput(blockTime, movingWindowMask.GetScreenMousePos().x, movingWindowMask.GetScreenMousePos().y, currentTargetEmotion, currentEmotionResponse, currentTargetValence, currentValenceResponse, ROI.Mouth, currentEvent));
                }
                else
                {
                    fileWriteManager.WriteRawData(new RawDataInput(blockTime, movingWindowMask.GetScreenMousePos().x, movingWindowMask.GetScreenMousePos().y, currentTargetEmotion, currentEmotionResponse, currentTargetValence, currentValenceResponse, ROI.Null, currentEvent));
                }
            }
            else
            {
                fileWriteManager.WriteRawData(new RawDataInput(blockTime, movingWindowMask.GetScreenMousePos().x, movingWindowMask.GetScreenMousePos().y, currentTargetEmotion, currentEmotionResponse, currentTargetValence, currentValenceResponse, ROI.Null, currentEvent));
            }
        }
        else
        {
            fileWriteManager.WriteRawData(new RawDataInput(blockTime, movingWindowMask.GetScreenMousePos().x, movingWindowMask.GetScreenMousePos().y, currentTargetEmotion, currentEmotionResponse, currentTargetValence, currentValenceResponse, ROI.Null, currentEvent));
        }
    }
    /**********************************************************************************/
}
