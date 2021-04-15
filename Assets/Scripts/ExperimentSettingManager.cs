using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(ExperimentManager))]
public class ExperimentSettingManager : MonoBehaviour
{
    [Header("Participant Infomation UI's Properties")]
    public InputField participantID;
    public Dropdown experimentCondition;
    public Text log;

    void Start() {

    }

    void Update() {

    }

    public void GetStartButtonDown () {
        if (participantID.text.Length < 1)
        {
            StartCoroutine(RenderAlertLog());
        }
        else
        {
            string currentExpSettingStr = experimentCondition.options[experimentCondition.value].text;
            string dir = MakeExperimentDir(participantID.text, currentExpSettingStr);
            
            // BlockClass 인스턴스를 생성하고, 실험 준비가 됬음을 선포합니다!
            // 현재 실험 블록이 실험1 연습시행이라면, 연습시행1 BlockClass를 생성.
            if (experimentCondition.value == 0)
                this.GetComponent<ExperimentManager>().SetUpBlock(BlockState.Exp1P);
            // 현재 실험 블록이 실험1 MW 연습시행이라면, 연습시행1 MV BlockClass를 생성.
            else if (experimentCondition.value == 1)
                this.GetComponent<ExperimentManager>().SetUpBlock(BlockState.Exp1PMW);
            // 현재 실험 블록이 실험1 메인페이즈라면, 실험1 BlockClass를 생성.
            else if (experimentCondition.value == 2)
                this.GetComponent<ExperimentManager>().SetUpBlock(BlockState.Exp1);
            // 현재 실험 블록이 실험2 연습시행이라면, 연습시행2 BlockClass를 생성.
            else if (experimentCondition.value == 3)
                this.GetComponent<ExperimentManager>().SetUpBlock(BlockState.Exp2P);
            // 현재 실험 블록이 실험2 메인페이즈라면, 실험2 BlockClass를 생성.
            else if (experimentCondition.value == 4)
                this.GetComponent<ExperimentManager>().SetUpBlock(BlockState.Exp2);

            // 실험 데이터 디렉토리를 전달한다.
            this.GetComponent<ExperimentManager>().fileWriteManager.SetRawDataIODir(dir);
        }
    }

    /// <summary>
    /// Raw Data를 보관할 디렉토리를 구성할 String을 반환하는 함수.
    /// </summary>
    /// <param name="pID">기입된 피험자 ID</param>
    /// <param name="eCondition">실험 조건</param>
    string MakeExperimentDir (string pID, string eCondition) {
        // 파일 디렉토리 갱신하여, 해당 디렉토리의 string값을 반환.
        string localPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        localPath = localPath + "\\MovingWindowRawData\\" + pID;
        
        if(!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        string dir = localPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss_") + eCondition;
        Debug.Log("실험자 파일 데이터 생성 : " + dir);
        return dir;
    }

    /// <summary>
    /// 피험자 정보 입력 인터페이스에서 경고 로그를 출력하는 함수.
    /// </summary>
    IEnumerator RenderAlertLog ()
    {
        log.text = "피험자 ID를 정확히 입력하세요 (공백 및 공백문자를 단일로 사용하지 마십시오).";
        yield return new WaitForSecondsRealtime(2.5f);
        log.text = null;
    }
}
