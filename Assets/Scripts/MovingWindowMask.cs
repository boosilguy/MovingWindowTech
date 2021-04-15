using UnityEngine;
using UnityEngine.UI;

public class MovingWindowMask : MonoBehaviour
{
    [Header("Gaussian Blur Object's Image Component")]
    public Image movingWindowObject;

    public Vector3 screenMousePos = new Vector3 (0, 0, 0);         // 스크린 절대 위치
    public Vector3 viewportMousePos = new Vector3 (0, 0, 0);       // 스크린 상대 위치

    void Update()
    {
        screenMousePos = Input.mousePosition;
        viewportMousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        
        UpdateWindowMask();
    }

    /** Public Method *****************************************************************/
    /// <summary>
    /// 마우스 스크린 절대 위치를 반환하는 Getter.
    /// </summary>
    public Vector3 GetScreenMousePos() {
        return screenMousePos;
    }

    /// <summary>
    /// 마우스 스크린 상대 위치를 반환하는 Getter.
    /// </summary>
    public Vector3 GetViewportMousePos() {
        return viewportMousePos;
    }
    /**********************************************************************************/

    /** Default Method ****************************************************************/
    /// <summary>
    /// Gaussian Blur의 마스크를 마우스 위치에 따라 움직이는 함수.
    /// </summary>
    void UpdateWindowMask() {
        movingWindowObject.material.SetFloat("_WindowPositionX", GetViewportMousePos().x);
        movingWindowObject.material.SetFloat("_WindowPositionY", GetViewportMousePos().y);
    }
    /**********************************************************************************/
}
