using System;
using System.Collections.Generic;

public class ReactiveVariable<T>
{
    public event Action<T, T> OnValueChanged;

    private T value;
    private IEqualityComparer<T> comparer;

    public ReactiveVariable() : this(default(T), EqualityComparer<T>.Default)
    {
    }

    public ReactiveVariable(T value) : this(value, EqualityComparer<T>.Default)
    {
    }

    public ReactiveVariable(T value, IEqualityComparer<T> comparer)
    {
        this.value = value;
        this.comparer = comparer;
    }

    public T Value
    {
        get => value;
        set
        {
            T oldValue = this.value;

            this.value = value;

            if (!comparer.Equals(value, oldValue))
            {
                OnValueChanged?.Invoke(oldValue, this.value);
            }
        }
    }
}