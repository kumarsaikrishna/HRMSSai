using System.Text;

namespace AttendanceCRM.Utilities
{
	public class RandomGenerator
	{
		public static int RandomNumber(int min, int max)
		{
			Random random = new Random();
			return random.Next(min, max);
		}

		// Generate a random string with a given size    
		public static string RandomString(int size, bool lowerCase)
		{
			StringBuilder builder = new StringBuilder();
			Random random = new Random();
			char ch;
			for (int i = 0; i < size; i++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
				builder.Append(ch);
			}
			if (lowerCase)
				return builder.ToString().ToLower();
			return builder.ToString();
		}

		// Generate a random password    
		public static string RandomPassword()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(RandomString(4, true));
			builder.Append(RandomNumber(1000, 9999));
			builder.Append(RandomString(2, false));
			return builder.ToString();
		}

		public static string GenerateToken(int? userId)
		{
			byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
			byte[] key = Guid.NewGuid().ToByteArray();
			string token = Convert.ToBase64String(time.Concat(key).ToArray());
			token += userId.ToString();
			string randomString = RandomString(10, true);
			token += randomString;
			return token;
		}
	}
}
