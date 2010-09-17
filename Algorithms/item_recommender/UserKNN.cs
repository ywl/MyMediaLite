// Copyright (C) 2010 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MyMediaLite.correlation;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
    /// <summary>
    /// k-nearest neighbor user-based collaborative filtering using cosine-similarity
    /// k=inf equals most-popular.
    ///
    /// This engine does not support online updates.
    /// </summary>
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class UserKNN : KNN
    {
		protected int[][] nearest_neighbors;

        /// <inheritdoc />
        public override void Train()
        {
            int num_users = max_user_id + 1;
			correlation = new Cosine(num_users);
			correlation.ComputeCorrelations(data_user);

			if (k != UInt32.MaxValue)
			{
				nearest_neighbors = new int[max_user_id + 1][];
				for (int u = 0; u <= max_user_id; u++)
					nearest_neighbors[u] = correlation.GetNearestNeighbors(u, k);
			}
        }

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id > max_user_id))
                throw new ArgumentException("user is unknown: " + user_id);
            if ((item_id < 0) || (item_id > max_item_id))
                throw new ArgumentException("item is unknown: " + item_id);

			int count = 0;
			foreach (int neighbor in nearest_neighbors[user_id])
				if (data_user.GetRow(neighbor).Contains(item_id))
					count++;
			return (double) count / k;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("user-kNN, k={0}",
			                     k == UInt32.MaxValue ? "inf" : k.ToString());
		}
    }

    /// <summary>
    /// Weighted k-nearest neighbor user-based collaborative filtering using cosine-similarity
    ///
    /// This engine does not support online updates.
    /// </summary>
	/// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class WeightedUserKNN : UserKNN
    {
        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id > max_user_id))
                throw new ArgumentException("user is unknown: " + user_id);
            if ((item_id < 0) || (item_id > max_item_id))
                throw new ArgumentException("item is unknown: " + item_id);

			if (k == UInt32.MaxValue)
			{
				return correlation.SumUp(user_id, data_item.GetRow(item_id));
			}
			else
			{
				double result = 0;
				foreach (int neighbor in nearest_neighbors[user_id])
					if (data_user.GetRow(neighbor).Contains(item_id))
						result += correlation.Get(user_id, neighbor);
				return result;
			}
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("weighted-user-kNN k={0}",
			                     k == UInt32.MaxValue ? "inf" : k.ToString());
		}
    }
}