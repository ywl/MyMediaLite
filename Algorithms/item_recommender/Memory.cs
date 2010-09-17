// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using System.Linq;
using System.Text;
using MyMediaLite.data_type;
using MyMediaLite.util;

namespace MyMediaLite.item_recommender
{
    /// <summary>
    /// Abstract item recommender class, that loads the training data into two sparse matrices: one column-wise and one row-wise
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public abstract class Memory : ItemRecommender
    {
        /// <summary>
        /// Maximum user ID
        /// </summary>
        protected int max_user_id = 0;
        /// <summary>
        /// Maximum item ID
        /// </summary>
        protected int max_item_id = 0;
        /// <summary>
        /// Data user
        /// </summary>
        protected SparseBooleanMatrix data_user;
        /// <summary>
        /// Data item
        /// </summary>
        protected SparseBooleanMatrix data_item;

		/// <inheritdoc/>
		public override void AddFeedback(int user_id, int item_id)
		{
			if (user_id > max_user_id)
				throw new ArgumentException("Unknown user " + user_id + ". Add it before inserting event data.");
			if (item_id > max_item_id)
				throw new ArgumentException("Unknown item " + item_id + ". Add it before inserting event data.");

			// update data structures
			HashSet<int> user_items = data_user.GetRow(user_id);
			user_items.Add(item_id);
			HashSet<int> item_users = data_item.GetRow(item_id);
			item_users.Add(user_id);
		}

		/// <inheritdoc/>
		public override void RemoveFeedback(int user_id, int item_id)
		{
			if (user_id > max_user_id)
				throw new ArgumentException("Unknown user " + user_id);
			if (item_id > max_item_id)
				throw new ArgumentException("Unknown item " + item_id);

			// update data structures
			HashSet<int> user_items = data_user.GetRow(user_id);
			user_items.Remove(item_id);
			HashSet<int> item_users = data_item.GetRow(item_id);
			item_users.Remove(user_id);
		}

		/// <inheritdoc/>
		public override void AddUser(int user_id)
		{
			if (user_id > max_user_id)
			{
				data_user.GetRow(user_id);
				max_user_id = user_id;
			}
		}

		/// <inheritdoc/>
		public override void AddItem(int item_id)
		{
			if (item_id > max_item_id)
			{
				data_item.GetRow(item_id);
				max_item_id = item_id;
			}
		}

		/// <inheritdoc/>
		public override void RemoveUser(int user_id)
		{
			// remove all mentions of this user from data structures
			HashSet<int> user_items = data_user.GetRow (user_id);
			foreach (int item_id in user_items) {
				data_item.GetRow (item_id).Remove(user_id);
			}
			user_items.Clear();

			if (user_id == max_user_id)
				max_user_id--;
		}

		/// <inheritdoc/>
		public override void RemoveItem(int item_id)
		{
			// remove all mentions of this item from data structures
			HashSet<int> item_users = data_item.GetRow (item_id);
			foreach (int user_id in item_users) {
				data_user.GetRow (user_id).Remove(item_id);
			}
			item_users.Clear();

			if (item_id == max_item_id)
				max_item_id--;
		}

		public void SetCollaborativeData(SparseBooleanMatrix user_items, SparseBooleanMatrix item_users)
		{
            this.data_user = user_items;
            this.data_item = item_users;
			this.max_user_id = user_items.GetNumberOfRows() - 1;
			this.max_item_id = item_users.GetNumberOfRows() - 1;
		}
    }

}