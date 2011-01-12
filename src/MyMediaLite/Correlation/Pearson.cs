// Copyright (C) 2010, 2011 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Correlation class for Pearson correlation</summary>
	public class Pearson : CorrelationMatrix
	{
		/// <summary>shrinkage parameter</summary>
		public float shrinkage = 10;

		/// <summary>Constructor. Create a Pearson correlation matrix</summary>
		/// <param name="num_entities">the number of entities</param>
		public Pearson(int num_entities) : base(num_entities) { }

		/// <summary>
		/// Create a Pearson correlation matrix from given data
		/// </summary>
		/// <param name="ratings">the ratings data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		/// <param name="shrinkage">a shrinkage parameter</param>
		/// <returns>the complete Pearson correlation matrix</returns>
		static public CorrelationMatrix Create(RatingData ratings, EntityType entity_type, float shrinkage)
		{
			Pearson cm;
			int num_entities = 0;
			if (entity_type.Equals(EntityType.USER))
				num_entities = ratings.MaxUserID + 1;
			else if (entity_type.Equals(EntityType.ITEM))
				num_entities = ratings.MaxItemID + 1;
			else
				throw new ArgumentException("Unknown entity type: " + entity_type);

			try
			{
				cm = new Pearson(num_entities);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}
			cm.shrinkage = shrinkage;
			cm.ComputeCorrelations(ratings, entity_type);
			return cm;
		}

		/// <summary>Compute correlations between two entities for given ratings</summary>
		/// <param name="ratings1">the rating data for entity 1</param>
		/// <param name="ratings2">the rating data for entity 2</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		/// <param name="i">the ID of first entity</param>
		/// <param name="j">the ID of second entity</param>
		/// <param name="shrinkage">the shrinkage parameter</param>
		public static float ComputeCorrelation(Ratings ratings1, Ratings ratings2, EntityType entity_type, int i, int j, float shrinkage)
		{
			if (i == j)
				return 1;

			// get common ratings for the two entities
			HashSet<int> e1 = entity_type == EntityType.USER ? ratings1.GetItems() : ratings1.GetUsers();
			HashSet<int> e2 = entity_type == EntityType.USER ? ratings2.GetItems() : ratings2.GetUsers();

			e1.IntersectWith(e2);

			int n = e1.Count;
			
			Console.WriteLine("compare computation {0}, {1} : {2}", i, j, n);
			
			if (n < 2) // TODO reconsider this
				return 0;

			// single-pass variant
			double i_sum = 0;
			double j_sum = 0;
			double ij_sum = 0;
			double ii_sum = 0;
			double jj_sum = 0;
			foreach (int other_entity_id in e1)
			{
				// get ratings
				double rating_i = 0;
				double rating_j = 0;
				if (entity_type == EntityType.USER)
				{
					rating_i = ratings1.FindRating(i, other_entity_id).rating;
					rating_j = ratings2.FindRating(j, other_entity_id).rating;
				}
				else
				{
					rating_i = ratings1.FindRating(other_entity_id, i).rating;
					rating_j = ratings2.FindRating(other_entity_id, j).rating;
				}

				// update sums
				i_sum  += rating_i;
				j_sum  += rating_j;
				ij_sum += rating_i * rating_j;
				ii_sum += rating_i * rating_i;
				jj_sum += rating_j * rating_j;
			}
				
			double denominator = Math.Sqrt((n * ii_sum - i_sum * i_sum) * (n * jj_sum - j_sum * j_sum));
			
			if (denominator == 0)
				return 0;
			double pmcc = (n * ij_sum - i_sum * j_sum) / denominator;
			
			Console.WriteLine("ij_sum * n - i_sum * j_sum = {0} * {1} - {2} * {3} = {4}", ij_sum, n, i_sum, j_sum, n * ij_sum - i_sum * j_sum);			
			
			return (float) pmcc * (n / (n + shrinkage));
		}

		/// <summary>Compute correlations for given ratings</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		public override void ComputeCorrelations(RatingData ratings, EntityType entity_type)
		{
			if (entity_type != EntityType.USER && entity_type != EntityType.ITEM)
				throw new ArgumentException("entity type must be either USER or ITEM, not " + entity_type);

			List<Ratings> ratings_by_entity = (entity_type == EntityType.USER) ? ratings.ByItem : ratings.ByUser;

			var freqs   = new SparseMatrix<int>(num_entities);
			var i_sums  = new SparseMatrix<double>(num_entities);
			var j_sums  = new SparseMatrix<double>(num_entities);
			var ij_sums = new SparseMatrix<double>(num_entities);			
			var ii_sums = new SparseMatrix<double>(num_entities);			
			var jj_sums = new SparseMatrix<double>(num_entities);			
		
			foreach (var other_entity_ratings in ratings_by_entity)
				for (int i = 0; i < other_entity_ratings.Count; i++)
				{
					var r1 = other_entity_ratings[i];
					int x = (entity_type == EntityType.USER) ? r1.user_id : r1.item_id;

					// update pairwise scalar product and frequency
	        		for (int j = i + 1; j < other_entity_ratings.Count; j++)
					{
						var r2 = other_entity_ratings[j];
						int y = (entity_type == EntityType.USER) ? r2.user_id : r2.item_id;

						// ensure x < y
						if (x > y)
						{
							int tmp = x;
							x = y;
							y = tmp;
						}

						freqs[x, y]   += 1;
						i_sums[x, y]  += r1.rating;
						j_sums[x, y]  += r2.rating;
						ij_sums[x, y] += r1.rating * r2.rating;
						ii_sums[x, y] += r1.rating * r1.rating;
						jj_sums[x, y] += r2.rating * r2.rating;
	        		}
				}

			// the diagonal of the correlation matrix
			for (int i = 0; i < num_entities; i++)
				this[i, i] = 1;

			// fill the entries with interactions
			foreach (var index_pair in freqs.NonEmptyEntryIDs)
			{
				int x = index_pair.First;
				int y = index_pair.Second;
				int n = freqs[x, y];
				if (x == 0 && y == 1)
					Console.WriteLine("{0}, {1} : {2}", x, y, n);

				if (n < 2) // TODO reconsider this
				{
					this[x, y] = 0;
					continue;
				}

				double numerator = ij_sums[x, y] * n - i_sums[x, y] * j_sums[x, y];
				
				if (x == 0 && y == 1)
					Console.WriteLine("this[x,y] * n - sums[x] * sums[y] = {0} * {1} - {2} * {3} = {4}", this[x,y], n, i_sums[x, y], j_sums[x, y], numerator);
				
				double denominator = Math.Sqrt( (n * ii_sums[x, y] - i_sums[x, y] * i_sums[x, y]) * (n * jj_sums[x, y] - j_sums[x, y] *j_sums[x, y]) );
				if (denominator == 0)
				{
					this[x, y] = 0;
					continue;
				}
					
				double pmcc = numerator / denominator;

				if (x == 0 && y == 1)
					Console.WriteLine("{0}/{1} = {2}", numerator, denominator, pmcc);

				
				if (x == 0 && y == 1)
				{
					float pmcc2 = ComputeCorrelation(ratings.ByUser[x], ratings.ByUser[y], entity_type, x, y, shrinkage);
					Console.WriteLine("compare with {0}", pmcc2);
				}
				
				this[x, y] = (float) (pmcc * (n / (n + shrinkage)));
			}
		}
	}
}