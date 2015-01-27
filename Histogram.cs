using System;
using System.Collections;

namespace MMT.LVDOS
{
	/// <summary>
	/// Histogrammklasse
	/// </summary>
	public class Histogram<T> : IDisposable, IEnumerable, IComparable, ICollection where T : IComparable<T>
	{

		/// <summary>
		/// Gibt an, ob das Objekt freigegeben wurde
		/// </summary>
		private bool		m_boolDisposed = true;

		/// <summary>
		/// Gibt die initiale und kumulative Kapazität des Hashtables an
		/// </summary>
		private int m_intCapacity = 100;

		/// <summary>
		/// Liefert oder setzt die initiale und kumulative Kapazität des Hashtables an
		/// </summary>
		protected int Capacity
		{
			get
			{
				return this.m_intCapacity;
			}
			set
			{
				m_intCapacity = value;
			}
		}

		/// <summary>
		/// Liefert, ob das Objekt gelöscht wurde oder nicht
		/// </summary>
		protected bool IsDisposed
		{
			get
			{
				return this.m_boolDisposed;
			}
		}

		/// <summary>
		/// Liefert die Liste der Prozentsätze der Objekte im Histogramm
		/// </summary>
		/// <returns>Liste der Prozentsätze</returns>
		public float[] GetPercentageList()
		{
			ArrayList sortArray = new ArrayList(this.m_hashHistogramBuckets.Count);
			IDictionaryEnumerator hashEnum = this.m_hashHistogramBuckets.GetEnumerator();
			
			// keys sortieren
			while (hashEnum.MoveNext())
			{
				sortArray.Add(hashEnum.Key);
			}
			sortArray.Sort();

			// Sortierte keys zu Prozentsätzen geben
			ArrayList percArray = new ArrayList(this.m_hashHistogramBuckets.Count);
			for (int i = 0; i < sortArray.Count; ++i )
			{
				percArray.Add(GetPercentage((T)sortArray[i]));
			}

			// Array ausgeben
			return (float[])percArray.ToArray(typeof(float));
		}

		/// <summary>
		/// Liefert die Liste der Prozentsätze der Objekte im Histogramm
		/// </summary>
		/// <param name="averagePercentage">Durchschnittlicher Prozentsatz der Verteilung</param>
		/// <returns>Liste der Prozentsätze</returns>
		public float[] GetPercentageList( out float averagePercentage )
		{
			ArrayList sortArray = new ArrayList(this.m_hashHistogramBuckets.Count);
			IDictionaryEnumerator hashEnum = this.m_hashHistogramBuckets.GetEnumerator();

			// keys sortieren
			while (hashEnum.MoveNext())
			{
				sortArray.Add(hashEnum.Key);
			}
			sortArray.Sort();

			// Sortierte keys zu Prozentsätzen geben
			ArrayList percArray = new ArrayList(this.m_hashHistogramBuckets.Count);
			averagePercentage = 0.0f;
			for (int i = 0; i < sortArray.Count; ++i)
			{
				percArray.Add(GetPercentage((T)sortArray[i]));
				averagePercentage += GetPercentage((T)sortArray[i]);
			}
			averagePercentage /= HistogrammedItemCount;
			averagePercentage *= 100.0f;

			// Array ausgeben
			return (float[])percArray.ToArray(typeof(float));
		}


		/// <summary>
		/// Liefert die Liste der Anzahl der einzelnen Objekte im Histogramm
		/// </summary>
		/// <returns>Liste der Anzahl der Elemente</returns>
		public ulong[] GetCountList()
		{
			ArrayList sortArray = new ArrayList(this.m_hashHistogramBuckets.Count);
			IDictionaryEnumerator hashEnum = this.m_hashHistogramBuckets.GetEnumerator();

			// keys sortieren
			while (hashEnum.MoveNext())
			{
				sortArray.Add(hashEnum.Key);
			}
			sortArray.Sort();

			// Sortierte keys zu Prozentsätzen geben
			ArrayList percArray = new ArrayList(this.m_hashHistogramBuckets.Count / 4);
			for (int i = 0; i < sortArray.Count; ++i)
			{
				percArray.Add((ulong)this.GetCount((T)sortArray[i]));
			}

			// Array ausgeben
			return (ulong[])percArray.ToArray(typeof(ulong));
		}


