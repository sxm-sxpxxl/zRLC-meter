using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static class NativeSpanExtensions
{
    public static unsafe ReadOnlySpan<T> GetReadOnlySpan<T>(this NativeArray<T> array) where T : unmanaged
    {
        void* ptr = array.GetUnsafeReadOnlyPtr();
        return new Span<T>(ptr, array.Length);
    }
}
