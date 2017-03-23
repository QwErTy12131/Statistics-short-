using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace Statistics
{
	public enum KillType
	{
		Mob = 0,
		Boss,
		Player
	}

	public class Database
	{
		private readonly IDbConnection _db;

		internal void CheckUpdateInclude(int userId)
		{
            User Who = TShock.Users.GetUserByID(userId);
			var update = false;
			using (var reader = QueryReader("SELECT Logins FROM Statistics WHERE UserID = @0", userId))
			{
				if (reader.Read())
					update = true;
			}
			if (update)
				Query("UPDATE Statistics SET Logins = Logins + 1 WHERE UserID = @0", userId);
			else
			{
				Query("INSERT INTO Statistics (UserID, Logins, Time, UserName, UserGroup) VALUES (@0, @1, @2, @3, @4)",
					userId, 1, 0, Who.Name, Who.Group);
			}
		}

		internal QueryResult QueryReader(string query, params object[] args)
		{
			return _db.QueryReader(query, args);
		}

		internal int Query(string query, params object[] args)
		{
			return _db.Query(query, args);
		}

		internal void EnsureExists(SqlTable table)
		{
			var creator = new SqlTableCreator(_db,
				_db.GetSqlType() == SqlType.Sqlite
					? (IQueryBuilder) new SqliteQueryCreator()
					: new MysqlQueryCreator());

			creator.EnsureTableStructure(table);
		}

		internal void UpdateTime(int userId, int time)
		{
			var query = string.Format("UPDATE Statistics SET Time = Time + {0} WHERE UserID = @0",
				time);
			Query(query, userId);
		}

		/// <summary>
		/// Returns an array of timespans. 
		/// [0] -> registered time
		/// [1] -> played time
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="logins"></param>
		/// <returns></returns>
		internal TimeSpan[] GetTimes(int userId, ref int logins)
		{
			var ts = new TimeSpan[2];
			using (
				var reader = Statistics.tshock.QueryReader("SELECT Registered FROM Users WHERE ID = @0",
					userId))
			{
				if (reader.Read())
				{
					ts[0] = DateTime.UtcNow -
					        DateTime.ParseExact(reader.Get<string>("Registered"), "s", CultureInfo.CurrentCulture,
						        DateTimeStyles.AdjustToUniversal);
				}
				else
					return null;
			}

			using (var reader = QueryReader("SELECT Time, Logins from Statistics WHERE UserID = @0", userId))
			{
				if (reader.Read())
				{
					ts[1] = new TimeSpan(0, 0, 0, reader.Get<int>("Time"));
					logins = reader.Get<int>("Logins");
				}
				else
					return null;
			}

			return ts;
		}

		internal TimeSpan GetLastSeen(int userId)
		{
			using (var reader = Statistics.tshock.QueryReader("SELECT LastAccessed FROM Users WHERE ID = @0",
				userId))
			{
				if (reader.Read())
				{
					return DateTime.UtcNow -
					       DateTime.ParseExact(reader.Get<string>("LastAccessed"), "s", CultureInfo.CurrentCulture,
						       DateTimeStyles.AdjustToUniversal);
				}
			}
			return TimeSpan.MaxValue;
		}


		private Database(IDbConnection db)
		{
			_db = db;
		}

		public static Database InitDb(string name)
		{
			IDbConnection idb;

			if (TShock.Config.StorageType.ToLower() == "sqlite")
				idb =
					new SqliteConnection(string.Format("uri=file://{0},Version=3",
						Path.Combine(TShock.SavePath, name + ".sqlite")));

			else if (TShock.Config.StorageType.ToLower() == "mysql")
			{
                try
                {
                    var host = TShock.Config.MySqlHost.Split(':');
                    idb = new MySqlConnection
                    {
                        ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword
                            )
                    };
                }
                catch (MySqlException x)
                {
                    TShock.Log.Error(x.ToString());
                    throw new Exception("MySQL not setup correctly.");
                }
			}
			else
				throw new Exception("Invalid storage type.");

			var db = new Database(idb);
			return db;
		}

    }
}
