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

namespace MyMediaLite.data_type
{
	// TODO move some of the similarity measures/norms/distances here

	public class VectorUtils
	{
		public static double EuclideanNorm(ICollection<double> vector)
		{
			double norm = 0;
			foreach (double v in vector)
				norm += Math.Pow(v, 2);
			return norm;
		}

        static public void InitNormal(IList<double> vector, double mean, double stdev)
        {
            var rnd = MyMediaLite.util.Random.GetInstance();
            for (int i = 0; i < vector.Count; i++)
            	vector[i] = rnd.NextNormal(mean, stdev);
        }
	}
}