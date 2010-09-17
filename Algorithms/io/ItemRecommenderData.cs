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
using System.Globalization;
using System.IO;
using MyMediaLite.data_type;
using MyMediaLite.util;

namespace MyMediaLite.io
{
	public class ItemRecommenderData
	{
		static public Pair<SparseBooleanMatrix, SparseBooleanMatrix> Read(string filename)
		{
            using ( StreamReader reader = new StreamReader(filename) )
			{
				return Read(reader);
			}
		}

		static public Pair<SparseBooleanMatrix, SparseBooleanMatrix> Read(StreamReader reader)
		{
	        SparseBooleanMatrix user_items = new SparseBooleanMatrix();
        	SparseBooleanMatrix item_users = new SparseBooleanMatrix();


			NumberFormatInfo ni = new NumberFormatInfo(); ni.NumberDecimalDigits = '.';
			char[] split_chars = new char[]{ '\t', ' ' };
			string line;

			while (!reader.EndOfStream)
			{
	           	line = reader.ReadLine();
				if (line.Trim().Equals(String.Empty))
					continue;

	            string[] tokens = line.Split(split_chars);

				if (tokens.Length < 2)
					throw new IOException("Expected at least two columns: " + line);

				int user_id = int.Parse(tokens[0]);
				int item_id = int.Parse(tokens[1]);

               	user_items.AddEntry(user_id, item_id);
               	item_users.AddEntry(item_id, user_id);
			}

			return new Pair<SparseBooleanMatrix, SparseBooleanMatrix>(user_items, item_users);
		}
	}
}

