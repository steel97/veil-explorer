namespace ExplorerBackend.Core.Node;


#pragma warning disable CA2018
public class ArrayHelpers
{
    public static T[] ConcatArrays<T>(params T[][] arrays)
    {
        ArgumentNullException.ThrowIfNull(arrays);
        arrays.ToList().ForEach(el => { if (el == null) throw new Exception(""); });

        //Contract.Ensures(Contract.Result<T[]>() != null);
        //Contract.Ensures(Contract.Result<T[]>().Length == arrays.Sum(arr => arr.Length));

        var result = new T[arrays.Sum(arr => arr.Length)];
        int offset = 0;
        for (int i = 0; i < arrays.Length; i++)
        {
            var arr = arrays[i];
            Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
            offset += arr.Length;
        }
        return result;
    }

    public static T[] ConcatArrays<T>(T[] arr1, T[] arr2)
    {
        //Contract.Ensures(Contract.Result<T[]>() != null);
        ArgumentNullException.ThrowIfNull(arr1);
        ArgumentNullException.ThrowIfNull(arr2);
        //Contract.Ensures(Contract.Result<T[]>().Length == arr1.Length + arr2.Length);

        var result = new T[arr1.Length + arr2.Length];
        Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
        Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);
        return result;
    }

    public static T[] SubArray<T>(T[] arr, int start, int length)
    {
        //Contract.Requires(arr != null);
        //Contract.Requires(start >= 0);
        //Contract.Requires(length >= 0);
        if (start < 0 || length < 0) throw new Exception("");
        ArgumentNullException.ThrowIfNull(arr);
        //Contract.Requires(start + length <= arr.Length);
        //Contract.Ensures(Contract.Result<T[]>() != null);
        //Contract.Ensures(Contract.Result<T[]>().Length == length);

        var result = new T[length];
        Buffer.BlockCopy(arr, start, result, 0, length);
        return result;
    }

    public static T[] SubArray<T>(T[] arr, int start)
    {
        //Contract.Requires(arr != null);
        //Contract.Requires(start >= 0);
        if (start < 0) throw new Exception("");
        ArgumentNullException.ThrowIfNull(arr);
        //Contract.Requires(start <= arr.Length);
        //Contract.Ensures(Contract.Result<T[]>() != null);
        //Contract.Ensures(Contract.Result<T[]>().Length == arr.Length - start);

        return SubArray(arr, start, arr.Length - start);
    }
}
#pragma warning restore CA2018