using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class ForceBar : MonoBehaviour
{
    public Image fill;
    public Gradient barColor;
    //datos provisorios
    public float maxFill;
    public float actualFill;
    public float percent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        percent = actualFill / maxFill;
        fill.color = barColor.Evaluate(percent);
        fill.fillAmount = percent;
    }
}