		/// <summary>
		/// Liefert die Liste der Anzahl der einzelnen Objekte im Histogramm
		/// </summary>
		/// <param name="averageCount">Durchschnittliche Anzahl</param>
		/// <returns>Liste der Anzahl der Elemente</returns>
		public ulong[] GetCountList( out float averageCount )
		{
			ArrayList sortArray = new ArrayList(this.m_hashHistogramBuckets.Count);
			IDictionaryEnumerator hashEnum = this.m_hashHistogramBuckets.GetEnumerator();

			// keys sortieren
			while (hashEnum.MoveNext())
			{
				sortArray.Add(hashEnum.Key);
			}
			sortArray.Sort();

			// Sortierte keys zu Prozentsätzen geben
			ArrayList percArray = new ArrayList(this.m_hashHistogramBuckets.Count / 4);
			averageCount = 0.0f;
			for (int i = 0; i < sortArray.Count; ++i)
			{
				percArray.Add((ulong)this.GetCount((T)sortArray[i]));
				averageCount += (ulong)this.GetCount((T)sortArray[i]);
			}
			averageCount /= this.HistogrammedItemCount;

			// Array ausgeben
			return (ulong[])percArray.ToArray(typeof(ulong));
		}

		/// <summary>
		/// Fügt dem Histogramm einen neuen Messwert hinzu
		/// </summary>
		/// <param name="element"></param>
		protected void AddInternal(T element)
		{
			if (element == null) throw new System.ArgumentNullException("element");
			// Prüfen, ob das Objekt bereits existiert
			if (this.m_hashHistogramBuckets[element] == null)
			{
				// Element existiert nicht
				this.m_hashHistogramBuckets[element] = (ulong)1;
			}
			else
			{
				// Element existiert
				ulong iCount = (ulong)this.m_hashHistogramBuckets[element];
				this.m_hashHistogramBuckets[element] = iCount + 1;
			}

			// Internen zähler erhöhen
			++this.m_intHistogrammedItems;
		}

		/// <summary>
		/// Beinhaltet die Anzahl der histogrammten Einträge
		/// </summary>
		private uint		m_intHistogrammedItems = 0;

		/// <summary>
		/// Liefert die Anzahl der histogrammten Einträge
		/// Alias für Count
		/// </summary>
		public uint HistogrammedItemCount
		{
			get
			{
				return this.m_intHistogrammedItems;
			}
		}

		/// <summary>
		/// Enthält die Buckets, über die das Histogramm erstellt wird
		/// </summary>
		private Hashtable	m_hashHistogramBuckets = null;

		/// <summary>
		/// Erzeugt ein neues Histogramm
		/// </summary>
		public Histogram()
		{
			this.Create();
		}

		/// <summary>
		/// Erzeugt ein neues Histogramm
		/// </summary>
		public Histogram(int capacity)
		{
			this.Create( capacity );
		}

		/// <summary>
		/// Erzeugt das Histogramm
		/// </summary>
		protected void Create()
		{
			this.SetUnDisposed();
			this.m_hashHistogramBuckets = new Hashtable(this.m_intCapacity);
		}


		/// <summary>
		/// Erzeugt das Histogramm
		/// </summary>
		/// <param name="capacity">Gibt die initiale Größe des Histogrammes an; in diesen Schritten wird das Histogramm vergrößert</param>
		protected void Create( int capacity )
		{
			this.m_intCapacity = capacity;
			this.Create();
		}

		/// <summary>
		/// Liefert die Anzahl der Einträge für das gegebene Element
		/// </summary>
		/// <param name="Element">Das gesuchte Element</param>
		/// <returns>Anzahl der Einträge für das gegebene Element</returns>
		protected ulong GetCount( T Element )
		{
			if (Element == null) throw new System.ArgumentNullException("Element");
			try
			{
				if (this.m_hashHistogramBuckets[Element] == null) return 0;
			}
			catch
			{
				try
				{
					return (ulong)this.m_hashHistogramBuckets[Element];
				}
				catch
				{
					return 0;
				}
			}
			return 0;
		}

