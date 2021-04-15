using UnityEngine;

public class ValenceCategorySelector : MonoBehaviour
{
    [Header("Experiment Manager Script")]
    public ExperimentManager experimentManager;

    public void GetNagativeButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Valence(ValenceCategory.Nagative));
    }
    public void GetPositiveButtonDown ()
    {
        experimentManager.SetCurrentResponse(new Valence(ValenceCategory.Positive));
    }
}
