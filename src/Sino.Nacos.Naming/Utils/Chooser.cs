using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Utils
{
    public class Chooser<K, T> where T:class
    {
        private K _uniqueKey;
        private volatile Point<T> _point;
        private Random _random;

        public Chooser(K uniqueKey)
            : this(uniqueKey, new List<Pair<T>>()) { }

        public Chooser(K uniqueKey, IList<Pair<T>> pairs)
        {
            Point<T> point = new Point<T>(pairs);
            point.Refresh();
            _uniqueKey = uniqueKey;
            _point = point;
            _random = new Random();
        }

        public T Random()
        {
            var items = _point.Items;
            if (items.Count <= 0)
                return null;
            if (items.Count == 1)
                return items[0];

            return items[_random.Next(0, items.Count - 1)];
        }

        public T RandomWithWeight()
        {
            var point = _point;
            double random = _random.NextDouble();
            int index = BinarySearch(point.Weights, random);
            if (index < 0)
            {
                index = -index - 1;
            }
            else
            {
                return point.Items[index];
            }

            if (index >= 0 && index < point.Weights.Length)
            {
                if (random < point.Weights[index])
                {
                    return point.Items[index];
                }
            }

            return point.Items[point.Items.Count - 1];
        }

        public K GetUniqueKey()
        {
            return _uniqueKey;
        }

        public Point<T> GetPoint()
        {
            return _point;
        }

        public void Refresh(IList<Pair<T>> itemsWithWeight)
        {
            var newPoint = new Point<T>(itemsWithWeight);
            newPoint.Refresh();
            newPoint.Poller = _point.Poller.Refresh(newPoint.Items);
            _point = newPoint;
        }

        private int BinarySearch(double[] a, double key)
        {
            int low = 0;
            int high = a.Length - 1;

            while(low <= high)
            {
                int mid = RightMove(low + high, 1);
                double midVal = a[mid];

                if (midVal < key)
                {
                    low = mid + 1;
                }
                else if (midVal > key)
                {
                    high = mid - 1;
                }
                else
                {
                    if (midVal == key)
                        return mid;
                    else if (midVal < key)
                        low = mid + 1;
                    else
                        high = mid - 1;
                }
            }
            return -(low + 1);
        }

        /// <summary>
        /// 实现无符号右移
        /// </summary>
        private int RightMove(int value, int pos)
        {
            if (pos != 0)
            {
                int mask = int.MaxValue;
                value = value >> 1;
                value = value & mask;
                value = value >> pos - 1;
            }
            return value;
        }

        public override int GetHashCode()
        {
            return _uniqueKey.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            Chooser<K, T> otherChooser = obj as Chooser<K ,T>;
            if (this._uniqueKey == null)
            {
                if (otherChooser.GetUniqueKey() != null)
                {
                    return false;
                }
            }
            else
            {
                if (otherChooser.GetUniqueKey() == null)
                {
                    return false;
                }
                else if (!this._uniqueKey.Equals(otherChooser.GetUniqueKey()))
                {
                    return false;
                }
            }

            if (this._point == null)
            {
                if (otherChooser.GetPoint() != null)
                {
                    return false;
                }
            }
            else
            {
                if (otherChooser.GetPoint() == null)
                {
                    return false;
                }
                else if (!this._point.Equals(otherChooser.GetPoint()))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
