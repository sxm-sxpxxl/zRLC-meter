using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// Расширения для работы с нативным массивом. Используется для взаимодействия с оберткой аудио-библиотеки libsoundio.
/// Не рекомендую здесь останавливаться, т.к. требует углубленного понимания.
/// </summary>
public static class NativeArrayExtensions
{
    public static unsafe ReadOnlySpan<T> GetReadOnlySpan<T>(this NativeArray<T> array) where T : unmanaged
    {
        void* ptr = array.GetUnsafeReadOnlyPtr();
        return new Span<T>(ptr, array.Length);
    }
}
