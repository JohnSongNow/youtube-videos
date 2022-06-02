using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MergeSortExampleCutscene : Cutscene
{
    public ListVisual listVisual;
    public ListVisual mergeSortHelperlistVisual;

    public override IEnumerator Play()
    {
        yield return base.Play();

        ListVisualSorts.waitTime = 0.1f;
        List<int> numbers = Enumerable.Range(1, 20).ToList();
        ListVisualSorts.ShuffleNumbers(numbers);
        listVisual.SetNumbers(numbers);
        OnStepEnded();

        yield return new WaitForNextStep(this);
        yield return ListVisualSorts.MergeSort(listVisual, mergeSortHelperlistVisual);
        OnStepEnded();

        yield return new WaitForNextStep(this);
        numbers = Enumerable.Range(1, 40).ToList();
        ListVisualSorts.ShuffleNumbers(numbers);
        listVisual.SetNumbers(numbers);
        mergeSortHelperlistVisual.transform.localScale = new Vector3(1, 0.5f, 1);
        yield return ListVisualSorts.MergeSort(listVisual, mergeSortHelperlistVisual);
        OnStepEnded();

        End();
    }
}
