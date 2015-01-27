using System;
using System.Collections;
using MMT.LVDOS;

namespace MMT.LVDOS.LVDMath
{
	/// <summary>
	/// Histogrammklasse
	/// </summary>
	public class NumericHistogram : Histogram<float>
	{
		private float m_mean = 0;

		public float Mean
		{
			get { return m_mean; }
		}
		private float m_median = 0;

		public float Median
		{
			get { return m_median; }
		}
		private float m_stdDev = 0;

		public float StandardAbweichung
		{
			get { return m_stdDev; }
		}
		private float m_total = 0;

		public float Total
		{
			get { return m_total; }
		}
		private float m_max = 0;

		public float Max
		{
			get { return m_max; }
		}
		private float m_min = 0;

		public float Min
		{
			get { return m_min; }
		}

		/// <summary>
		/// Erzeugt ein neues Histogramm
		/// </summary>
		public NumericHistogram()
		{
			this.Create();
		}

		/// <summary>
		/// Erzeugt ein neues Histogramm
		/// </summary>
		public NumericHistogram( int capacity )
		{
			this.Create();
		}

		/// <summary>
		/// Erzeugt ein neues Histogramm
		/// </summary>
		public NumericHistogram( float[] values )
			: this()
		{
			m_max = float.MinValue;
			m_min = float.MaxValue;

			// Werte samplen
			for (int i = 0; i < values.Length; ++i)
			{
				float value = values[i];
				// Samplen
				AddInternal(value);

				// Statistik
				if (value > m_max) m_max = value;
				if (value < m_min) m_min = value;

				m_total += value;
				m_mean += i * value; // Bucket * Anzahl
			}

			// Durchschnitt
			m_mean /= m_total;

			// Standardabweichung berechnen
			for (int i = 0; i < values.Length; ++i)
			{
				float value = values[i];
				m_stdDev += (float)System.Math.Pow(i - m_mean, 2) * value;
			}
			m_stdDev = (float)System.Math.Sqrt(m_stdDev / m_total);

			// Median berechnen

			// calculate median
			float h = m_total / 2;
			float v;
			for (m_median = 0, v = 0; m_median < values.Length; m_median++)
			{
				v += values[(int)m_median];
				if (v >= h)	break;
			}
		}

		private bool m_bInSampling = false;

		public void BeginSampling()
		{
			this.Create();
			m_bInSampling = true;
		}

		public void Add(float value)
		{
			if (!m_bInSampling) throw new Exception("Missing call to BeginSampling()");
			AddInternal(value);
		}

		public void EndSampling()
		{
			if (!m_bInSampling) throw new Exception("Missing call to BeginSampling()");
			m_bInSampling = false;

			m_max = float.MinValue;
			m_min = float.MaxValue;

			ulong[] values = GetCountList();

			// Werte samplen
			for (int i = 0; i < values.Length; ++i)
			{
				float value = values[i];

				// Statistik
				if (value > m_max) m_max = value;
				if (value < m_min) m_min = value;

				m_total += value;
				m_mean += i * value; // Bucket * Anzahl
			}

			// Durchschnitt
			m_mean /= m_total;

			// Standardabweichung berechnen
			for (int i = 0; i < values.Length; ++i)
			{
				float value = values[i];
				m_stdDev += (float)System.Math.Pow(i - m_mean, 2) * value;
			}
			m_stdDev = (float)System.Math.Sqrt(m_stdDev / m_total);

			// Median berechnen

			// calculate median
			float h = m_total / 2;
			float v;
			for (m_median = 0, v = 0; m_median < values.Length; m_median++)
			{
				v += values[(int)m_median];
				if (v >= h) break;
			}
		}
	}
}
