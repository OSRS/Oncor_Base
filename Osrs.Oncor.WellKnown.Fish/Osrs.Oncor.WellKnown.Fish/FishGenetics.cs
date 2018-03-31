//Copyright 2017 Open Science, Engineering, Research and Development Information Systems Open, LLC. (OSRS Open)
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Osrs.Numerics;
using Osrs.Runtime;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Osrs.Oncor.WellKnown.Fish
{
    public sealed class FishGenetics
    {
        //FishId, GeneticSampleId(String), LabSampleId(String), Description, StockEstimates
        public Guid Identity
        {
            get;
        }

        private Guid fishId;
        public Guid FishId
        {
            get { return this.fishId; }
        }

        public string GeneticSampleId
        {
            get;
            set;
        }

        public string LabSampleId
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public StockEstimates StockEstimates
        {
            get;
        }

        public FishGenetics(Guid id, Guid fishId, string geneticSampleId, string labSampleId, StockEstimates estimates, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.Assert(!Guid.Empty.Equals(fishId), nameof(fishId));
            MethodContract.NotNull(estimates, nameof(estimates));
            this.Identity = id;
            this.fishId = fishId;
            this.GeneticSampleId = geneticSampleId;
            this.LabSampleId = labSampleId;
            this.StockEstimates = estimates;
            this.Description = description;
        }

        public bool Equals(FishGenetics other)
        {
            if (other != null)
                return this.FishId.Equals(other.FishId);
            return false;
        }
    }

    public sealed class StockEstimate
    {
        //Stock(String), Probability(float)
        public string Stock
        {
            get;
        }

        public float Probability
        {
            get;
        }

        private StockEstimate(string stock, float probability)
        {
            this.Stock = stock;
            this.Probability = probability;
        }

        internal static StockEstimate Create(string stock, float probability)
        {
            if (string.IsNullOrEmpty(stock))
                return null;
            if (probability < 0 || probability > 1.0 || MathUtils.IsInfiniteOrNaN(probability))
                return null;

            return new StockEstimate(stock.Trim(), probability);
        }

        public bool SameStock(StockEstimate other)
        {
            if (other != null)
                return this.SameStock(other.Stock);
            return false;
        }

        public bool SameStock(string stockName)
        {
            if (!string.IsNullOrEmpty(stockName))
            {
                return this.Stock.ToLowerInvariant().Equals(stockName.Trim().ToLowerInvariant());
            }
            return false;
        }
    }

    public sealed class StockEstimates : IEnumerable<StockEstimate> //as a list
    {
        //stock names must be unique - can't have exactly the same stock with multiple estimates
        //can't have "holes" must be compact from ordinal 1 -> N  generally, less than 4 - so performance isn't an issue per fish
        //generally this is immutable overall
        private List<StockEstimate> estimates = new List<StockEstimate>();

        public int Count
        {
            get { return this.estimates.Count; }
        }

        internal void Add(StockEstimate estimate)
        {
            if (estimate != null && !this.Exists(estimate.Stock))
                this.estimates.Add(estimate);
        }

        public bool Exists(string stockName)
        {
            if (!string.IsNullOrEmpty(stockName))
            {
                stockName = stockName.Trim().ToLowerInvariant();
                foreach (StockEstimate cur in this.estimates)
                {
                    if (cur.SameStock(stockName))
                        return true;
                }
            }
            return false;
        }

        public IEnumerator<StockEstimate> GetEnumerator()
        {
            return this.estimates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public float this[string stockName]
        {
            get
            {
                if (!string.IsNullOrEmpty(stockName))
                {
                    stockName = stockName.Trim().ToLowerInvariant();
                    foreach (StockEstimate cur in this.estimates)
                    {
                        if (cur.SameStock(stockName))
                            return cur.Probability;
                    }
                }
                return 0.0f;
            }
            set
            {
                if (!MathUtils.IsInfiniteOrNaN(value) && value >= 0 && value <= 1.0)
                {
                    if (!string.IsNullOrEmpty(stockName))
                    {
                        stockName = stockName.Trim().ToLowerInvariant();
                        if (stockName.Length > 0)
                        {
                            for (int i = 0; i < this.estimates.Count; i++)
                            {
                                StockEstimate cur = this.estimates[i];
                                if (cur.SameStock(stockName))
                                {
                                    this.estimates[i] = StockEstimate.Create(cur.Stock, value);
                                    return;
                                }
                            }
                            //ok, not in there
                            this.estimates.Add(StockEstimate.Create(stockName, value));
                        }
                    }
                }
            }
        }

        public StockEstimates()
        { }
    }
}
