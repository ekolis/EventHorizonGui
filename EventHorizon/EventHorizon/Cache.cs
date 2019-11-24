using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventHorizon
{
	/// <summary>
	/// A cache of data which might take a while to compute.
	/// </summary>
	/// <typeparam name="T">The type of data.</typeparam>
	public class Cache<T>
	{
		private Func<T> computer;
		private T value;
		private bool isStale;

		/// <summary>
		/// Creates a cache with a computation function.
		/// </summary>
		/// <param name="computer">The computation function.</param>
		public Cache(Func<T> computer)
		{
			Computer = computer;
		}

		/// <summary>
		/// Creates a cache with a static initial value.
		/// </summary>
		/// <param name="value">The initial value.</param>
		public Cache(T value)
		{
			Value = value;
		}

		/// <summary>
		/// A computation function to get the value.
		/// </summary>
		public Func<T> Computer
		{
			get { return computer; }
			set
			{
				computer = value;
				isStale = true;
			}
		}

		/// <summary>
		/// The most recently cached value. Setting this will overwrite the computation function with one that returns a constant value.
		/// </summary>
		public T Value
		{
			get
			{
				if (isStale)
					Refresh();
				return value;
			}
			set
			{
				Computer = () => value;
			}
		}

		/// <summary>
		/// Recomputes the data in the cache.
		/// </summary>
		public void Refresh()
		{
			value = Computer();
			isStale = false;
		}

		public static implicit operator T(Cache<T> cache)
		{
			return cache.Value;
		}

		public static implicit operator Cache<T>(T value)
		{
			return new Cache<T>(value);
		}

		public static implicit operator Cache<T>(Func<T> computer)
		{
			return new Cache<T>(computer);
		}
	}
}