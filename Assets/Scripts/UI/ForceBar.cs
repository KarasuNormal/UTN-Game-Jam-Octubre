using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class ForceBar : MonoBehaviour
{
    public Image fill;
    public Gradient barColor;
    //datos provisorios
    [SerializeField] private float percent;
    [SerializeField] private float maxForce;
    [SerializeField] private float actualForce;
    [SerializeField] private ObjectGrabber playerReference;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxForce = playerReference.MaxThrowForce;
    }

    // Update is called once per frame
    void Update()
    {
        actualForce = playerReference.CurrentThrowForce;
        percent = actualForce / maxForce;
        fill.color = barColor.Evaluate(percent);
        fill.fillAmount = percent;
    }
}
