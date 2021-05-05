using System;
using ADOTrain.Repositories;

namespace ADOTrain
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }
                
            var repo = new SqlRepository();

            if (args[0] == "-init" && args.Length == 1)
            {
                repo.InitDb();
            }

            if (args[0] == "-lecture" && args.Length == 3)
            {
                if (DateTime.TryParse(args[1], out var date))
                {
                    Console.WriteLine(@"Rows affected " + repo.AddLecture(date,args[2]));
                }
            }

            if (args[0] == "-student" && args.Length == 2)
            {
                Console.WriteLine(@"Rows affected " + repo.AddStudent(args[1]));
            }

            if (args[0] == "-attend" && args.Length == 4)
            {
                if (DateTime.TryParse(args[2], out var date))
                {
                    if (int.TryParse(args[3], out var mark))
                    {
                        Console.WriteLine(@"Rows affected " + repo.AddAttend(args[1], date, mark));
                    }
                }
            }

            if (args[0] == "-report" && args.Length == 1)
            {
                var (attendance, students) = repo.GetReport();
                foreach (var item in attendance)
                {
                    Console.WriteLine($@"{item.LectureDate} {item.Topic}");
                    foreach (var student in item.Students)
                    {
                        Console.WriteLine(student);
                    }
                }
                Console.WriteLine(@"Missing students:");
                foreach (var student in students)
                {
                    Console.WriteLine(student);
                }
            }

            Console.Read();
        }
    }
}
