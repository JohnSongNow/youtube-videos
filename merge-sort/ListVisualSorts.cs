using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class ListVisualSorts
{
    public static float waitTime = 0.015f;

    public static IEnumerator BubbleSort(ListVisual listVisual)
    {
        Color pivotColour = new Color(0, 1, 0);
        Color regularColour = new Color(1, 1, 1);
        Color checkColour = new Color(1, 0, 0);

        if (listVisual.numbers.Count == 0)
        {
            yield break;
        }

        bool swapped = true;
        int possibleSwaps = listVisual.numbers.Count;
        while (swapped)
        {
            listVisual.Highlight(0, pivotColour);

            swapped = false;
            for (int i = 0; i < possibleSwaps - 1; i++)
            {
                yield return new WaitForSecondsRealtime(waitTime);

                if (listVisual.numbers[i] > listVisual.numbers[i + 1])
                {
                    listVisual.SwapNumbers(i, i + 1);
                    swapped = true;
                }
                else
                {
                    listVisual.Highlight(i + 1, pivotColour);
                    listVisual.Highlight(i, regularColour);
                }
            }
            possibleSwaps -= 1;
            listVisual.Highlight(possibleSwaps, checkColour);
        }

        for (int i = 0; i < listVisual.numbers.Count; i++)
        {
            listVisual.Highlight(i, regularColour);
        }
    }

    public static IEnumerator QuickSort(ListVisual listVisual)
    {
        yield return QuickSort(listVisual, 0, listVisual.numbers.Count - 1);
    }

    private static IEnumerator QuickSort(ListVisual listVisual, int low, int high)
    {
        if (low < high)
        {
            int partitionIndex = 0;
            yield return QuickSortPartition(listVisual, low, high, (k) => { partitionIndex = k; });
            yield return QuickSort(listVisual, low, partitionIndex - 1);
            yield return QuickSort(listVisual, partitionIndex + 1, high);
        }
    }

    private static IEnumerator QuickSortPartition(ListVisual listVisual, int low, int high, System.Action<int> callback)
    {
        Color pivotColour = new Color(0, 1, 0);
        Color regularColour = new Color(1, 1, 1);
        Color swappedPivotColour = new Color(1, 0, 0);
        Color checkColour = new Color(0, 0, 1);
        listVisual.Highlight(high, pivotColour);

        int pivot = listVisual.numbers[high];
        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            listVisual.Highlight(j, checkColour);
            listVisual.Highlight(i, swappedPivotColour);

            yield return new WaitForSeconds(waitTime);

            listVisual.Highlight(i, regularColour);
            listVisual.Highlight(j, regularColour);

            if (listVisual.numbers[j] < pivot)
            {
                i++;
                listVisual.SwapNumbers(i, j);
            }
        }

        listVisual.Highlight(high, regularColour);
        listVisual.SwapNumbers(i + 1, high);
        callback(i + 1);
    }

    public static IEnumerator MergeSort(ListVisual listVisual, ListVisual helperListVisual)
    {
        yield return MergeSort(listVisual, 0, listVisual.numbers.Count - 1, helperListVisual);
        helperListVisual.enabled = false;
    }

    private static IEnumerator MergeSort(ListVisual listVisuals, int leftIndex, int rightIndex, ListVisual helperListVisual)
    {
        if (leftIndex >= rightIndex)
        {
            yield break;
        }

        int middleIndex = leftIndex + (rightIndex - leftIndex) / 2;
        yield return MergeSort(listVisuals, leftIndex, middleIndex, helperListVisual);
        yield return MergeSort(listVisuals, middleIndex + 1, rightIndex, helperListVisual);
        yield return Merge(listVisuals, leftIndex, middleIndex, rightIndex, helperListVisual);
    }

    private static IEnumerator Merge(ListVisual listVisual, int leftIndex, int middleIndex, int rightIndex, ListVisual helperListVisual)
    {
        Color pivotColour = new Color(0, 1, 0);
        Color regularColour = new Color(1, 1, 1);
        Color checkColour = new Color(1, 0, 0);

        List<int> leftList = new List<int>();
        List<int> rightList = new List<int>();

        for (int i = leftIndex; i < middleIndex + 1; i++)
        {
            leftList.Add(listVisual.numbers[i]);
        }
        for (int i = middleIndex + 1; i < rightIndex + 1; i++)
        {
            rightList.Add(listVisual.numbers[i]);
        }

        helperListVisual.SetNumbers(new List<int>().Concat(leftList).Concat(rightList).ToList());
        RectTransform helperRectTransform = helperListVisual.GetComponent<RectTransform>();
        int totalRectsCovered = (rightIndex + 1 - listVisual.numbers.Count + leftIndex);
        helperRectTransform.localPosition = new Vector3(
            totalRectsCovered * (listVisual.widthPerBar + listVisual.GetComponent<HorizontalLayoutGroup>().spacing) / 2 + listVisual.GetComponent<RectTransform>().localPosition.x
            , helperRectTransform.localPosition.y);

        int leftSize = leftList.Count;
        int rightSize = rightList.Count;
        int leftCounter = 0;
        int rightCounter = 0;

        while (leftCounter < leftSize && rightCounter < rightSize)
        {
            helperListVisual.Highlight(leftCounter, pivotColour);
            helperListVisual.Highlight(leftSize + rightCounter, pivotColour);
            listVisual.Highlight(leftIndex + leftCounter + rightCounter, checkColour);

            yield return new WaitForSeconds(waitTime);

            helperListVisual.Highlight(leftCounter, regularColour);
            helperListVisual.Highlight(leftSize + rightCounter, regularColour);
            listVisual.Highlight(leftIndex + leftCounter + rightCounter, regularColour);

            if (leftList[leftCounter] < rightList[rightCounter])
            {
                listVisual.SetNumberAtIndex(leftIndex + leftCounter + rightCounter, leftList[leftCounter]);
                leftCounter++;
            }
            else
            {
                listVisual.SetNumberAtIndex(leftIndex + leftCounter + rightCounter, rightList[rightCounter]);
                rightCounter++;
            }
        }

        for (; leftCounter < leftSize; leftCounter++)
        {
            helperListVisual.Highlight(leftCounter, pivotColour);
            listVisual.Highlight(leftIndex + leftCounter + rightCounter, checkColour);

            listVisual.SetNumberAtIndex(leftIndex + leftCounter + rightCounter, leftList[leftCounter]);

            helperListVisual.Highlight(leftCounter, regularColour);
            helperListVisual.Highlight(leftSize + rightCounter, regularColour);
            listVisual.Highlight(leftIndex + leftCounter + rightCounter, regularColour);
        }

        for (; rightCounter < rightSize; rightCounter++)
        {
            helperListVisual.Highlight(leftSize + rightCounter, pivotColour);
            listVisual.Highlight(leftIndex + leftCounter + rightCounter, checkColour);

            listVisual.SetNumberAtIndex(leftIndex + leftCounter + rightCounter, rightList[rightCounter]);

            helperListVisual.Highlight(leftSize + rightCounter, regularColour);
            listVisual.Highlight(leftIndex + leftCounter + rightCounter, regularColour);
        }
        yield return new WaitForSeconds(waitTime);
    }

    public static void BubbleSort(List<int> numbers)
    {
        bool swapped = true;
        while (swapped)
        {
            swapped = false;
            for (int i = 0; i < numbers.Count - 1; i++)
            {
                if (numbers[i] > numbers[i + 1])
                {
                    SwapIndexes(numbers, i, i + 1);
                    swapped = true;
                }
            }
        }
    }

    public static void QuickSort(List<int> numbers)
    {
        QuickSort(numbers, 0, numbers.Count - 1);
    }

    private static void QuickSort(List<int> numbers, int low, int high)
    {
        if (low < high)
        {
            int partitionIndex = QuickSortPartition(numbers, low, high);
            QuickSort(numbers, low, partitionIndex - 1);
            QuickSort(numbers, partitionIndex + 1, high);
        }
    }

    private static int QuickSortPartition(List<int> numbers, int low, int high)
    {
        int pivot = numbers[high];
        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            if (numbers[j] < pivot)
            {
                i++;
                SwapIndexes(numbers, i, j);
            }
        }
        SwapIndexes(numbers, i + 1, high);
        return i + 1;
    }

    public static void MergeSort(List<int> numbers)
    {
        MergeSort(numbers, 0, numbers.Count - 1);
    }

    private static void MergeSort(List<int> numbers, int leftIndex, int rightIndex)
    {
        if (leftIndex >= rightIndex)
        {
            return;
        }

        int middleIndex = leftIndex + (rightIndex - leftIndex) / 2;
        MergeSort(numbers, leftIndex, middleIndex);
        MergeSort(numbers, middleIndex + 1, rightIndex);
        Merge(numbers, leftIndex, middleIndex, rightIndex);
    }

    private static void Merge(List<int> numbers, int leftIndex, int middleIndex, int rightIndex)
    {
        List<int> leftList = new List<int>();
        List<int> rightList = new List<int>();

        for (int i = leftIndex; i < middleIndex + 1; i++)
        {
            leftList.Add(numbers[i]);
        }
        for (int i = middleIndex + 1; i < rightIndex + 1; i++)
        {
            rightList.Add(numbers[i]);
        }

        int leftSize = leftList.Count;
        int rightSize = rightList.Count;
        int leftCounter = 0;
        int rightCounter = 0;

        while (leftCounter < leftSize && rightCounter < rightSize)
        {
            if (leftList[leftCounter] < rightList[rightCounter])
            {
                numbers[leftIndex + leftCounter + rightCounter] = leftList[leftCounter];
                leftCounter++;
            }
            else
            {
                numbers[leftIndex + leftCounter + rightCounter] = rightList[rightCounter];
                rightCounter++;
            }
        }

        for (; leftCounter < leftSize; leftCounter++)
        {
            numbers[leftIndex + leftCounter + rightCounter] = leftList[leftCounter];
        }

        for (; rightCounter < rightSize; rightCounter++)
        {
            numbers[leftIndex + leftCounter + rightCounter] = rightList[rightCounter];
        }
    }

    private static void SwapIndexes(List<int> numbers, int indexOne, int indexTwo)
    {
        int temp = numbers[indexOne];
        numbers[indexOne] = numbers[indexTwo];
        numbers[indexTwo] = temp;
    }

    public static void ShuffleNumbers(List<int> numbers)
    {
        int n = numbers.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n);
            SwapIndexes(numbers, k, n);
        }
    }
}