		/// <summary>
		/// Liefert die prozentuale Verteilung des Objektes
		/// </summary>
		/// <param name="Element">Das gesuchte Element</param>
		/// <returns>Prozentsatz der Häufigkeit des Objektes</returns>
		protected float GetPercentage(T Element)
		{
			if (Element == null) throw new System.ArgumentNullException("Element");
			ulong lHit = this.GetCount(Element);
			float fPercentage = (float)lHit / (float)this.HistogrammedItemCount;
			fPercentage *= 100.0f;
			return fPercentage;
		}


		/// <summary>
		/// Liefert die Anzahl des Elementes mit der höhsten Anzahl
		/// </summary>
		/// <returns>die höchste Anzahl</returns>
		public ulong GetHighestCount()
		{
			ulong lCount = 0;
			IDictionaryEnumerator hashEnum = this.m_hashHistogramBuckets.GetEnumerator();

			// keys sortieren
			while (hashEnum.MoveNext())
			{
				if ((ulong)hashEnum.Value > lCount) lCount = (ulong)hashEnum.Value;
			}
			return lCount;
		}

		/// <summary>
		/// Liefert die prozentuale Vertretung des Elementes mit der höhsten Anzahl
		/// </summary>
		/// <returns>der höchste Prozentsatz</returns>
		public float GetHighestPercentage()
		{
			return (float)this.GetHighestCount() / (float)this.HistogrammedItemCount * 100.0f;
		}

		#region Disposing

				/// <summary>
		/// Destruktor
		/// </summary>
		~Histogram()
		{
			this.Dispose( false );
		}

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="ActiveDispose">Gibt an, ob wir aktiv oder passiv aufgerufen wurden</param>
		protected void Dispose( bool ActiveDispose )
		{
			// Doppeltes Disposing verhindern
			if( !this.m_boolDisposed )
			{
				this.SetDisposed();

				// Wenn ein aktiver Dispose vorliegt, können wir auf andere Objekte verweisen
				if( ActiveDispose )
				{
				}
				// Cleanup
				if( m_hashHistogramBuckets != null ) this.m_hashHistogramBuckets.Clear();
				this.m_hashHistogramBuckets = null;
				this.m_intHistogrammedItems = 0;
			}
		}

		/// <summary>
		/// Leert das Histogramm
		/// </summary>
		public void Clear()
		{
			this.Dispose();
		}

		/// <summary>
		/// Gibt alle verwendeten Ressourcen frei
		/// </summary>
		public void Dispose()
		{
			this.Dispose( true );
		}

		/// <summary>
		/// Markiert dieses Objekt als 'Undisposed', also 'erzeugt'
		/// </summary>
		private void SetUnDisposed()
		{
			this.m_boolDisposed = false;
			GC.ReRegisterForFinalize(this);
		}

		/// <summary>
		/// Markiert dieses Objekt als 'Disposed'
		/// </summary>
		private void SetDisposed()
		{
			this.m_boolDisposed = true;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Stellt sicher, dass die Hashtable allokiert ist
		/// </summary>
		protected void AssureValidObject()
		{
			if (!this.IsDisposed) return;
			this.Create();
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return this.m_hashHistogramBuckets.GetEnumerator();
		}

		#endregion

		public override int GetHashCode()
		{
			return this.m_hashHistogramBuckets.GetHashCode();
		}

		public override string ToString()
		{
			return "Histogram contains " + this.m_intHistogrammedItems.ToString() + " items";
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			try
			{
				return this.HistogrammedItemCount.CompareTo(((Histogram<T>)obj).HistogrammedItemCount);
			}
			catch
			{
				throw new System.ArgumentException();
			}
		}

		#endregion


		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			this.m_hashHistogramBuckets.CopyTo(array, index);
		}

		public int Count
		{
			get
			{
				if (this.HistogrammedItemCount > int.MaxValue) throw new System.Exception("Integer range too small, use HistogrammedItemCount instead");
				return (int)this.HistogrammedItemCount;
			}
		}

		public bool IsSynchronized
		{
			get 
			{
				return this.m_hashHistogramBuckets.IsSynchronized;
			}
		}

		public object SyncRoot
		{
			get 
			{
				return this.m_hashHistogramBuckets.SyncRoot;
			}
		}

		#endregion
	}
}
