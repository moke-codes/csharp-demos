
int[] collection = [3, 34, 8, 83, 7, 38, 23, 1, 93];
Quicksort(collection.AsSpan());
foreach (var i in collection)
{
    Console.Write($"{i} ");
}

Console.ReadLine();

void Quicksort<T>(Span<T> list)
{
    if (list.Length <= 1)
    {
        return;
    }
    
    var pivotIndex = Partition(list);
    Quicksort(list[..pivotIndex]);
    Quicksort(list[(pivotIndex + 1)..]);
}

int Partition<T>(Span<T> list)
{
    var comparer = Comparer<T>.Default;
    var pivot = list[^1];
    int i = -1;

    for (int j = 0; j < list.Length - 1; j++)
    {
        if (comparer.Compare(list[j], pivot) < 0)
        {
            i++;
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    (list[i + 1], list[^1]) = (list[^1], list[i + 1]);

    return i + 1;
}

