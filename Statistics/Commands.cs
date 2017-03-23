using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace Statistics
{
	public static class Commands
	{
		public static void Core(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax. /info [flag] <player name>");
				args.Player.SendErrorMessage(
					"Valid flags: -t : time, -s : seen, -ts : time + seen");
				return;
			}

			switch (args.Parameters[0].ToLowerInvariant())
			{
				case "-t":
				case "-time":
				{
					var logins = 1;
					if (args.Parameters.Count < 2)
					{
						var times = Statistics.database.GetTimes(args.Player.User.ID, ref logins);
						if (times == null)
							args.Player.SendErrorMessage("Unable to discover your times. Sorry.");
						else
						{
							var total = times[1].Add(new TimeSpan(0, 0, 0, Statistics.TimeCache[args.Player.Index]));
							args.Player.SendSuccessMessage("You have played for {0}.", total.SToString());
							args.Player.SendSuccessMessage("You have been registered for {0}.", times[0].SToString());
							args.Player.SendSuccessMessage("You have logged in {0} times.", logins);
						}
					}
					else
					{
						var name = args.Parameters[1];
						var users = GetUsers(name);
						if (users.Count > 1)
						{
							args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
								name, string.Join(", ", users.Select(u => u.Name)));
							break;
						}
						if (users.Count == 0)
						{
							args.Player.SendErrorMessage("No users matched your search '{0}'", name);
							break;
						}

						var user = users[0];

						var times = Statistics.database.GetTimes(user.ID, ref logins);
						if (times == null)
							args.Player.SendErrorMessage("Unable to discover the times of {0}. Sorry.",
								user.Name);
						else
						{
							args.Player.SendSuccessMessage("{0} has played for {1}.", user.Name,
								times[1].SToString());
							args.Player.SendSuccessMessage("{0} has been registered for {1}.", user.Name,
								times[0].SToString());
							args.Player.SendSuccessMessage("{0} has logged in {1} times.", user.Name, logins);
						}
					}
					break;
				}
				case "-s":
				case "-seen":
				{
					if (args.Parameters.Count < 2)
						args.Player.SendErrorMessage("Invalid syntax. /info -s [player name]");
					else
					{
						var name = args.Parameters[1];
						var users = GetUsers(name);
						if (users.Count > 1)
						{
							args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
								name, string.Join(", ", users.Select(u => u.Name)));
							break;
						}
						if (users.Count == 0)
						{
							args.Player.SendErrorMessage("No users matched your search '{0}'", name);
							break;
						}

						var user = users[0];
						var seen = Statistics.database.GetLastSeen(user.ID);
						if (seen == TimeSpan.MaxValue)
							args.Player.SendErrorMessage("Unable to find {0}'s last login time.",
								user.Name);
						else
							args.Player.SendSuccessMessage("{0} last logged in {1} ago.", user.Name, seen.SToString());
					}
					break;
				}
                case "-ts":
                    var logins2 = 1;
                    if (args.Parameters.Count < 2)
                    {
                        var times = Statistics.database.GetTimes(args.Player.User.ID, ref logins2);
                        if (times == null)
                            args.Player.SendErrorMessage("Unable to discover your times. Sorry.");
                        else
                        {
                            var total = times[1].Add(new TimeSpan(0, 0, 0, Statistics.TimeCache[args.Player.Index]));
                            args.Player.SendSuccessMessage("You have played for {0}.", total.SToString());
                            args.Player.SendSuccessMessage("You have been registered for {0}.", times[0].SToString());
                            args.Player.SendSuccessMessage("You have logged in {0} times.", logins2);
                        }
                    }
                    else
                    {
                        var name = args.Parameters[1];
                        var users = GetUsers(name);
                        if (users.Count > 1)
                        {
                            args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
                                name, string.Join(", ", users.Select(u => u.Name)));
                            break;
                        }
                        if (users.Count == 0)
                        {
                            args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                            break;
                        }

                        var user = users[0];

                        var times = Statistics.database.GetTimes(user.ID, ref logins2);
                        if (times == null)
                            args.Player.SendErrorMessage("Unable to discover the times of {0}. Sorry.",
                                user.Name);
                        else
                        {
                            args.Player.SendSuccessMessage("{0} has played for {1}.", user.Name,
                                times[1].SToString());
                            args.Player.SendSuccessMessage("{0} has been registered for {1}.", user.Name,
                                times[0].SToString());
                            args.Player.SendSuccessMessage("{0} has logged in {1} times.", user.Name, logins2);
                        }
                        var seen = Statistics.database.GetLastSeen(user.ID);
                        if (seen == TimeSpan.MaxValue)
                            args.Player.SendErrorMessage("Unable to find {0}'s last login time.",
                                user.Name);
                        else
                            args.Player.SendSuccessMessage("{0} last logged in {1} ago.", user.Name, seen.SToString());
                    }
                    break;
			}
		}

		private static List<User> GetUsers(string username)
		{
			var users = TShock.Users.GetUsers();
			var ret = new List<User>();
			foreach (var user in users)
			{
				if (user.Name.Equals(username))
					return new List<User> {user};
				if (user.Name.StartsWith(username))
					ret.Add(user);
			}
			return ret;
		}
	}
}
