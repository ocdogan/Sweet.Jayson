using System;
using System.Threading;

namespace Jayson
{
	# region JaysonStackList

    internal sealed class JaysonStackList
	{
		# region Constants

		private const int QueueLength = 20;

		private const int FrameSize = 20;
		private const int FrameLength = 1;
		private static readonly int FrameSizeMin1 = FrameSize - 1;

		# endregion Constants

		# region Frame

		private class Frame
		{
			public readonly object[] Items = new object[FrameSize];
		}

		# endregion Frame

		# region Static Members

		private static int s_QueueIndex = -1;
		private static object s_QueueLock = new object();
		private static JaysonStackList[] s_Queue = new JaysonStackList[QueueLength];

		# endregion Static Members

		private int m_Count = 0;
		private int m_ItemIndex = -1;
		private int m_FrameIndex;
		private object[] m_CurrentItems;
		private int m_FrameCount = FrameLength;
		private int m_FrameCountMin1 = FrameLength - 1;

		private Frame[] m_Frames = new Frame[FrameLength] { new Frame() };

		private JaysonStackList()
		{
			Init();
		}

		private void Init()
		{
			m_Count = 0;
			m_ItemIndex = -1;
			m_FrameIndex = 0;
			m_CurrentItems = m_Frames[0].Items;
		}

		public static JaysonStackList Get()
		{
			lock (s_QueueLock)
			{
				int index = Interlocked.Decrement(ref s_QueueIndex);
				if (index > -1)
				{
					return s_Queue[index];
				}
				Interlocked.Exchange(ref s_QueueIndex, -1);
			}
			return new JaysonStackList();
		}

		public static void Release(JaysonStackList stack)
		{
			if (stack != null)
			{
				stack.Init();
				lock (s_QueueLock)
				{
					int index = Interlocked.Increment(ref s_QueueIndex);
					if (index >= QueueLength)
					{
						Interlocked.Exchange(ref s_QueueIndex, QueueLength);
						return;
					}
					s_Queue[index] = stack;
				}
			}
		}

		private void EnsureCapacity(int min)
		{
			if (m_FrameCount < min)
			{
				Frame[] curFrames = m_Frames;
				Frame[] tmpFrames = new Frame[min];

				Array.Copy(curFrames, tmpFrames, m_FrameCount);

				int toFrame = m_FrameCount - 1;
				for (int i = tmpFrames.Length - 1; i > toFrame; i--)
				{
					tmpFrames[i] = new Frame();
				}

				m_Frames = tmpFrames;
				m_FrameCount += FrameLength;
				m_FrameCountMin1 = m_FrameCount - 1;
			}
		}

		public int Count
		{
			get { return m_Count; }
		}

		public void Push(object obj)
		{
			if ((m_ItemIndex == FrameSizeMin1) && (m_FrameIndex == m_FrameCountMin1))
			{
				EnsureCapacity(m_FrameCount + FrameLength);
			}

			if (++m_ItemIndex == FrameSize)
			{
				m_ItemIndex = 0;
				m_FrameIndex++;
				m_CurrentItems = m_Frames[m_FrameIndex].Items;
			}

			m_CurrentItems[m_ItemIndex] = obj;
			m_Count++;
		}

		public void Pop()
		{
			if (m_Count > 0)
			{
				m_CurrentItems[m_ItemIndex] = null;
				m_Count--;

				if (--m_ItemIndex < 0)
				{
					m_ItemIndex = 0;
					if (--m_FrameIndex < 0)
					{
						m_FrameIndex = 0;
					}
				}
			}
		}

		public bool Contains(object obj)
		{
			if (obj != null)
			{
				for (int j = m_ItemIndex; j > -1; j--)
				{
					if (obj == m_CurrentItems[j])
						return true;
				}

				if (m_FrameIndex > 0)
				{
					object[] items;
					for (int i = m_FrameIndex - 1; i > -1; i--)
					{
						items = m_Frames[i].Items;
						for (int j = FrameSizeMin1; j > -1; j--)
						{
							if (obj == items[j])
								return true;
						}
					}
				}
			}
			return false;
		}
	}

	# endregion JaysonStackList
}