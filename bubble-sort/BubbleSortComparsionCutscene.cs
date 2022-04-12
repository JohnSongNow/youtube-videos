using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BubbleSortComparsionCutscene : Cutscene
{
    public ListVisual bubbleListVisual;
    public ListVisual mergeListVisual;
    public ListVisual mergeHelperListVisual;
    public ListVisual quickListVisual;

    public override IEnumerator Play()
    {
        yield return base.Play();

        ListVisualSorts.waitTime = 0.1f;
        List<int> numbers = Enumerable.Range(1, 20).ToList();
        ListVisualSorts.ShuffleNumbers(numbers);
        bubbleListVisual.SetNumbers(numbers);
        mergeListVisual.SetNumbers(new List<int>(numbers));
        quickListVisual.SetNumbers(new List<int>(numbers));
        OnStepEnded();

        yield return new WaitForNextStep(this);
        Coroutine bubble = StartCoroutine(ListVisualSorts.BubbleSort(bubbleListVisual));
        Coroutine merge = StartCoroutine(ListVisualSorts.MergeSort(mergeListVisual, mergeHelperListVisual));
        Coroutine quick = StartCoroutine(ListVisualSorts.QuickSort(quickListVisual));
        yield return bubble;
        yield return merge;
        yield return quick;
        OnStepEnded();

        End();
    }
}
