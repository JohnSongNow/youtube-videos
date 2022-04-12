using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BubbleSortExampleCutscene : Cutscene
{
    public ListVisual listVisual;

    public override IEnumerator Play()
    {
        yield return base.Play();

        ListVisualSorts.waitTime = 0.1f;
        List<int> numbers = Enumerable.Range(1, 40).ToList();
        ListVisualSorts.ShuffleNumbers(numbers);
        listVisual.SetNumbers(numbers);
        OnStepEnded();

        yield return new WaitForNextStep(this);
        yield return ListVisualSorts.BubbleSort(listVisual);
        OnStepEnded();

        yield return new WaitForNextStep(this);
        numbers = Enumerable.Range(1, 40).ToList();
        listVisual.SetNumbers(numbers);
        yield return ListVisualSorts.BubbleSort(listVisual);
        OnStepEnded();

        yield return new WaitForNextStep(this);
        numbers = Enumerable.Range(1, 40).ToList();
        numbers.Reverse();
        listVisual.SetNumbers(numbers);
        yield return ListVisualSorts.BubbleSort(listVisual);
        OnStepEnded();

        End();
    }
}
