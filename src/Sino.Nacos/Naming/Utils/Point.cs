using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Naming.Utils
{
    public class Point<T>
    {
        private IList<Pair<T>> _itemsWithWeight;

        public IPoller<T> Poller { get; set; }

        public double[] Weights { get; private set; }

        public IList<T> Items { get; private set; }

        public Point(IList<Pair<T>> itemsWithWeight)
        {
            Items = new List<T>();
            Poller = new GenericPoller<T>(Items);
            _itemsWithWeight = itemsWithWeight;
        }

        public void Refresh()
        {
            double originWeightSum = 0;

            foreach(Pair<T> item in _itemsWithWeight)
            {
                double weight = item.Weight;
                if (weight <= 0)
                    continue;

                Items.Add(item.Item);
                if (double.IsInfinity(weight))
                {
                    weight = 10000;
                }
                if (double.IsNaN(weight))
                {
                    weight = 1;
                }
                originWeightSum += weight;
            }

            double[] exactWeights = new double[Items.Count];
            int index = 0;

            foreach(Pair<T> item in _itemsWithWeight)
            {
                double singleWeight = item.Weight;
                if (singleWeight <= 0)
                    continue;

                exactWeights[index++] = singleWeight / originWeightSum;
            }

            Weights = new double[Items.Count];
            double randomRange = 0;
            for (int i = 0; i < index; i++)
            {
                Weights[i] = randomRange + exactWeights[i];
                randomRange += exactWeights[i];
            }

            double doublePrecisionDelta = 0.0001;

            if (index == 0 || (Math.Abs(Weights[index - 1] - 1) < doublePrecisionDelta))
            {
                return;
            }

            throw new InvalidOperationException("Cumulative Weight caculate wrong, the sum of probabilities does not equals 1.");
        }

        public override int GetHashCode()
        {
            return _itemsWithWeight.GetHashCode();
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
            if (!(this.GetType().GetInterfaces()[0].Equals(obj.GetType().GetInterfaces()[0])))
            {
                return false;
            }
            Point<T> otherRef = obj as Point<T>;
            if (_itemsWithWeight == null)
            {
                if (otherRef._itemsWithWeight != null)
                {
                    return false;
                }
            }
            else
            {
                if (otherRef._itemsWithWeight == null)
                {
                    return false;
                }
                else
                {
                    return this._itemsWithWeight.Equals(otherRef._itemsWithWeight);
                }
            }
            return true;
        }
    }
}
